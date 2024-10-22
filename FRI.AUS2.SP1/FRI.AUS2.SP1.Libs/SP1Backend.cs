using FRI.AUS2.Libs.Structures.Trees;
using FRI.AUS2.SP1.Libs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        public IList<GeoItem> Combined
        {
            // TODO - prerobit iterator...
            get 
            {
                var data = new List<GeoItem>();

                var it = _treeCombined.GetInOrderIterator();
                if (it is null)
                {
                    return data;
                }

                while (it.MoveNext())
                {
                    if (it.Current.Item is not null)
                    {
                        data.Add(it.Current.Item);
                    }
                }

                return data;
            }
        }

        private KdTree<GpsPointItem<Property>> _treeProperties { get; init; } = new KdTree<GpsPointItem<Property>>();
        private KdTree<GpsPointItem<Parcel>> _treeParcels { get; init; } = new KdTree<GpsPointItem<Parcel>>();
        private KdTree<GpsPointItem<GeoItem>> _treeCombined { get; init; } = new KdTree<GpsPointItem<GeoItem>>();

        public SP1Backend()
        {
            _initializeProperties();
            _initializeParcels();
        }

        private void _initializeProperties()
        {
            AddProperty(1, "Property 1", new GpsPoint(1.1, 1.1), new GpsPoint(1.1, 1.2));
            AddProperty(2, "Property 2", new GpsPoint(-1.1, 1.1), new GpsPoint(1.1, 1.3));
            AddProperty(3, "Property 3", new GpsPoint(1.1, -1.1), new GpsPoint(1.1, 1.4));
            AddProperty(4, "Property 4", new GpsPoint(1.1, 1.2), new GpsPoint(1.1, 1.5));
            AddProperty(5, "Property 5", new GpsPoint(-1.1, 1.1), new GpsPoint(1.1, 1.2));
        }


        private void _initializeParcels()
        {
            AddParcel(1, "Parcel 1", new GpsPoint(1.1, 1.1), new GpsPoint(1.1, 1.2));
            AddParcel(2, "Parcel 2", new GpsPoint(-1.1, 1.1), new GpsPoint(1.1, 1.3));
            AddParcel(3, "Parcel 3", new GpsPoint(1.1, -1.1), new GpsPoint(1.1, 1.4));
            AddParcel(4, "Parcel 4", new GpsPoint(1.1, 1.2), new GpsPoint(1.1, 1.5));
            AddParcel(5, "Parcel 5", new GpsPoint(-1.1, 1.1), new GpsPoint(1.1, 1.2));
        }

        #region Adding
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

            // add parcel to the structures
            _parcels.Add(parcel);
            _addToTree(parcel, _treeParcels);
            _addToTree(parcel, _treeCombined);
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

            // add property to the structures
            _properties.Add(property);
            _addToTree(property, _treeProperties);
            _addToTree(property, _treeCombined);
        }

        private void _addToTree<T>(T item, KdTree<GpsPointItem<T>> tree) where T : GeoItem
        {
            if (item.PositionA is not null)
            {
                tree.Insert(new GpsPointItem<T>(
                    item.PositionA, 
                    (T)item
                ));
            }

            if (item.PositionB is not null)
            {
                tree.Insert(new GpsPointItem<T>(
                    item.PositionB, 
                    (T)item
                ));
            }
        }

        #endregion

        #region Generating
        public void GenerateParcels(int count, int seed, string descriptionPrefix, (int min, int max) streetNumber, (int min, int max) posA_X, (int min, int max) posA_Y, (int min, int max) posB_X, (int min, int max) posB_Y)
        {
            _generateGeoItems(count, seed, descriptionPrefix, streetNumber, posA_X, posA_Y, posB_X, posB_Y, AddParcel);
        }

        public void GenerateProperties(int count, int seed, string descriptionPrefix, (int min, int max) streetNumber, (int min, int max) posA_X, (int min, int max) posA_Y, (int min, int max) posB_X, (int min, int max) posB_Y)
        {
            _generateGeoItems(count, seed, descriptionPrefix, streetNumber, posA_X, posA_Y, posB_X, posB_Y, AddProperty);
        }

        private void _generateGeoItems(int count, int seed, string descriptionPrefix, (int min, int max) streetNumber, (int min, int max) posA_X, (int min, int max) posA_Y, (int min, int max) posB_X, (int min, int max) posB_Y, Action<int, string, GpsPoint, GpsPoint> addAction)
        {
            Random random = new Random(seed);

            for (int i = 0; i < count; i++)
            {
                addAction(
                    random.Next(streetNumber.min, streetNumber.max),
                    $"{descriptionPrefix} {i + 1}",
                    new GpsPoint(
                        _getRandomDouble(random, posA_X),
                        _getRandomDouble(random, posA_Y)
                    ),
                    new GpsPoint(
                        _getRandomDouble(random, posB_X),
                        _getRandomDouble(random, posB_Y)
                    )
                );
            }
        }

        private double _getRandomDouble(Random random, (int min, int max) range, int precision = 6)
        {
            var precisionFactor = (int) Math.Pow(10, precision);

            return random.NextInt64(range.min * precisionFactor, range.max * precisionFactor) / (double)precisionFactor;
        }
        #endregion
        
        #region Finding
        public IList<Parcel> FindParcels(GpsPoint point)
        {
            return _findItems(point, _treeParcels);
        }

        public IList<Property> FindProperties(GpsPoint point)
        {
            return _findItems(point, _treeProperties);
        }

        public IList<GeoItem> FindCombined(GpsPoint point)
        {
            return _findItems(point, _treeCombined);
        }

        private IList<T> _findItems<T>(GpsPoint point, KdTree<GpsPointItem<T>> tree)
        {
            var data = tree.Find(new GpsPointItem<T>(point, default))
                            .Where(item => item.Item is not null)
                            .Select(item => item.Item);

            // can use ! because the data is not null
            return data!.ToList<T>();
        }
        #endregion
    }
}
