using System.Text;
using FRI.AUS2.Libs.Helpers;
using FRI.AUS2.Libs.Structures.Files;

namespace FRI.AUS2.SP2.Libs.Models
{
    public class ServiceVisit : IBinaryData
    {
        public DateOnly Date { get; set; }
        public double Price { get; set; }


        public const byte DescriptionsMaxCount = 10;
        private const byte _descriptionLengthMax = 20;
        private string[] _descriptions = new string[DescriptionsMaxCount];

        public string[] Descriptions
        {
            get => _descriptions;
            set
            {
                if (value.Length > DescriptionsMaxCount)
                {
                    throw new ArgumentException($"Descriptions count is max {DescriptionsMaxCount}");
                }

                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i].Length > _descriptionLengthMax)
                    {
                        throw new ArgumentException($"One description is max {_descriptionLengthMax} characters long");
                    }
                }

                _descriptions = value;
            }
        }

        /// <summary>
        /// date (4 bytes) + price (8 bytes) + descriptions (4 bytes (length) + 10 * (1 + 20 bytes))
        /// </summary>
        /// <returns></returns>
        public int Size => Date.Size() + sizeof(double) + sizeof(int) + DescriptionsMaxCount * (_descriptionLengthMax + sizeof(byte));

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[Size];
            int offset = 0;

            // Date (4 bytes)
            Date.ToBytes().CopyTo(buffer, offset);
            offset += Date.Size();

            // Price (8 bytes)
            BitConverter.GetBytes(Price).CopyTo(buffer, offset);
            offset += sizeof(double);

            // DescriptionS (4 bytes (length) + 10 * (1 + 20 bytes))
            BitConverter.GetBytes(Descriptions.Length).CopyTo(buffer, offset);
            offset += sizeof(int);
            for (int i = 0; i < DescriptionsMaxCount; i++)
            {
                buffer[offset] = (byte)_descriptions[i].Length;
                offset += sizeof(byte);
                Encoding.ASCII.GetBytes(_descriptions[i].PadRight(_descriptionLengthMax)).CopyTo(buffer, offset);
                offset += _descriptionLengthMax;
            }

            return buffer;
        }

        public void FromBytes(byte[] bytes)
        {
            int offset = 0;

            // Date (4 bytes)
            Date = DateOnlyExtension.FromBytes(bytes);
            offset += Date.Size();

            // Price (8 bytes)
            Price = BitConverter.ToDouble(bytes, offset);
            offset += sizeof(double);

            // Description (4 bytes actual Count + 10 * (1 + 20 bytes))
            int actualDescriptionsCount = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            for (int i = 0; i < DescriptionsMaxCount; i++)
            {
                if (i < actualDescriptionsCount)
                {
                    _descriptions[i] = Encoding.ASCII.GetString(bytes, offset + sizeof(byte), _descriptionLengthMax).TrimEnd();
                }

                offset += _descriptionLengthMax + sizeof(byte);
            }
        }

        public override string ToString()
        {
            return $"{Date} - {Price:F2}€ - {string.Join(", ", _descriptions)}";
        }
    }
}
