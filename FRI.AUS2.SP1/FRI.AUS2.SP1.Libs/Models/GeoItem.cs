namespace FRI.AUS2.SP1.Libs.Models
{
    public abstract class GeoItem
    {
        public GpsPoint? PositionA { get; set; }
        public GpsPoint? PositionB { get; set; }

        public abstract string Data { get; }

        public bool EqualsPosition(GeoItem other)
        {
            if (PositionA is not null) 
            {
                if (other.PositionA is null) 
                {
                    return false;
                }

                if (!PositionA.Equals(other.PositionA)) 
                {
                    return false;
                }
            }

            if (PositionB is not null) 
            {
                if (other.PositionB is null) 
                {
                    return false;
                }

                if (!PositionB.Equals(other.PositionB)) 
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return $"{Data} [{PositionA}], [{PositionB}]";
        }
    }
}
