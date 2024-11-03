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

        public static string GetCsvHeader()
        {
            return "Id;PositionA_X;PositionA_Y;PositionB_X;PositionB_Y;Number;Description;Properties";
        }
        public string ToCsv()
        {
            return $"{Id};{PositionA?.ToCsv() ?? ";"};{PositionB?.ToCsv() ?? ";"};{Number};{Description};{string.Join(",", _properties.Select(p => p.Id))}";
        }
    }
}
