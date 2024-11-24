using FRI.AUS2.Libs.Structures.Files;
using FRI.AUS2.Libs.Structures.Trees.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRI.AUS2.StructureTester.ExtendableHashFileTester.Models
{
    public class ExtendableHashFileData : IExtendableHashFileData
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

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }

        BitArray IExtendableHashFileData.GetHash()
        {
            throw new NotImplementedException();
        }
    }
}
