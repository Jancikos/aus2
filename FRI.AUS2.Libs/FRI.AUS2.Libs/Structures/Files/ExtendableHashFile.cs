using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FRI.AUS2.Libs.Helpers;

namespace FRI.AUS2.Libs.Structures.Files
{
    public class ExtendableHashFile<TData> : IBinaryData, IDisposable where TData : class, IExtendableHashFileData, new()
    {
        protected BinaryFileManager _fileManager;

        private int _depth = 0;
        public int Depth => _depth;

        /// <summary>
        /// array of addresses to the blocks
        /// </summary>
        /// <returns></returns>
        private ExtendableHashFileBlock<TData>[] _addresses = [];
        public int AddressesCount => _addresses.Length;
        public ExtendableHashFileBlock<TData>[] Addresses => _addresses;

        private HeapFile<TData> _heapFile;
        public HeapFile<TData> HeapFile => _heapFile;

        public int Size => sizeof(int) + _addresses.Length * new ExtendableHashFileBlock<TData>(_heapFile).Size;

        public ExtendableHashFile(int blockSize, Uri dataFolder, string fileNamesPrefix = "ehf")
        {
            _heapFile = new HeapFile<TData>(blockSize, new($"{dataFolder.LocalPath}{fileNamesPrefix}-data.bin"))
            {
                ManageFreeBlocks = false,
                DeleteEmptyBlocksFromEnd = false,
                EnqueueNewBlockToEmptyBlocks = false
            };


            _fileManager = new BinaryFileManager(new($"{dataFolder.LocalPath}{fileNamesPrefix}-meta.bin"));

            _initialize();
        }

        #region Insert
        public void Insert(TData data)
        {
            var hash = data.GetHash();

            bool inserted;
            do
            {
                int addressIndex = _getAddressIndex(hash);
                var ehfBlock = _addresses[addressIndex];

                if (ehfBlock.Address is null)
                {
                    ehfBlock.Address = _heapFile.GetEmptyBlock();
                    _updateAddressOfAllBlocksInGoup(addressIndex, ehfBlock);
                }

                try
                {
                    _heapFile.InsertToBlock(ehfBlock.Address.Value, data);
                    inserted = true;
                }
                catch (InvalidOperationException)
                {
                    // block is full
                    // split block

                    // blok uz je maximalnej hlbky, tak sa hlabka struktury musi zvacsit aby sa mohol blok rozdelit
                    if (ehfBlock.BlockDepth == Depth)
                    {
                        _increaseDepth();

                        addressIndex = _getAddressIndex(hash);
                        ehfBlock = _addresses[addressIndex];
                    }

                    // rozdel blok
                    _splitBlock(addressIndex, ehfBlock.BlockDepth);

                    inserted = false;
                }
            } while (!inserted);
        }
        #endregion

        #region Find
        public TData Find(TData filter)
        {
            var hash = filter.GetHash();
            int addressIndex = _getAddressIndex(hash);

            var block = _addresses[addressIndex].Block;
            if (block is null || block.IsEmpty)
            {
                throw new KeyNotFoundException("Block is empty");
            }

            var data = block.GetItem(filter);
            if (data is null)
            {
                throw new KeyNotFoundException("Data not found");
            }

            return data;
        }

        private void _updateAddressOfAllBlocksInGoup(int addressIndex, ExtendableHashFileBlock<TData> block)
        {
            var groupSize = (int)Math.Pow(2, Depth - block.BlockDepth);
            var groupStartIndex = addressIndex - (addressIndex % groupSize);

            for (int i = 0; i < groupSize; i++)
            {
                _addresses[groupStartIndex + i].Address = block.Address;
            }
        }

        #endregion

