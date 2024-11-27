using System.Collections;
using System.Text;
 using FRI.AUS2.Libs.Structures.Files;

namespace FRI.AUS2.SP2.Libs.Models
{
    public class CustomerAddressById : IExtendableHashFileData
    {
        public int Id;
        public int Addreess;

        public int Size => sizeof(int) * 2;

        public bool Equals(IHeapFileData other)
        {
            if (other is CustomerAddressById otherCustomerAddressById)
            {
                return Id == otherCustomerAddressById.Id;
            }

            return false;
        }

        public byte[] ToBytes()
        {
            var buffer = new byte[Size];

            BitConverter.GetBytes(Id).CopyTo(buffer, 0);
            BitConverter.GetBytes(Addreess).CopyTo(buffer, sizeof(int));

            return buffer;
        }

        public void FromBytes(byte[] bytes)
        {
            Id = BitConverter.ToInt32(bytes, 0);
            Addreess = BitConverter.ToInt32(bytes, sizeof(int));
        }


        public BitArray GetHash()
        {
            return new BitArray([Id]);
        }

        public override string ToString()
        {
            return $"{Id}: {Addreess}";
        }
    }
}