﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRI.AUS2.SP1.Libs.Models
{
    public class Parcel
    {
        public int Number { get; set; }
        public string Description { get; set; } = string.Empty;

        public double X {  get; set; }
        public double Y { get; set; }

        // TODO list of Properties
        
        public override string ToString()
        {
            return $"Parcel {Number}. - {Description} [{X}, {Y}]";
        }
    }
}
