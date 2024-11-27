using System.Collections;
using System.Text;
 using FRI.AUS2.Libs.Structures.Files;

namespace FRI.AUS2.SP2.Libs.Models
{
    public class Customer :  IHeapFileData
    {private const int _firstnameMax = 15;
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

        /// <summary>
        /// pozor na unikatnost
        /// </summary>
        public int Id;

        public const int VisitsMaxCount = 5;
        private List<ServiceVisit> _visits = new List<ServiceVisit>(VisitsMaxCount);
        public List<ServiceVisit> Visits
        {
            get => _visits;
            set
            {
                if (value.Count > VisitsMaxCount)
                {
                    throw new ArgumentException($"Items count is max {VisitsMaxCount}");
                }
                _visits = value;
            }
        }

        
        public const int EcvMaxSize = 10;
        private string _ecv = "";
        public string ECV
        {
            get => _ecv;
            set
            {
                if (value.Length > EcvMaxSize)
                {
                    throw new ArgumentException($"ecv is max {EcvMaxSize} characters long");
                }
                _ecv = value;
            }
        }

        /// <summary>
        /// Id (int) + Firstname (int length + _firstnameMax bytes) + Lastname (int length + _lastnameMax bytes) + ECV (int length + _ecvMax bytes) + Items (int actual length + VisitsMaxCount * (new ServiceVisit()).Size)
        /// </summary>
        /// <returns></returns>
        public int Size => sizeof(int) + sizeof(int) + _firstnameMax + sizeof(int) + _lastnameMax + sizeof(int) + EcvMaxSize + sizeof(int) + VisitsMaxCount * (new ServiceVisit()).Size;

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
            offset += _lastnameMax;

            // ECV (4 bytes (length) + 10 bytes)
            BitConverter.GetBytes(ECV.Length).CopyTo(buffer, offset);
            offset += sizeof(int);
            Encoding.ASCII.GetBytes(ECV.PadRight(EcvMaxSize)).CopyTo(buffer, offset);
            offset += EcvMaxSize;

            // Items (4 bytes actual length + 5 * NesteHeapDataItemSize)
            int actualVisitsLength = Visits.Count;
            BitConverter.GetBytes(actualVisitsLength).CopyTo(buffer, offset);
            offset += sizeof(int);

            for (int i = 0; i < VisitsMaxCount; i++)
            {
                if (i < actualVisitsLength)
                {
                    var visit = Visits[i];
                    visit.ToBytes().CopyTo(buffer, offset);
                }

                offset += (new ServiceVisit()).Size;
            }

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
            offset += _lastnameMax;

            // ECV (4 bytes actual length + 10 bytes)
            int ecvLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            ECV = Encoding.ASCII.GetString(bytes, offset, EcvMaxSize)[..ecvLength];
            offset += EcvMaxSize;

            // Items (4 bytes actual length + 5 * NesteHeapDataItemSize)
            int actualItemsLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);

            Visits.Clear();
            var itemSize = (new ServiceVisit()).Size;
            for (int i = 0; i < VisitsMaxCount; i++)
            {
                if (i < actualItemsLength)
                {
                    var item = new ServiceVisit();
                    item.FromBytes(bytes[offset..(offset + itemSize)]);
                    Visits.Add(item);
                }

                offset += itemSize;
            }
        }

        public bool Equals(IHeapFileData other)
        {
            var otherData = other as Customer;
            if (otherData is null)
            {
                return false;
            }

            return Id == otherData.Id;
        }

        public override string ToString()
        {
            return $"{Id}/{ECV} - {Firstname} {Lastname} [{Visits.Count}]";
        }
    }
}
