using System.Collections;
using System.Security.Cryptography;
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


        // todo - premysliet ci toto je spravne
        public BitArray GetHash()
        {
            // return new BitArray(Encoding.ASCII.GetBytes(ECV)); // 10 * 8 = 80 bits
            
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(ECV));

                // Extract the first 4 bytes (32 bits) of the hash
                byte[] firstFourBytes = new byte[4];
                Array.Copy(hashBytes, 0, firstFourBytes, 0, 4);

                // Create a BitArray from the 4 bytes
                return new BitArray(firstFourBytes);
            }
        }

        public override string ToString()
        {
            return $"{ECV}: {Addreess}";
        }
    }
}