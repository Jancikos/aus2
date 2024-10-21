namespace FRI.AUS2.SP1.Libs.Models
{
    public class GpsPoint
    {
        public double X {  get; set; }
        public double Y { get; set; }

        public override string ToString()
        {
            return $"{X}, {Y}";
        }
    }
}
