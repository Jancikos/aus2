using FRI.AUS2.Libs.Helpers;

namespace FRI.AUS2.SP1.Libs.Models
{
    public class GpsPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public string Latitude => $"{X}° {(X >= 0 ? "N" : "S")}";
        public string Longitude => $"{Y}° {(Y >= 0 ? "E" : "W")}";

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

        public bool Equals(GpsPoint? other)
        {
            if (other is null)
            {
                return false;
            }

            return X.CompareToWithE(other.X) == 0 && Y.CompareToWithE(other.Y) == 0;
        }

        public override string ToString()
        {
            // return $"{Latitude}, {Longitude}";
            return $"{X}, {Y}";
        }

        public string ToCsv()
        {
            return $"{X};{Y}";
        }
    }
}
