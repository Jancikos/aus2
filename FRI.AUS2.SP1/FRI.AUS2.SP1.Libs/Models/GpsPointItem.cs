using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRI.AUS2.Libs.Structures.Trees.Interfaces;

namespace FRI.AUS2.SP1.Libs.Models
{
    public class GpsPointItem<T> : IKdTreeData
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

        public int Compare(int level, IKdTreeData other)
        {
            double val1 = GetDiminesionValue(level % GetDiminesionsCount());
            double val2 = other.GetDiminesionValue(level % GetDiminesionsCount());

            double diff = val1 - val2;
            if (diff < E)
            {
                return 0;
            }

            return diff > 0 ? 1 : -1;
        }

        public int GetDiminesionsCount() => 2;

        public double GetDiminesionValue(int dim)
        {
            switch (dim)
            {
                case 0:
                    return Position.X;
                case 1:
                    return Position.Y;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
