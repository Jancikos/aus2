namespace FRI.AUS2.SP1.Libs.Models
{
    public class Property : GeoItem
    {
        public static string[] Cities = { "Ladzany", "Košice", "Bratislava", "Prešov", "Banská Bystrica", "Žilina", "Trnava", "Nitra", "Trenčín" };

        public int StreetNumber { get; set; }
        public string Description { get; set; } = string.Empty;

        public override string Data => $"N {Description} - {StreetNumber}.";

        private IList<Parcel> _parcels = new List<Parcel>();
        public IList<Parcel> Parcels => _parcels;

        public int ParcelsCount => _parcels.Count;

        public void AddParcel(Parcel parcel)
        {
            _parcels.Add(parcel);
        }

        public void RemoveParcel(Parcel parcel)
        {
            _parcels.Remove(parcel);
        }

        public override string ToString()
        {
            return $"{Data} [{base.ToString()}]";
        }
    }
}
