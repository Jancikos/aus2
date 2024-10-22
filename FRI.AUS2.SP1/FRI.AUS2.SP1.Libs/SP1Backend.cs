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

        public SP1Backend()
        {
            _initializeProperties();
            _initializeParcels();
        }

        private void _initializeProperties()
        {
            AddProperty(1, "Property 1", new GpsPoint(48.1486, 17.1077), new GpsPoint(48.1486, 17.1077));
            AddProperty(2, "Property 2", new GpsPoint(48.1486, 17.1077), new GpsPoint(48.1486, 17.1077));
            AddProperty(3, "Property 3", new GpsPoint(48.1486, 17.1077), new GpsPoint(48.1486, 17.1077));
            AddProperty(4, "Property 4", new GpsPoint(48.1486, 17.1077), new GpsPoint(48.1486, 17.1077));
            AddProperty(5, "Property 5", new GpsPoint(48.1486, 17.1077), new GpsPoint(48.1486, 17.1077));
        }


        private void _initializeParcels()
        {
            AddParcel(1, "Parcel 1", new GpsPoint(48.1486, 17.1077), new GpsPoint(48.1486, 17.1077));
            AddParcel(2, "Parcel 2", new GpsPoint(48.1486, 17.1077), new GpsPoint(48.1486, 17.1077));
            AddParcel(3, "Parcel 3", new GpsPoint(48.1486, 17.1077), new GpsPoint(48.1486, 17.1077));
            AddParcel(4, "Parcel 4", new GpsPoint(48.1486, 17.1077), new GpsPoint(48.1486, 17.1077));
            AddParcel(5, "Parcel 5", new GpsPoint(48.1486, 17.1077), new GpsPoint(48.1486, 17.1077));
        }

        public void AddParcel(int number, string description, GpsPoint posA, GpsPoint posB)
        {
            Parcel parcel = new Parcel
            {
                Number = number,
                Description = description,
                PositionA = posA,
                PositionB = posB
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
                PositionA = posA,
                PositionB = posB
            };

            // TODO: Add parcels to the property (by the PosA and PosB coordinates)

            _properties.Add(property);
        }

    }
}
