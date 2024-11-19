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
        private int _depth;
        public int Depth => _depth;

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
        
        private int _getAddress(int hash)
        {
            int address = 0;

            for (int i = 0; i < Depth; i++)
            {
                int mask = 1 << i;
                int bit = (hash & mask) >> i;

                address = address << 1;
                address = address | bit;
            }

            return address;
        }

        #endregion
    }
}