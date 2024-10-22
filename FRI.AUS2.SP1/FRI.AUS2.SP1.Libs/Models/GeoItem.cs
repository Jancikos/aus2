namespace FRI.AUS2.SP1.Libs.Models
{
    public abstract class GeoItem
    {
        public GpsPoint? PositionA { get; set; }
        public GpsPoint? PositionB { get; set; }

        public abstract string Data { get; }

        public override string ToString()
        {
            return $"{Data} [{PositionA}], [{PositionB}]";
        }
    }
}
