namespace FRI.AUS2.SP1.Libs.Models
{
    public class GpsPoint
    {
        public double X {  get; set; }
        public double Y { get; set; }

        public GpsPoint()
        {
            X = 0;
            Y = 0;
        }
        public GpsPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"{X}, {Y}";
        }
    }
}
