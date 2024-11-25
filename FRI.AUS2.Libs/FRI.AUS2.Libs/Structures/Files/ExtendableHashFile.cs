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

        public ExtendableHashFile(int blockSize, FileInfo file)
        {
            _heapFile = new HeapFile<TData>(blockSize, file)
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

            bool inserted;
            do
            {
                int addressIndex = _getAddressIndex(hash);
                var ehfBlock = _addresses[addressIndex];

                try
                {
                    ehfBlock.InsertToBlock(data);
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

                newAddresses[i] = new ExtendableHashFileBlock<TData>(adressBlock.Address, _heapFile)
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
                    newAddresses[newAddressesIndex++] = new ExtendableHashFileBlock<TData>(adressBlock.Address, _heapFile)
                    {
                        BlockDepth = adressBlock.BlockDepth
                    };
                });
            }

            _addresses = newAddresses;
        }

        private void _splitBlock(int baseSplittingBlockIndex, int baseSplittingBlockDepth)
        {
            var depthsDifference = Depth - baseSplittingBlockDepth;

            // aby splittingBlockIndex ukazoval na prvy blok v adresari s danou hlbkou
            var splittingBlockIndex = baseSplittingBlockIndex - (baseSplittingBlockIndex % (int)Math.Pow(2, depthsDifference));
            var splittingBlock = _addresses[splittingBlockIndex];

            var newBlockDepth = splittingBlock.BlockDepth + 1;

            var targetBlockIndex = splittingBlockIndex + 1;
            var targetBlock = _addresses[targetBlockIndex];

            Debug.WriteLine($"BASE Splitting index: {baseSplittingBlockIndex}");
            Debug.WriteLine($"Splitting index: {splittingBlockIndex} [{splittingBlock}]");
            Debug.WriteLine($"Target index: {targetBlockIndex} [{targetBlock}]");

            // try reinsert items
            var items = splittingBlock.Block.ValidItems;
            var splittingBlockItems = items.ToList();
            var targetBlockItems = new List<TData>();
            foreach (var item in items)
            {
                var hash = item.GetHash();
                int newIndex = _getAddressIndex(hash, newBlockDepth); // tuto pozor

                if (newIndex == splittingBlockIndex)
                {
                    // item stays in the same block
                    continue;
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
                    splittingBlock.Address = _heapFile.GetEmptyBlock();
                }

                if (splittingBlockItems.Count != 0)
                {
                    // zapise sa zmena do suboru
                    splittingBlock.SetBlockItems(splittingBlockItems.ToArray());

                    
                    targetBlock.Address = _heapFile.GetEmptyBlock();
                    targetBlock.SetBlockItems(targetBlockItems.ToArray());
                }
            }

            // update siblings blocks
            var siblingsCount = (int)Math.Pow(2, depthsDifference - 1) - 1;
            for (int i = 0; i <= siblingsCount; i++)
            {
                _addresses[splittingBlockIndex + i].Address = splittingBlock.Address;
                _addresses[splittingBlockIndex + i].BlockDepth = newBlockDepth;
            
                _addresses[targetBlockIndex + i].Address = targetBlock.Address;
                _addresses[targetBlockIndex + i].BlockDepth = newBlockDepth;
            }
        }

        #endregion
    }

    public class ExtendableHashFileBlock<TData> where TData : class, IExtendableHashFileData, new()
    {
        public int Address { get; set; }
        public int BlockDepth { get; set; } = 1;
        public HeapFileBlock<TData> Block =>
            _heapFile.GetBlock(Address);
            // _heapFile.GetBlock(
            //         Address is null 
            //         ? throw new InvalidOperationException("Block address is not set")
            //         : Address.Value
            // );

        private HeapFile<TData> _heapFile;

        public ExtendableHashFileBlock(int address, HeapFile<TData> heapFile)
        {
            Address = address;
            _heapFile = heapFile;
        }

        public void InsertToBlock(TData data)
        {
            _heapFile.InsertToBlock(Address, data);
        }

        public void SetBlockItems(TData[] items)
        {
            _heapFile.SetBlockItems(Address, items);
        }
        public override string ToString()
        {
            return $"[{BlockDepth}] {Address}";
        }
    }
}