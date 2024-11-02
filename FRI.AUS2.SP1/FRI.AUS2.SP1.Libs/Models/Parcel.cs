namespace FRI.AUS2.SP1.Libs.Models
{
    public class Parcel : GeoItem
    {
        public static string[] ParcelTypes = new string[] { "Zastavané plochy a nádvoria", "Záhrady", "Orná pôda", "Trvalé trávne porasty", "Lesné pozemky", "Vodné plochy", "Ostatné plochy" };

        public int Number { get; set; }
        public string Description { get; set; } = string.Empty;

        public override string Data => $"P {Number}. - {Description}";

        private IList<Property> _properties = new List<Property>();
        public IList<Property> Properties => _properties;

        public int PropertiesCount => _properties.Count;

        public void AddProperty(Property property)
        {
            _properties.Add(property);
        }

        public void RemoveProperty(Property property)
        {
            _properties.Remove(property);
        }
        
        public override string ToString()
        {
            return $"{Data} [{base.ToString()}]";
        }

    }
}
