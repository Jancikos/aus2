using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRI.AUS2.Libs.Structures.Files;

namespace FRI.AUS2.StructureTester.HeapFileTester.Models
{
    public class HeapData : IBinaryData
    {
        private const int _firstnameMax = 15;
        private string _firstname = "";
        public string Firstname
        {
            get => _firstname;
            set
            {
                if (value.Length > _firstnameMax)
                {
                    throw new ArgumentException($"Firstname is max {_firstnameMax} characters long");
                }
                _firstname = value;
            }
        }

        private const int _lastnameMax = 20;
        private string _lastname = "";
        public string Lastname
        {
            get => _lastname;
            set
            {
                if (value.Length > _lastnameMax)
                {
                    throw new ArgumentException($"Lastname is max {_lastnameMax} characters long");
                }
                _lastname = value;
            }
        }

        public int Id;
        public int Size => sizeof(int) + _firstnameMax + _lastnameMax;


        public byte[] ToBytes()
        {
            byte[] buffer = new byte[Size];
            int offset = 0;

            // Convert Id to bytes (4 bytes)
            BitConverter.GetBytes(Id).CopyTo(buffer, offset);
            offset += sizeof(int);

            // Convert Firstname to bytes (15 bytes, padded if necessary)
            Encoding.ASCII.GetBytes(Firstname.PadRight(_firstnameMax)).CopyTo(buffer, offset);
            offset += _firstnameMax;

            // Convert Lastname to bytes (20 bytes, padded if necessary)
            Encoding.ASCII.GetBytes(Lastname.PadRight(_lastnameMax)).CopyTo(buffer, offset);

            return buffer;
        }

        public void FromBytes(byte[] bytes)
        {
            int offset = 0;

            // Convert Id from bytes (4 bytes)
            Id = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);

            // Convert Firstname from bytes (15 bytes)
            Firstname = Encoding.ASCII.GetString(bytes, offset, _firstnameMax).Trim();
            offset += _firstnameMax;

            // Convert Lastname from bytes (20 bytes)
            Lastname = Encoding.ASCII.GetString(bytes, offset, _lastnameMax).Trim();
        }
    }
}
