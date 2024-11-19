using FRI.AUS2.Libs.Structures.Files;
using FRI.AUS2.Libs.Structures.Trees.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRI.AUS2.StructureTester.DynamicHashFileTester.Models
{
    public class DynamicHashFileData : IDynamicHashFileData
    {
        public int Size => 50;

        public bool Equals(IHeapFileData other)
        {
            throw new NotImplementedException();
        }

        public void FromBytes(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public int GetHash()
        {
            throw new NotImplementedException();
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }
}
