using FRI.AUS2.Libs.Helpers;

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

        public bool Equals(GpsPoint other)
        {
            return X.CompareE(other.X) == 0 && Y.CompareE(other.Y) == 0;
        }

        public override string ToString()
        {
            return $"{X}, {Y}";
        }
    }
}
