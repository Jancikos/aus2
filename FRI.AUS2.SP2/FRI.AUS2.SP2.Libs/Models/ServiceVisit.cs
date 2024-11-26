using System.Text;
using FRI.AUS2.Libs.Helpers;
using FRI.AUS2.Libs.Structures.Files;

namespace FRI.AUS2.SP2.Libs.Models
{
    internal class ServiceVisit : IBinaryData
    {
        public DateOnly Date { get; set; }
        public double Price { get; set; }


        private const byte _descriptionsMaxCount = 10;
        private const byte _descriptionMax = 20;
        private string[] _descriptions = new string[_descriptionsMaxCount];

        public string[] Descriptions
        {
            get => _descriptions;
            set
            {
                if (value.Length > _descriptionsMaxCount)
                {
                    throw new ArgumentException($"Descriptions count is max {_descriptionsMaxCount}");
                }

                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i].Length > _descriptionMax)
                    {
                        throw new ArgumentException($"One description is max {_descriptionMax} characters long");
                    }
                }

                _descriptions = value;
            }
        }

        /// <summary>
        /// date (4 bytes) + price (8 bytes) + descriptions (4 bytes (length) + 10 * (1 + 20 bytes))
        /// </summary>
        /// <returns></returns>
        public int Size => Date.Size() + sizeof(double) + sizeof(int) + _descriptionsMaxCount * (_descriptionMax + sizeof(byte));

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
            for (int i = 0; i < _descriptionsMaxCount; i++)
            {
                buffer[offset] = (byte)_descriptions[i].Length;
                offset += sizeof(byte);
                Encoding.ASCII.GetBytes(_descriptions[i].PadRight(_descriptionMax)).CopyTo(buffer, offset);
                offset += _descriptionMax;
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
            for (int i = 0; i < _descriptionsMaxCount; i++)
            {
                if (i < actualDescriptionsCount)
                {
                    _descriptions[i] = Encoding.ASCII.GetString(bytes, offset + sizeof(byte), _descriptionMax).TrimEnd();
                }

                offset += _descriptionMax + sizeof(byte);
            }
        }

        public override string ToString()
        {
            return $"{Date} - {Price:F2}€ - {string.Join(", ", _descriptions)}";
        }
    }
}
