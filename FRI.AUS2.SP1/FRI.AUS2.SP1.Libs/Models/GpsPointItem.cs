using FRI.AUS2.Libs.Structures.Trees.Interfaces;
using static FRI.AUS2.Libs.Helpers.DoubleExtension;

namespace FRI.AUS2.SP1.Libs.Models
{
    public class GpsPointItem<T> : IKdTreeData where T : GeoItem
    {
        /// <summary>
        /// Epsilon for double comparison 
        /// </summary>
        public const double E = 0.0000001;

        public GpsPoint Position { get; init; }
        public T? Item { get; set; }

        public GpsPointItem(GpsPoint position, T? item)
        {
            Position = position;
            Item = item;
        }

        public bool EqualsPosition(GpsPointItem<T> other) 
        {
            return Position.Equals(other.Position);
        }

        public int Compare(int level, IKdTreeData other)
        {
            var otherModel = (GpsPointItem<T>) other;

            switch (level % GetDiminesionsCount())
            {
                case 0:
                    return Position.X.CompareToWithE(otherModel.Position.X);
                case 1:
                    return Position.Y.CompareToWithE(otherModel.Position.Y);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public int GetDiminesionsCount() => 2;

        public bool Equals(IKdTreeData other)
        {
            var otherModel = (GpsPointItem<T>) other;
            if (otherModel is null) {
                return false;
            }

            if (otherModel.Item is null) {
                return Item is null;
            }

            if(Item is null) {
                return false;
            }

            return Item.Id == otherModel.Item.Id;
        }
    }
}
