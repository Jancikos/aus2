namespace FRI.AUS2.SP1.Libs.Models
{
    public abstract class GeoItem
    {
        public static long IdCounter = 0;

        private static long GetNextId()
        {
            return IdCounter++;
        }
        public static void SetIdCounter(long id)
        {
            IdCounter = id;
        }

        private long _id;
        public long Id
        {
            get => _id;
        }
        
        public GpsPoint? PositionA { get; set; }
        public GpsPoint? PositionB { get; set; }

        public abstract string Data { get; }


        protected GeoItem(long id = 0)
        {
            _id = id == 0 ? GetNextId() : id;
        }

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
