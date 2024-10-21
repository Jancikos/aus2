﻿namespace FRI.AUS2.SP1.Libs.Models
{
    public class Parcel : GeoItem
    {
        public int Number { get; set; }
        public string Description { get; set; } = string.Empty;

        // TODO list of Properties
        
        public override string ToString()
        {
            return $"Parcel {Number}. - {Description} [{base.ToString()}]";
        }
    }
}