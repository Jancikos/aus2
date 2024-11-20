using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRI.AUS2.Libs.Helpers;

namespace FRI.AUS2.Libs.Structures.Files
{
    /// <summary>
    /// O(r, w) - pocet pristupov danych operacii ku blokom v subore
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class HeapFile<TData> : IBinaryData, IDisposable where TData : class, IHeapFileData, new()
    {
        protected BinaryFileManager _fileManager;
        public int FileSize => _fileManager.Length;
        public int BlockSize { get; private set; }
        public int BlocksCount => _fileManager.Length / BlockSize;
        public int ValidItemsCount => GetAllDataBlocks().Sum(b => b.ValidCount);

        public IList<TData> AllData
        {
            get
            {
                List<TData> data = new();

                foreach (var block in GetAllDataBlocks())
                {
                    data.AddRange(block.Items.Take(block.ValidCount));
                }

                return data;
            }
        }

        private int? _nextFreeBlock = null;
        public int? NextFreeBlock { get => _nextFreeBlock; protected set => _nextFreeBlock = value; }
        public int FreeBlocksCount
        {
            get => _countStackItems(NextFreeBlock);
        }
        public bool ManageFreeBlocks { get; set; } = true;

        private int? _nextEmptyBlock = null;
        public int? NextEmptyBlock { get => _nextEmptyBlock; protected set => _nextEmptyBlock = value; }
        public int EmptyBlocksCount
        {
            get => _countStackItems(NextEmptyBlock);
        }

        protected int ActiveBlockAddress { get; set; }
        public HeapFileBlock<TData> ActiveBlock { get; protected set; }
        /// <summary>
        /// if true, than method _saveActiveBlock will not do anything
        /// </summary>
        private bool _saveActiveBlockDisabled = false;

        public int Size => _fileManager.Length;

        public HeapFile(int blockSize, FileInfo file)
        {
            _fileManager = new BinaryFileManager(file);
            BlockSize = blockSize;

            ActiveBlock = new HeapFileBlock<TData>(BlockSize);

            if (!file.Exists || _fileManager.Length == 0) {
                // init structure metadata
                _saveMetadata();
            } else {
                // load metadata from first file block
                FromBytes(_fileManager.ReadBytes(0, BlockSize));
            }
        }

        public void Clear()
        {
            ActiveBlockAddress = 0;
            ActiveBlock.ResetBlock();

            NextFreeBlock = null;
            NextEmptyBlock = null;

            _saveMetadata();
            _fileManager.Truncate(BlockSize);
        }

        #region Insert
        /// <summary>
        /// O(1, 1) + operacie nad zasobnikmi
        /// do dokumentacie rozpisat zlozitosti podla jednotlivych operacii
        /// - do noveho bloku
        /// - do ciastocne volneho bloku
        ///    -  kt. sa naplni 
        ///    -  kt. sa nenaplni 
        /// </summary>
        /// <param name="data"></param>
        /// <returns>address of the block where the data was inserted</returns>
        public int Insert(TData data)
        {
            var (address, addressType) = _findAddressOfNextFreeBlock();

            _loadActiveBlock(address);
            ActiveBlock.AddItem(data);

            _postInsertToActiveBlock(addressType);

            return address;
        }

        public int InsertToBlock(int address, TData data)
        {
            _validateAddress(address);

            _loadActiveBlock(address);
            var addressType = _getBlockAddressType(address, ActiveBlock);
            if (addressType == BlockAdressType.FullBlock)
            {
                throw new InvalidOperationException("Block is full");
            }

            ActiveBlock.AddItem(data);

            _postInsertToActiveBlock(addressType);

            return address;
        }

        public void SetBlockItems(int address, TData[] items)
        {
            _validateAddress(address);

            _loadActiveBlock(address);

            if (ActiveBlock.BlockFactor < items.Length)
            {
                throw new InvalidOperationException("Items count is greater than block factor");
            }

            var addressType = _getBlockAddressType(address, ActiveBlock);

            ActiveBlock.Items = items;
            ActiveBlock.ValidCount = items.Length;

            _postInsertToActiveBlock(addressType);
        }

        /// <summary>
        /// it also saves the active block
        /// </summary>
        /// <param name="addressType">the state within the block was before inserting</param>
        /// <returns></returns>
        private void _postInsertToActiveBlock(BlockAdressType addressType)
        {
            _saveActiveBlockDisabled = true;
            switch (addressType)
            {
                case BlockAdressType.FreeBlock:
                    if (ActiveBlock.IsFull)
                    {
                        _dequeNextFreeBlock();
                    }
                    break;
                case BlockAdressType.EmptyBlock:
                    if (!ActiveBlock.IsEmpty)
                    {
                        _dequeNextEmptyBlock();
                    }

                    if (!ActiveBlock.IsFull)
                    {
                        _enqueNextFreeBlock();
                    }
                    break;
                case BlockAdressType.NewBlock:
                    // iba jeden zapis
                    if (!ActiveBlock.IsFull)
                    {
                        _enqueNextFreeBlock();
                    }
                    break;
                case BlockAdressType.FullBlock:
                    if (ActiveBlock.IsEmpty)
                    {
                        _enqueNextEmptyBlock();
                        break;
                    }

                    if (!ActiveBlock.IsFull)
                    {
                        _enqueNextFreeBlock();
                    }
                    break;
            }

            _saveActiveBlock(true);
        }

        public int CreateNewBlock()
        {
            _loadActiveBlock(_fileManager.Length);

            _saveActiveBlockDisabled = true;
            // lebo sa do noveho nepridali ziadne data
            _enqueNextEmptyBlock();

            _saveActiveBlock(true);

            return ActiveBlockAddress;
        }
        
        #endregion

        #region Find
        /// <summary>
        /// O(1, 0)
        /// </summary>
        /// <param name="address"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public TData? Find(int address, TData filter)
        {
            _validateAddress(address);

            _loadActiveBlock(address);

            return ActiveBlock.GetItem(filter);
        }
        #endregion

        #region Delete

        /// <summary>
        /// O(1, 1) + operacie nad zasobnikmi + mazanie prazdnych blokov na konci suboru
        /// 
        /// pozor na mazanie poslednych blokov
        ///   - pri mazani viacerych volnych blokov, ich mozem nacitat viacej 
        ///       - potom ich spracovat v ramke
        ///       - a naraz to zapisat
        ///   - nebude chyba to zapisovat aj na viac razy
        /// 
        /// <param name="address"></param>
        /// <param name="filter"></param>
        public void Delete(int address, TData filter)
        {
            _validateAddress(address);

            _loadActiveBlock(address);

            // to znamena ze bol v zozname volnych blokov
            bool wasInFreeBlockStack = ActiveBlock.IsChained || _nextFreeBlock == address; 

            bool itemDeleted = ActiveBlock.RemoveItem(filter);
            if (!itemDeleted)
            {
                // item not found
                throw new InvalidOperationException("Item not found");
            }

            // ak ostal plny, tak problem
            if (ActiveBlock.IsFull)
            {
                throw new InvalidOperationException("Block cannot be full after deletion");
            }

            _saveActiveBlockDisabled = true;

            // ak je ciastocne naplneny
            if (!ActiveBlock.IsEmpty) 
            {
                // ak nebol v zozname volnych blokov, tak ho tam pridame
                if (!wasInFreeBlockStack)
                {
                    _enqueNextFreeBlock();
                }

                _saveActiveBlock(true);
            }

            // ak je prazdny
            if (ActiveBlock.IsEmpty)
            {
                // ak bol v zozname volnych blokov, tak ho odstranime
                if (wasInFreeBlockStack)
                {
                    _dequeNextFreeBlock();
                }

                // ak je to posledny blok, tak spustime proces mazania blokov od konca
                if (address == _getAddressByBlockIndex(BlocksCount - 1))
                {
                    _deleteEmptyBlocksFromEnd();
                    return;
                } 

                // zaradime ho do zoznamu prazdnych blokov
                _enqueNextEmptyBlock();

                _saveActiveBlock(true);
            }
        }


        #endregion

        #region Blocks management

        public HeapFileBlock<TData> GetBlock(int address)
        {
            _validateAddress(address);

            return _loadBlock(address);
        }
        public List<HeapFileBlock<TData>> GetAllDataBlocks()
        {
            List<HeapFileBlock<TData>> blocks = new();

            for (int i = 1; i < BlocksCount; i++)
            {
                HeapFileBlock<TData> block = new(BlockSize);
                block.FromBytes(_fileManager.ReadBytes(i * BlockSize, BlockSize));
                blocks.Add(block);
            }

            return blocks;
        }

        private void _validateAddress(int address)
        {
            if (address < 0 || address >= _fileManager.Length)
            {
                throw new IndexOutOfRangeException("Address out of range");
            }

            if (address % BlockSize != 0)
            {
                throw new InvalidOperationException("Address is not valid address of the block beginning");
            }
        }

        private BlockAdressType _getBlockAddressType(int adrress, HeapFileBlock<TData> block)
        {
            if (adrress == FileSize)
            {
                return BlockAdressType.NewBlock;
            }

            if (block.IsEmpty)
            {
                return BlockAdressType.EmptyBlock;
            }

            if (block.IsFull)
            {
                return BlockAdressType.FullBlock;
            }

            return BlockAdressType.FreeBlock;
        }

        /// <summary>
        /// O(n , 1) + operacie nad zasobnikmi, kde n je pocet prazdnych blokov na konci suboru
        /// </summary>
        private void _deleteEmptyBlocksFromEnd()
        {
            int lastBlockIndex = BlocksCount - 1;
            int lastBlockAddress = _getAddressByBlockIndex(lastBlockIndex);
            int lastDeletedBlockAddress = lastBlockAddress;

            while (lastBlockIndex > 0)
            {
                _loadActiveBlock(lastBlockAddress);

                if (!ActiveBlock.IsEmpty)
                {
                    break;
                }

                _dequeNextEmptyBlock();
                lastDeletedBlockAddress = lastBlockAddress;

                lastBlockIndex--;
                lastBlockAddress = _getAddressByBlockIndex(lastBlockIndex);
            }

            // if (lastDeletedBlockAddress == lastBlockAddress)
            // {
            //     // no empty blocks from end found
            //     return;
            // }

            _fileManager.Truncate(lastDeletedBlockAddress);
        }

        private int _countStackItems(int? startAddress)
        {
            int count = 0;

            int? nextBlockAddress = startAddress;
            while (nextBlockAddress is not null)
            {
                count++;
                nextBlockAddress = _loadBlock(nextBlockAddress.Value).NextBlock;
            }

            return count;
        }

        /// <summary>
        /// O(0, 1) - v pripade prazdneho zasobnika
        /// O(1, 2) - v pripade neprazdneho zasobnika
        /// </summary>
        /// <param name="queueStartAddress"></param>
        private void _enqueActiveBlock(ref int? queueStartAddress)
        {
            if (ActiveBlock.IsChained || queueStartAddress == ActiveBlockAddress)
            {
                throw new InvalidOperationException("Cannot enqueue block that is already in stack");
            }

            // enquing block
            ActiveBlock.NextBlock = queueStartAddress;
            ActiveBlock.PreviousBlock = null;
            _saveActiveBlock();

            // set new previous block
            if (queueStartAddress is not null)
            {
                var oldStartBlock = _loadBlock(queueStartAddress.Value);
                oldStartBlock.PreviousBlock = ActiveBlockAddress;
                _saveBlock(queueStartAddress.Value, oldStartBlock);
            }

            // setting new next free block
            queueStartAddress = ActiveBlockAddress;
            // _saveMetadata();
        }

        /// <summary>
        /// O(2, 3) - v pripade ze ma aktivny blok predchadzajuci a nasledujuci blok
        /// O(1, 2) - v pripade ze ma aktivny blok len predchadzajuci/nasledujuci blok
        /// O(0, 1) - v pripade ze nema aktivny blok predchadzajuci/nasledujuci blok
        /// </summary>
        /// <param name="queueStartAddress"></param>
        private void _dequeActiveBlock(ref int? queueStartAddress)
        {
            var prevBlock = _loadBlock(ActiveBlock.PreviousBlock);
            var nextBlock = _loadBlock(ActiveBlock.NextBlock);

            // setting new queue start address
            if (queueStartAddress == ActiveBlockAddress)
            {
                queueStartAddress = ActiveBlock.NextBlock;
                // _saveMetadata();
            }

            // dequeing block
            if (prevBlock is not null)
            {
                prevBlock.NextBlock = ActiveBlock.NextBlock;
                _saveBlock(ActiveBlock.PreviousBlock!.Value, prevBlock);
            }
            if (nextBlock is not null)
            {
                nextBlock.PreviousBlock = ActiveBlock.PreviousBlock;
                _saveBlock(ActiveBlock.NextBlock!.Value, nextBlock);
            }

            if (prevBlock is not null || nextBlock is not null)
            {
                ActiveBlock.PreviousBlock = null;
                ActiveBlock.NextBlock = null;

                _saveActiveBlock();
            }
        }

        private void _enqueNextFreeBlock()
        {
            if (!ManageFreeBlocks) 
            {
                return;
            }

            _enqueActiveBlock(ref _nextFreeBlock);
        }

        private void _dequeNextFreeBlock()
        {
            if (!ManageFreeBlocks) 
            {
                return;
            }

            _dequeActiveBlock(ref _nextFreeBlock);
        }

        private void _enqueNextEmptyBlock()
        {
            _enqueActiveBlock(ref _nextEmptyBlock);
        }

        private void _dequeNextEmptyBlock()
        {
            _dequeActiveBlock(ref _nextEmptyBlock);
        }

        private void _loadActiveBlock(int address)
        {
            if (ActiveBlockAddress == address)
            {
                return;
            }

            ActiveBlock.FromBytes(_fileManager.ReadBytes(address, BlockSize));
            ActiveBlockAddress = address;
        }
        private HeapFileBlock<TData> _loadBlock(int address)
        {
            HeapFileBlock<TData> block = new(BlockSize);
            block.FromBytes(_fileManager.ReadBytes(address, BlockSize));

            return block;
        }
        private HeapFileBlock<TData>? _loadBlock(int? address)
        {
            if (address is null)
            {
                return null;
            }

            return _loadBlock(address.Value);
        }

        public void _saveBlock(int address, HeapFileBlock<TData> block)
        {
            _fileManager.WriteBytes(address, block.ToBytes());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="force">saves even _saveActiveBlockDisabled is true, also sets _saveActiveBlockDisabled to false</param>
        private void _saveActiveBlock(bool force = false)
        {
            if (_saveActiveBlockDisabled) 
            {
                if (!force) {
                    return; // do not save if disabled
                }
                _saveActiveBlockDisabled = false;
            }

            _saveBlock(ActiveBlockAddress, ActiveBlock);
        }

        private (int address, BlockAdressType adressType) _findAddressOfNextFreeBlock()
        {
            if (NextFreeBlock is not null)
            {
                return (NextFreeBlock.Value, BlockAdressType.FreeBlock);
            }

            if (NextEmptyBlock is not null)
            {
                return (NextEmptyBlock.Value, BlockAdressType.EmptyBlock);
            }

            return (_fileManager.Length, BlockAdressType.NewBlock);
        }

        public int _getAddressByBlockIndex(int index)
        {
            return index * BlockSize;
        }


        #endregion

        #region Bytes conversion

        protected void _saveMetadata()
        {
            _fileManager.WriteBytes(0, ToBytes());
        }

        /// <summary>
        /// saves metadata to the bytes array
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            byte[] buffer = new byte[BlockSize];
            int offset = 0;

            BitConverter.GetBytes(NextFreeBlock ?? -1).CopyTo(buffer, offset);
            offset += sizeof(int);

            BitConverter.GetBytes(NextEmptyBlock ?? -1).CopyTo(buffer, offset);
            offset += sizeof(int);

            return buffer;
        }

        public void FromBytes(byte[] bytes)
        {
            int offset = 0;

            int nextFreeBlock = BitConverter.ToInt32(bytes, offset);
            NextFreeBlock = (nextFreeBlock == -1 || nextFreeBlock ==  0) ? null : nextFreeBlock;
            offset += sizeof(int);

            int nextEmptyBlock = BitConverter.ToInt32(bytes, offset);
            NextEmptyBlock = (nextEmptyBlock == -1 || nextEmptyBlock ==  0) ? null : nextEmptyBlock;
            offset += sizeof(int);
        }

        public void Dispose()
        {
            _saveMetadata();
        }
        #endregion
    }

