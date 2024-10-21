namespace FRI.AUS2.SP1.Libs.Models
{
    public abstract class GeoItem
    {
        public GpsPoint? PositionA { get; set; }
        public GpsPoint? PositionB { get; set; }

        public override string ToString()
        {
            return $"[{PositionA}], [{PositionB}]";
        }
    }
}
