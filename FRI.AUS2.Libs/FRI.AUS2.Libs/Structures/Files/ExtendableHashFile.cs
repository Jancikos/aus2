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
    public class ExtendableHashFile<TData> where TData : class, IExtendableHashFileData, new()
    {
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

        public ExtendableHashFile(FileInfo file)
        {
            // todo vythianut aj block size 
            _heapFile = new HeapFile<TData>(500, file)
            {
                ManageFreeBlocks = false,
                DeleteEmptyBlocksFromEnd = false,
                EnqueueNewBlockToEmptyBlocks = false
            };
            _heapFile.Clear();
            
            _initializeAddresses();
        }

        #region Insert
        public void Insert(TData data)
        {
            var hash = data.GetHash();

            bool inserted = false;
            do {
                int addressIndex = _getAddressIndex(hash);
                var dhfBlock = _addresses[addressIndex];

                try {
                    _heapFile.InsertToBlock(dhfBlock.Address, data);
                    inserted = true;
                } catch (InvalidOperationException) {
                    // block is full
                    // split block
                    
                    // blok uz je maximalnej hlbky, tak sa hlabka struktury musi zvacsit aby sa mohol blok rozdelit
                    if (dhfBlock.BlockDepth == Depth) {
                        _increaseDepth();
                    }

                    // rozdel blok
                    _splitBlock(addressIndex);

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
            if (block.IsEmpty)
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
        #endregion

        #region Delete
        public void Delete(TData filter)
        {
            var hash = filter.GetHash();
            int deletionIndex = _getAddressIndex(hash);
            var deletionHashBlock = _addresses[deletionIndex];

            _heapFile.Delete(deletionHashBlock.Address, filter);

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
            if (deletionBlock.ValidCount + siblingBlock.ValidCount <= _heapFile.BlockSize)
            {
                // merge blocks into one
                foreach (var item in deletionBlock.ValidItems)
                {
                    siblingBlock.AddItem(item);
                }
                _heapFile._saveBlock(siblingHashBlock.Address, siblingBlock);

                // delete deletion block items
                deletionBlock.ClearItems();

                // TODO - doplnit to aby to bolo cyklicke...
            }

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

            _heapFile._saveBlock(deletionAddress, deletionBlock);
        }
        #endregion

        #region Management

        private bool _hasBlockWithStrucuteDepth()
        {
            return _addresses.Any(a => a.BlockDepth == Depth);
        }

        private void _initializeAddresses()
        {
            _increaseDepth();
            
            _addresses[0] = new ExtendableHashFileBlock<TData>(_heapFile.GetEmptyBlock(), _heapFile);
            _addresses[1] = new ExtendableHashFileBlock<TData>(_heapFile.GetEmptyBlock(), _heapFile);
        }

        public int _getAddressIndex(BitArray hash) => _getAddressIndex(hash, Depth);
        public int _getAddressIndex(BitArray hash, int depth)
        {
            var mask = new BitArray(hash.Length);
            for (int i = 0; i < depth; i++)
            {
                mask[i] = true;
            }

            BitArray result = hash.And(mask);

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
                int newIndexBase = i;
                var adressBlock = _addresses[i];
                
                var newAddress = newAddresses[i] = new ExtendableHashFileBlock<TData>(adressBlock.Address, _heapFile) 
                {
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

            _depth++;
            ExtendableHashFileBlock<TData>[] newAddresses = new ExtendableHashFileBlock<TData>[(int)Math.Pow(2, Depth)];

            for (int i = 0; i < _addresses.Length; i++)
            {
                int newIndexBase = i;
                var adressBlock = _addresses[i];

                // for (int j = 0; j < 2; j++)
                foreach(int offset in new int[]{
                    0, 
                    (int)Math.Pow(2, Depth - 1)
                })
                {
                    newAddresses[newIndexBase + offset] = new ExtendableHashFileBlock<TData>(adressBlock.Address, _heapFile) 
                    {
                         BlockDepth = adressBlock.BlockDepth
                    };
                }
            }

            _addresses = newAddresses;
        }

        private void _splitBlock(int splittingBlockIndex)
        {
            var splittingIndex = splittingBlockIndex % (int)Math.Pow(2, _addresses[splittingBlockIndex].BlockDepth);
            var splittingBlock = _addresses[splittingIndex];
            var newBlockDepth = splittingBlock.BlockDepth + 1;
            var targetBlock = _addresses[splittingIndex + (int)Math.Pow(2, splittingBlock.BlockDepth)];
            // create new block
            targetBlock.Address = _heapFile.GetEmptyBlock();

            // try reinsert items
            var items = splittingBlock.Block.ValidItems;
            var splittingBlockItems = items.ToList();
            var targetBlockItems = new List<TData>();
            foreach (var item in items)
            {
                var hash = item.GetHash();
                int newIndex = _getAddressIndex(hash, newBlockDepth); // tuto pozor

                if (newIndex == splittingIndex)
                {
                    // item stays in the same block
                    continue;
                }

                // do tychto zoznamov sa to uklada kvoli odlozenemu zapisu do suboru
                splittingBlockItems.Remove(item);
                targetBlockItems.Add(item);
            }

            if (splittingBlockItems.Count != 0)
            {
                // zapise sa zmena do suboru
                _heapFile.SetBlockItems(splittingBlock.Address, splittingBlockItems.ToArray());
                _heapFile.SetBlockItems(targetBlock.Address, targetBlockItems.ToArray());
            }

            // update block depths
            splittingBlock.BlockDepth = newBlockDepth;
            targetBlock.BlockDepth = newBlockDepth;
        }

        #endregion
    }

    public class ExtendableHashFileBlock<TData> where TData : class, IExtendableHashFileData, new()
    {
        public int Address { get; set; }
        public int BlockDepth { get; set; } = 1;
        public HeapFileBlock<TData> Block => _heapFile.GetBlock(Address);

        private HeapFile<TData> _heapFile;

        public ExtendableHashFileBlock(int address, HeapFile<TData> heapFile)
        {
            Address = address;
            _heapFile = heapFile;
        }

        public override string ToString()
        {
            return $"[{BlockDepth}] {Address}";
        }
    }
}