    #region HeapFileBlock

    public enum BlockAdressType
    {
        FreeBlock,
        EmptyBlock,
        NewBlock,
        FullBlock
    }

    public class HeapFileBlock<TData> : IBinaryData where TData : class, IHeapFileData, new()
    {
        /// <summary>
        /// count of items that can be stored in the block
        /// </summary>
        /// <value></value>
        public int BlockFactor
        {
            get => Size / (TDataSize + 1);
        }

        /// <summary>
        /// number of valid items in the block (from the beginning of the block)
        /// </summary>
        /// <value></value>
        public int ValidCount { get;  set; }

        /// <summary>
        /// items stored in the block
        /// </summary>
        /// <value></value>
        public TData[] Items { get; set; }
        public TData[] ValidItems => Items.Take(ValidCount).ToArray();

        public int? PreviousBlock { get; set; }
        public int? NextBlock { get; set; }
        public bool IsChained => PreviousBlock is not null || NextBlock is not null;

        public int MetedataSize => 3 * sizeof(int);
        public int TDataSize =>  (new TData()).Size;
        public int DataSize => BlockFactor * TDataSize;

        private int _blockSize;
        /// <summary>
        /// size of the whole block in bytes
        /// </summary>
        /// <value></value>
        public int Size => _blockSize;

