using System;
using System.Collections.Generic;
using System.Linq;
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
        private int[] _addresses = new int[0];
        public int AddressesCount => _addresses.Length;
        public int[] Addresses => _addresses;

        private HeapFile<TData> _heapFile;
        public HeapFile<TData> HeapFile => _heapFile;

        public DynamicHashFile(FileInfo file)
        {
            _heapFile = new HeapFile<TData>(500, file);
            _heapFile.Clear();

            _increaseDepth();
            _addresses[0] = _heapFile.CreateNewBlock(); // pouztitie _heapFile.CreateNewBlock(); nahradit metodou GetEmptyBlock
            _addresses[1] = _heapFile.CreateNewBlock(); // pouztitie _heapFile.CreateNewBlock(); nahradit metodou GetEmptyBlock
        }

        #region Insert
        public void Insert(TData data)
        {
            int hash = data.GetHash();
            int addressIndex = _getAddressIndex(hash);
            int blockAddress = _addresses[addressIndex];

            try {
                _heapFile.InsertToBlock(blockAddress, data);
            } catch (InvalidOperationException e) {
                // block is full
                // split block
                throw new NotImplementedException("Block is full and split is not implemented");
            }
        }
        #endregion

        #region Find
        public TData Find(TData filter)
        {
            int hash = filter.GetHashCode();
            int addressIndex = _getAddressIndex(hash);
            int blockAddress = _addresses[addressIndex];

            var block = _heapFile.GetBlock(blockAddress);
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
            int[] newAddresses = new int[(int)Math.Pow(2, Depth)];
            
            for (int i = 0; i < _addresses.Length; i++)
            {
                int newIndexBase = i * 2;

                for (int j = 0; j < 2; j++)
                {
                    newAddresses[newIndexBase + j] = _addresses[i];
                }
            }

            _addresses = newAddresses;
        }

        #endregion
    }
}