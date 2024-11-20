using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FRI.AUS2.Libs.Helpers;

namespace FRI.AUS2.Libs.Structures.Files
{
    public class DynamicHashFile<TData> where TData : class, IDynamicHashFileData, new()
    {
        private int _depth = 0;
        public int Depth => _depth;

        /// <summary>
        /// array of addresses to the blocks
        /// </summary>
        /// <returns></returns>
        private DynamicHashFileBlock<TData>[] _addresses = [];
        public int AddressesCount => _addresses.Length;
        public DynamicHashFileBlock<TData>[] Addresses => _addresses;

        private HeapFile<TData> _heapFile;
        public HeapFile<TData> HeapFile => _heapFile;

        public DynamicHashFile(FileInfo file)
        {
            _heapFile = new HeapFile<TData>(500, file);
            _heapFile.Clear();
            
            _initializeAddresses();
        }

        #region Insert
        public void Insert(TData data)
        {
            int hash = data.GetHash();

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
                    // throw new NotImplementedException("Block is full and split is not implemented");
                    
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
            int hash = filter.GetHashCode();
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
            throw new NotImplementedException();
        }
        #endregion

        #region Management


        private void _initializeAddresses()
        {
            _increaseDepth();
            
            _addresses[0] = new DynamicHashFileBlock<TData>(_heapFile.CreateNewBlock(), _heapFile); // pouztitie _heapFile.CreateNewBlock(); nahradit metodou GetEmptyBlock
            _addresses[1] = new DynamicHashFileBlock<TData>(_heapFile.CreateNewBlock(), _heapFile); // pouztitie _heapFile.CreateNewBlock(); nahradit metodou GetEmptyBlock
        }

        public int _getAddressIndex(int hash) => _getAddressIndex(hash, Depth);
        public int _getAddressIndex(int hash, int depth)
        {
            int mask = 0;

            depth.Repeat(() =>
            {
                mask = (mask << 1) | 1;
            });
            
            return hash & mask;
        }

        public void _increaseDepth()
        {
            _depth++;
            DynamicHashFileBlock<TData>[] newAddresses = new DynamicHashFileBlock<TData>[(int)Math.Pow(2, Depth)];
            
            for (int i = 0; i < _addresses.Length; i++)
            {
                int newIndexBase = i * 2;
                var adressBlock = _addresses[i];

                for (int j = 0; j < 2; j++)
                {
                    newAddresses[newIndexBase + j] = new DynamicHashFileBlock<TData>(adressBlock.Address, _heapFile) 
                    {
                         BlockDepth = adressBlock.BlockDepth
                    };
                }
            }

            _addresses = newAddresses;
        }

        private void _splitBlock(int splittingIndex)
        {
            var splittingBlock = _addresses[splittingIndex];
            var newBlockDepth = splittingBlock.BlockDepth + 1;
            var targetBlock = _addresses[splittingIndex + 1];

            // create new block
            targetBlock.Address = _heapFile.CreateNewBlock(); // pouztitie _heapFile.CreateNewBlock(); nahradit metodou GetEmptyBlock

            // try reinsert items
            var items = splittingBlock.Block.ValidItems;
            var splittingBlockItems = items.ToList();
            var targetBlockItems = new List<TData>();
            foreach (var item in items)
            {
                int hash = item.GetHash();
                int newIndex = _getAddressIndex(hash, newBlockDepth);

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

    public class DynamicHashFileBlock<TData> where TData : class, IDynamicHashFileData, new()
    {
        public int Address { get; set; }
        public int BlockDepth { get; set; } = 1;
        public HeapFileBlock<TData> Block => _heapFile.GetBlock(Address);

        private HeapFile<TData> _heapFile;

        public DynamicHashFileBlock(int address, HeapFile<TData> heapFile)
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