        public HeapFileBlock(int blockSize)
        {
            _blockSize = blockSize;

            Items = new TData[BlockFactor];
        }

        public void ResetBlock()
        {
            ValidCount = 0;
            Items = new TData[BlockFactor];
            PreviousBlock = null;
            NextBlock = null;
        }

        #region Items

        public bool IsFull => ValidCount >= BlockFactor;
        public bool IsEmpty => ValidCount == 0;

        public void AddItem(TData item)
        {
            if (ValidCount >= BlockFactor)
            {
                throw new InvalidOperationException("Block is full");
            }

            Items[ValidCount++] = item;
        }

        public TData? GetItem(TData filter)
        {
            for (int i = 0; i < ValidCount; i++)
            {
                if (filter.Equals(Items[i]))
                {
                    return Items[i];
                }
            }

            return null;
        }

        public bool RemoveItem(TData filter)
        {
            for (int i = 0; i < ValidCount; i++)
            {
                if (filter.Equals(Items[i]))
                {
                    RemoveItem(i);
                    return true;
                }
            }

            return false;
        }

        public void RemoveItem(int index)
        {
            if (index < 0 || index >= ValidCount)
            {
                throw new IndexOutOfRangeException("Index out of range");
            }

            for (int i = index; i < ValidCount - 1; i++)
            {
                Items[i] = Items[i + 1];
            }

            ValidCount--;
        }


