namespace FRI.AUS2.SP1.Libs.Models
{
    public class Property : GeoItem
    {
        public int StreetNumber { get; set; }
        public string Description { get; set; } = string.Empty;

        // TODO list of Parcels
        public override string ToString()
        {
            return $"Property {StreetNumber}. - {Description} [{base.ToString()}]";
        }
    }
}
