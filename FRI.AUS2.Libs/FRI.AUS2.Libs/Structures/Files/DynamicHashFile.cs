using System;
using System.Collections.Generic;
using System.Linq;
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

        public DynamicHashFile(FileInfo file)
        {
            _heapFile = new HeapFile<TData>(500, file);

            _increaseDepth();
            _addresses[0] = _heapFile.CreateNewBlock();
            _addresses[1] = _heapFile.CreateNewBlock();
        }

        #region Insert
        public void Insert(TData data)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Find
        public TData Find(TData filter)
        {
            throw new NotImplementedException();
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