        #endregion

        #region Bytes conversion

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[Size];
            int offset = 0;

            // metadata
            BitConverter.GetBytes(ValidCount).CopyTo(buffer, offset);
            offset += sizeof(int);
                        
            BitConverter.GetBytes(PreviousBlock ?? -1).CopyTo(buffer, offset);
            offset += sizeof(int);

            BitConverter.GetBytes(NextBlock ?? -1).CopyTo(buffer, offset);
            offset += sizeof(int);

            // data
            for (int i = 0; i < ValidCount; i++)
            {
                Items[i].ToBytes().CopyTo(buffer, offset);

                // if (i < ValidCount)
                // {
                //     Items[i].ToBytes().CopyTo(buffer, offset);
                // }
                // else
                // {
                //     new TData().ToBytes().CopyTo(buffer, offset);
                // }

                offset += TDataSize;
            }

            return buffer;
        }

        public void FromBytes(byte[] bytes)
        {
            ResetBlock();
            int offset = 0;

            // metadata
            ValidCount = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);

            var prevBlock = BitConverter.ToInt32(bytes, offset);
            PreviousBlock = (prevBlock == -1 || prevBlock == 0) ? null : prevBlock;
            offset += sizeof(int);

            var nextBlock = BitConverter.ToInt32(bytes, offset);
            NextBlock = (nextBlock == -1 || nextBlock == 0) ? null : nextBlock;
            offset += sizeof(int);

            // data
            for (int i = 0; i < ValidCount; i++)
            {
                TData item = new();
                item.FromBytes(bytes[offset..(offset + TDataSize)]);
                Items[i] = item;

                offset += TDataSize;
            }
        }
        #endregion
    }
    #endregion
}
