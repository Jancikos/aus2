using System.Collections;
using System.Security.Cryptography;
using System.Text;
using FRI.AUS2.Libs.Helpers;
using FRI.AUS2.Libs.Structures.Files;

namespace FRI.AUS2.SP2.Libs.Models
{
    public class CustomerAddressByEcv : IExtendableHashFileData
    {
        public string ECV = "";
        public int Addreess;


        public int Size => 1 + Customer.EcvMaxSize + sizeof(int);

        public bool Equals(IHeapFileData other)
        {
            if (other is CustomerAddressByEcv otherCustomerAddressById)
            {
                return ECV == otherCustomerAddressById.ECV;
            }

            return false;
        }

        public byte[] ToBytes()
        {
            var buffer = new byte[Size];
            var offset = 0;

            buffer[offset] = (byte)ECV.Length;
            offset += 1;
            Encoding.ASCII.GetBytes(ECV.PadRight(Customer.EcvMaxSize)).CopyTo(buffer, offset);
            offset += Customer.EcvMaxSize;

            BitConverter.GetBytes(Addreess).CopyTo(buffer, offset);

            return buffer;
        }

        public void FromBytes(byte[] bytes)
        {
            var offset = 0;

            var ecvLength = bytes[offset];
            offset += 1;
            ECV = Encoding.ASCII.GetString(bytes, offset, ecvLength);
            offset += Customer.EcvMaxSize;

            Addreess = BitConverter.ToInt32(bytes, offset);
        }

        public BitArray GetHash()
        {
            // 10 * 8 = 80 bits - I can adrress only 32 bits, so I will use only 32 bits from end
            var fullBytes =  Encoding.ASCII.GetBytes(ECV);

            // get only 32 bits from end of ECV
            var lastBytes = new byte[4];
            int bitesToTake = Math.Min(4, fullBytes.Length);
            for (int i = 0; i < bitesToTake; i++)
            {
                lastBytes[i] = fullBytes[fullBytes.Length - bitesToTake + i];
            }

            return new BitArray(lastBytes);
        }

        public override string ToString()
        {
            return $"{ECV}: {Addreess}";
        }
    }
}