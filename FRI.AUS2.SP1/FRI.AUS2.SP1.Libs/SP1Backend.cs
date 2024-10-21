using FRI.AUS2.SP1.Libs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRI.AUS2.SP1.Libs
{
    public class SP1Backend
    {
        private List<Property> _properties { get; init; } = new List<Property>();
        private List<Parcel> _parcels { get; init; } = new List<Parcel>();

        public IList<Property> Properties => _properties;
        public IList<Parcel> Parcels => _parcels;

        public void AddParcel(int number, string description, GpsPoint posA, GpsPoint posB)
        {
            Parcel parcel = new Parcel
            {
                Number = number,
                Description = description,
                PosA = posA,
                PosB = posB
            };

            // TODO: Add properties to the parcel (by the PosA and PosB coordinates)

            _parcels.Add(parcel);
        }

        public void AddProperty(int streetNumber, string description, GpsPoint posA, GpsPoint posB)
        {
            Property property = new Property
            {
                StreetNumber = streetNumber,
                Description = description,
                PosA = posA,
                PosB = posB
            };

            // TODO: Add parcels to the property (by the PosA and PosB coordinates)

            _properties.Add(property);
        }

    }
}