        #region Delete
        public void Delete(TData filter)
        {
            var hash = filter.GetHash();
            int deletionIndex = _getAddressIndex(hash);
            var deletionHashBlock = _addresses[deletionIndex];

            // _heapFile.Delete(deletionHashBlock.Address, filter);

            // var deletionBlock = _heapFile.ActiveBlock;
            var deletionBlock = new HeapFileBlock<TData>(_heapFile.BlockSize);
            deletionBlock.FromBytes(_heapFile.ActiveBlock.ToBytes()); // aby sa tam nakopiaoval cely objekt, nie len odkaz nan
            var deletionAddress = deletionHashBlock.Address;

            var siblingIndex = deletionIndex % (int)Math.Pow(2, deletionHashBlock.BlockDepth - 1);
            if (siblingIndex == deletionIndex)
            {
                siblingIndex += (int)Math.Pow(2, deletionHashBlock.BlockDepth - 1);
            }

            // check if can be merged
            var siblingHashBlock = _addresses[siblingIndex];
            var siblingBlock = siblingHashBlock.Block;
            // TODO - items in deletion block can be moved to sibling block
            // if (deletionBlock.ValidCount + siblingBlock.ValidCount <= _heapFile.BlockSize)
            // {
            //     // merge blocks into one
            //     foreach (var item in deletionBlock.ValidItems)
            //     {
            //         siblingBlock.AddItem(item);
            //     }
            //     // _heapFile._saveBlock(siblingHashBlock.Address, siblingBlock);

            //     // delete deletion block items
            //     deletionBlock.ClearItems();

            //     // TODO - doplnit to aby to bolo cyklicke...
            // }

            // HLAVNE TO UROBIT PODLA MATERIALOV !!!

            // check if deletion block is empty
            if (deletionBlock.IsEmpty && deletionHashBlock.BlockDepth > 1)
            {
                if (siblingHashBlock.BlockDepth == deletionHashBlock.BlockDepth)
                {
                    // merge blocks
                    deletionHashBlock.Address = siblingHashBlock.Address;

                    deletionHashBlock.BlockDepth--;
                    siblingHashBlock.BlockDepth--;

                    // shrink file size
                    _heapFile._deleteEmptyBlocksFromEnd(true);


                    if (deletionHashBlock.BlockDepth + 1 == Depth)
                    {
                        if (!_hasBlockWithStrucuteDepth())
                        {
                            _decreaseDepth();
                        }
                    }
                }
            }

            // _heapFile._saveBlock(deletionAddress, deletionBlock);
        }
        #endregion

        #region Update

        public void Update(TData filter, TData newData)
        {
            if (!filter.GetHash().IsSameAs(newData.GetHash()))
            {
                throw new InvalidOperationException("Cannot update item with different hash");
            }

            var hash = filter.GetHash();
            int addressIndex = _getAddressIndex(hash);

            var ehfBlock = _addresses[addressIndex];
            var block = ehfBlock.Block;

            if (ehfBlock.Address is null || block is null || !block.RemoveItem(filter))
            {
                throw new KeyNotFoundException("Item not found");
            }

            block.AddItem(newData);

            _heapFile._saveBlock(ehfBlock.Address.Value, block);
        }

        #endregion

        #region Management

        private bool _hasBlockWithStrucuteDepth()
        {
            return _addresses.Any(a => a.BlockDepth == Depth);
        }

        private void _initialize()
        {
            // init from if file is not empty
            if (!_fileManager.IsEmpty)
            {
                FromBytes(_fileManager.ReadAllBytes());
                return;
            }

            // init from scratch
            _initializeFromScratch();
        }

        private void _initializeFromScratch()
        {
            _depth = 1;

            _addresses = new ExtendableHashFileBlock<TData>[(int)Math.Pow(2, Depth)];
            _addresses[0] = new ExtendableHashFileBlock<TData>(_heapFile);
            _addresses[1] = new ExtendableHashFileBlock<TData>(_heapFile);
        }

        public int _getAddressIndex(BitArray hash) => _getAddressIndex(hash, Depth);

