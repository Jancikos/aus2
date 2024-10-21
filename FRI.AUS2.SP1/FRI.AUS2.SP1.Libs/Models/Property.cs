using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRI.AUS2.SP1.Libs.Models
{
    public class Property
    {
        public int StreetNumber { get; set; }
        public string Description { get; set; } = string.Empty;

        public double X {  get; set; }
        public double Y { get; set; }

        // TODO list of Parcels

        public override string ToString()
        {
            return $"Property {StreetNumber}. - {Description} [{X}, {Y}]";
        }
    }
}
