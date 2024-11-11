using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRI.AUS2.Libs.Structures.Files
{
    public interface IBinaryData
    {
        public static abstract int Size { get; }
        byte[] ToBytes();
        void FromBytes(byte[] bytes);
    }

}
