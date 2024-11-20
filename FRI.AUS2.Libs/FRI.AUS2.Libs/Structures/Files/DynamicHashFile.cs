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
            int addressIndex = _getAddressIndex(hash);
            var dhfBlock = _addresses[addressIndex];

            bool inserted = false;
            do {
                try {
                    _heapFile.InsertToBlock(dhfBlock.Address, data);
                    inserted = true;
                } catch (InvalidOperationException) {
                    // block is full
                    // split block
                    throw new NotImplementedException("Block is full and split is not implemented");
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

        public int _getAddressIndex(int hash)
        {
            int mask = 0;

            _depth.Repeat(() =>
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