namespace FRI.AUS2.SP1.Libs.Models
{
    public abstract class GeoItem
    {
        public GpsPoint? PosA { get; set; }
        public GpsPoint? PosB { get; set; }

        public override string ToString()
        {
            return $"[{PosA}], [{PosB}]";
        }
    }
}