        public int _getAddressIndex(BitArray hash, int depth)
        {
            if (hash.Length > 32)
            {
                throw new InvalidOperationException("Hash length have to be <= 32");
            }

            // todo - vhodne pridat ako parameter ci sa ma hash reverznut alebo nie
            hash = hash.ReverseBits();

            var mask = new BitArray(hash.Length);
            for (int i = 0; i < depth; i++)
            {
                mask[hash.Length - 1 - i] = true;
            }

            BitArray hashShrinked = hash.And(mask);

            BitArray result = hashShrinked.RightShift(hash.Length - depth);

            int[] array = new int[1];
            result.CopyTo(array, 0);
            return array[0];
        }

        public void _decreaseDepth()
        {
            if (Depth <= 1)
            {
                throw new InvalidOperationException("Cannot decrease depth below 1");
            }

            _depth--;
            ExtendableHashFileBlock<TData>[] newAddresses = new ExtendableHashFileBlock<TData>[(int)Math.Pow(2, Depth)];

            for (int i = 0; i < newAddresses.Length; i++)
            {
                var adressBlock = _addresses[i];

                newAddresses[i] = new ExtendableHashFileBlock<TData>(_heapFile)
                {
                    Address = adressBlock.Address,
                    BlockDepth = adressBlock.BlockDepth
                };
            }

            _addresses = newAddresses;
        }

        public void _increaseDepth()
        {
            if (Depth >= 32)
            {
                throw new InvalidOperationException("Cannot increase depth above 32");
            }

            Debug.WriteLine($"Increasing depth from {Depth} to {Depth + 1}");

            _depth++;
            ExtendableHashFileBlock<TData>[] newAddresses = new ExtendableHashFileBlock<TData>[(int)Math.Pow(2, Depth)];

            int newAddressesIndex = 0;
            for (int i = 0; i < _addresses.Length; i++)
            {
                int newIndexBase = i;
                var adressBlock = _addresses[i];

                2.Repeat(() =>
                {
                    newAddresses[newAddressesIndex++] = new ExtendableHashFileBlock<TData>(_heapFile)
                    {
                        Address = adressBlock.Address,
                        BlockDepth = adressBlock.BlockDepth
                    };
                });
            }

            _addresses = newAddresses;
        }

        private void _splitBlock(int baseSplittingBlockIndex, int baseSplittingBlockDepth)
        {
            var depthsDifference = Depth - baseSplittingBlockDepth;
            var actualGroupSize = (int)Math.Pow(2, depthsDifference);
            var newGroupSize = (int)Math.Pow(2, depthsDifference - 1);

            // aby splittingBlockIndex ukazoval na prvy blok v adresari s danou hlbkou
            var splittingBlockIndex = baseSplittingBlockIndex - (baseSplittingBlockIndex % actualGroupSize);
            var splittingBlock = _addresses[splittingBlockIndex];

            var newBlockDepth = splittingBlock.BlockDepth + 1;

            var targetBlockIndex = splittingBlockIndex + newGroupSize;
            var targetBlock = _addresses[targetBlockIndex];

            // debug
            Debug.WriteLine($"BASE Splitting index: {baseSplittingBlockIndex}");
            Debug.WriteLine($"Splitting index: {splittingBlockIndex} [{splittingBlock}]");
            Debug.WriteLine($"Target index: {targetBlockIndex} [{targetBlock}]");

            // try reinsert items
            var items = splittingBlock.Block?.ValidItems ?? throw new InvalidOperationException("Splitting block does not have reference to heap file block");
            var splittingBlockItems = items.ToList();
            var targetBlockItems = new List<TData>();
            foreach (var item in items)
            {
                var hash = item.GetHash();
                int newIndexOrig = _getAddressIndex(hash);
                int newIndex = newIndexOrig - (newIndexOrig % newGroupSize);

                if (newIndex == splittingBlockIndex)
                {
                    // item stays in the same block
                    continue;
                }

                if (targetBlockIndex != newIndex)
                {
                    // todo - toto by sa nemalo stat, no dobre to nechat na debug kontrolu
                    throw new InvalidOperationException("Invalid target block index durring split");
                }

                // do tychto zoznamov sa to uklada kvoli odlozenemu zapisu do suboru
                splittingBlockItems.Remove(item);
                targetBlockItems.Add(item);
            }

            // update block depths
            splittingBlock.BlockDepth = newBlockDepth;
            targetBlock.BlockDepth = newBlockDepth;

            if (targetBlockItems.Count != 0)
            {
                // ak splittovany blok ostal prazdny, tak targetu nechaj povodnu adresu a splittovany blok bude mat novu adresu
                if (splittingBlockItems.Count == 0)
                {
                    splittingBlock.Address = null;
                }

                if (splittingBlockItems.Count != 0)
                {
                    // zapise sa zmena do suboru
                    if (splittingBlock.Address is null)
                    {
                        throw new InvalidOperationException("Splitting block address is not set even after split");
                    }
                    _heapFile.SetBlockItems(splittingBlock.Address.Value, splittingBlockItems.ToArray());

                    targetBlock.Address = _heapFile.GetEmptyBlock();
                    _heapFile.SetBlockItems(targetBlock.Address.Value, targetBlockItems.ToArray());
                }
            } else
            {
                targetBlock.Address = null;
            }

            // update group blocks
            for (int i = 0; i < newGroupSize; i++)
            {
                _addresses[splittingBlockIndex + i].Address = splittingBlock.Address;
                _addresses[splittingBlockIndex + i].BlockDepth = newBlockDepth;
            
                _addresses[targetBlockIndex + i].Address = targetBlock.Address;
                _addresses[targetBlockIndex + i].BlockDepth = newBlockDepth;
            }
        }

