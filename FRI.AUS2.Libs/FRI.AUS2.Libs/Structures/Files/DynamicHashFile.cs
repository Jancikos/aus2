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
        private int _depth = 3;
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
        
        public int _getAddressIndex(int hash)
        {
            int mask = 0;

            _depth.Repeat(() =>
            {
                mask = (mask << 1) | 1;
            });
            
            return hash & mask;
        }

        #endregion
    }
}