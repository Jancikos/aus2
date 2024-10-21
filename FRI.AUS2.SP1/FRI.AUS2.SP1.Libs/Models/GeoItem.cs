namespace FRI.AUS2.SP1.Libs.Models
{
    public abstract class GeoItem
    {
        public double X {  get; set; }
        public double Y { get; set; }

        public override string ToString()
        {
            return $"{X}, {Y}";
        }
    }
}