        public byte[] ToBytes()
        {
            var bytes = new List<byte>();

            // write depth
            bytes.AddRange(BitConverter.GetBytes(Depth));

            // write addresses
            foreach (var address in _addresses)
            {
                bytes.AddRange(address.ToBytes());
            }

            return bytes.ToArray();
        }

        public void FromBytes(byte[] bytes)
        {
            int offset = 0;

            // read depth
            _depth = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);

            // read addresses
            var addressesCount = (int)Math.Pow(2, Depth);
            var ehfBlockSize = new ExtendableHashFileBlock<TData>(_heapFile).Size;
            _addresses = new ExtendableHashFileBlock<TData>[addressesCount];
            for (int i = 0; i < addressesCount; i++)
            {
                var address = new ExtendableHashFileBlock<TData>(_heapFile);
                address.FromBytes(bytes[offset..(offset + ehfBlockSize)]);
                _addresses[i] = address;

                offset += address.Size;
            }
        }

        public void Clear()
        {
            _heapFile.Clear();
            _initializeFromScratch();
        }

        public void Dispose()
        {
            _fileManager.WriteBytes(0, ToBytes());
            _fileManager.Dispose();

            _heapFile.Dispose();
        }
        #endregion
    }

    #region ExtendableHashFileBlock
    public class ExtendableHashFileBlock<TData> : IBinaryData where TData : class, IExtendableHashFileData, new()
    {
        public int? Address { get; set; } = null;
        public int BlockDepth { get; set; } = 1;
        public HeapFileBlock<TData>? Block =>
            Address is null
            ? null
            : _heapFile.GetBlock(Address.Value);

        public int Size => 2 * sizeof(int);

        private HeapFile<TData> _heapFile;

        public ExtendableHashFileBlock(HeapFile<TData> heapFile)
        {
            _heapFile = heapFile;
        }

        public override string ToString()
        {
            return $"[{BlockDepth}] {Address.ToString() ?? "NULL"}";
        }

        public byte[] ToBytes()
        {
            var bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(Address ?? -1));
            bytes.AddRange(BitConverter.GetBytes(BlockDepth));

            return bytes.ToArray();
        }

        public void FromBytes(byte[] bytes)
        {
            int offset = 0;

            int address = BitConverter.ToInt32(bytes, offset);
            Address = address == -1 ? null : address;
            offset += sizeof(int);

            BlockDepth = BitConverter.ToInt32(bytes, offset);
        }
    }
    #endregion
}