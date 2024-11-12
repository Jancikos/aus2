using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRI.AUS2.Libs.Structures.Files
{
    public interface IHeapFileData : IBinaryData
    {
        public bool Equals(IHeapFileData other);
    }

}
