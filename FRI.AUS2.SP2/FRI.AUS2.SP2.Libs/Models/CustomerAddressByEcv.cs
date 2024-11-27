using System.Collections;
using System.Text;
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
            return new BitArray([ECV.GetHashCode()]);
        }

        public override string ToString()
        {
            return $"{ECV}: {Addreess}";
        }
    }
}