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

        /// <summary>
        /// Id (int) + Firstname (int length + _firstnameMax bytes) + Lastname (int length + _lastnameMax bytes)
        /// </summary>
        /// <returns></returns>
        public int Size => sizeof(int) + sizeof(int) + _firstnameMax + sizeof(int) + _lastnameMax;


        public byte[] ToBytes()
        {
            byte[] buffer = new byte[Size];
            int offset = 0;

            // Id (4 bytes)
            BitConverter.GetBytes(Id).CopyTo(buffer, offset);
            offset += sizeof(int);

            // Firstname (4 bytes (length) + 15 bytes)
            BitConverter.GetBytes(Firstname.Length).CopyTo(buffer, offset);
            offset += sizeof(int);
            Encoding.ASCII.GetBytes(Firstname.PadRight(_firstnameMax)).CopyTo(buffer, offset);
            offset += _firstnameMax;

            // Lastname (4 bytes (length) + 20 bytes)
            BitConverter.GetBytes(Lastname.Length).CopyTo(buffer, offset);
            offset += sizeof(int);
            Encoding.ASCII.GetBytes(Lastname.PadRight(_lastnameMax)).CopyTo(buffer, offset);

            return buffer;
        }

        public void FromBytes(byte[] bytes)
        {
            int offset = 0;

            // Id (4 bytes)
            Id = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);

            // Firstname (4 bytes actual length + 15 bytes)
            int firstnameLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            Firstname = Encoding.ASCII.GetString(bytes, offset, _firstnameMax)[..firstnameLength];
            offset += _firstnameMax;

            // Lastname (4 bytes actual length + 20 bytes)
            int lastnameLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            Lastname = Encoding.ASCII.GetString(bytes, offset, _lastnameMax)[..lastnameLength];
        }
    }
}
