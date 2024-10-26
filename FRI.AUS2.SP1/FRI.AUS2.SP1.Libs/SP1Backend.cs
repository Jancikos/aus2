using FRI.AUS2.Libs.Structures.Trees;
using FRI.AUS2.SP1.Libs.Models;

namespace FRI.AUS2.SP1.Libs
{
    public class SP1Backend
    {
        private List<Property> _properties { get; init; } = new List<Property>();
        private List<Parcel> _parcels { get; init; } = new List<Parcel>();

        public IList<Property> Properties => _properties;
        public IList<Parcel> Parcels => _parcels;
        public IEnumerable<GpsPointItem<GeoItem>> Combined
        {
            get => _treeCombined;
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
            AddProperty(1, "H1", new GpsPoint(1, 2), new GpsPoint(2, 2));
            AddProperty(2, "H2", new GpsPoint(1, 1), new GpsPoint(-1, -1));
            AddProperty(3, "H3", new GpsPoint(1, -2), new GpsPoint(0, 0));
            AddProperty(4, "H4", new GpsPoint(3, 3), new GpsPoint(1, 5));
            AddProperty(5, "H5", new GpsPoint(-1, 1), new GpsPoint(1, 1));
        }


        private void _initializeParcels()
        {
            AddParcel(1, "P1", new GpsPoint(1, 1), new GpsPoint(2, 2));
            AddParcel(2, "P2", new GpsPoint(1, -1), new GpsPoint(4, 4));
            AddParcel(3, "P3", new GpsPoint(1, 2), new GpsPoint(2, 2));
            AddParcel(4, "P4", new GpsPoint(1, 5), new GpsPoint(1, 1));
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

            //  Add properties to the parcel (by the PosA and PosB coordinates)
            // find properties by the PosA
            var properties = _findItems(posA, _treeProperties);

            // find properties by the PosB
            if (!posB.Equals(posA))
            {
                var propertiesByPosB = _findItems(posB, _treeProperties);

                // merge properties
                properties = properties.Union(propertiesByPosB).ToList();
            }
            foreach (var property in properties)
            {
                parcel.AddProperty(property);
                property.AddParcel(parcel);
            }

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

            // Add parcels to the property (by the PosA and PosB coordinates)
            // find parcels by the PosA
            var parcels = _findItems(posA, _treeParcels);

            // find parcels by the PosB
            if (!posB.Equals(posA))
            {
                var parcelsByPosB = _findItems(posB, _treeParcels);

                // merge parcels
                parcels = parcels.Union(parcelsByPosB).ToList();
            }

            foreach (var parcel in parcels)
            {
                property.AddParcel(parcel);
                parcel.AddProperty(property);
            }


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
            var precisionFactor = (int)Math.Pow(10, precision);

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

        public IList<GpsPointItem<GeoItem>> FindCombined(GpsPoint pointA, GpsPoint pointB)
        {
            var itemsByA = _treeCombined
                            .Find(new GpsPointItem<GeoItem>(pointA, default));
            var itemsByB = _treeCombined
                            .Find(new GpsPointItem<GeoItem>(pointB, default));

            return itemsByA
                    .Concat(itemsByB)
                    .ToList();
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

        #region Deleting

        public void DeleteParcel(Parcel parcel)
        {
            // remove parcel from the list
            var result = _parcels.Remove(parcel);
            if (!result)
            {
                // not found
                return;
            }

            // remove references from the properties
            foreach (var property in parcel.Properties)
            {
                property.RemoveParcel(parcel);
            }

            // remove from trees
            _removeFromTree(parcel, _treeParcels);
            _removeFromTree(parcel, _treeCombined); // TODO - chyba pri odstranovani prvkov, ktore maju zhodne suradnice ako iny prvok 

            /// NOTES
            // remove parcel from the tree
            // // find and DELETE ALL parcels by the PosA/B
            // // // from that parcels remove the right parcel (by Parcel reference)
            // // // // insert the rest of the parcels back to the tree
        }

        public void DeleteProperty(Property property)
        {
            // remove property from the list
            var result = _properties.Remove(property);
            if (!result)
            {
                // not found
                return;
            }

            // remove references from the parcels
            foreach (var parcel in property.Parcels)
            {
                parcel.RemoveProperty(property);
            }

            // remove from trees
            _removeFromTree(property, _treeProperties);
            _removeFromTree(property, _treeCombined); // TODO - chyba pri odstranovani prvkov, ktore maju zhodne suradnice ako iny prvok 
        }


        private void _removeFromTree<T>(T itemToBeDeleted, KdTree<GpsPointItem<T>> tree) where T : GeoItem
        {
            removeItemByPosition(itemToBeDeleted.PositionA);
            removeItemByPosition(itemToBeDeleted.PositionB);

            void removeItemByPosition(GpsPoint? gpsPoint)
            {
                if (gpsPoint is not null)
                {
                    IList<T> deletedItems = new List<T>();
                    var itemsByGpsPoint = _findItems(gpsPoint, tree);

                    // vsetky zo stromu odstran
                    foreach (var itemByGpsPos in itemsByGpsPoint)
                    {
                        tree.Remove(new GpsPointItem<T>(gpsPoint, itemByGpsPos));
                        deletedItems.Add(itemByGpsPos);
                    }

                    // before insert remove the item from the deleted list
                    deletedItems.Remove(itemToBeDeleted);

                    // insert the rest of the items back to the tree
                    foreach (var deletedItem in deletedItems)
                    {
                        tree.Insert(new GpsPointItem<T>(gpsPoint, deletedItem));
                    }
                }
            }
        }
        #endregion

        #region Editing

        public void EditParcel(Parcel actual, Parcel updated)
        {
            if (updated.PositionA is null || updated.PositionB is null)
            {
                throw new ArgumentException("The updated parcel must have both positions.");
            }


            if ((actual.PositionA?.Equals(updated.PositionA) ?? false) && (actual.PositionB?.Equals(updated.PositionB) ?? false))
            {
                // update only the parcel details
                actual.Number = updated.Number;
                actual.Description = updated.Description;

                return;
            }

            // remove the actual parcel
            DeleteParcel(actual);

            // add the updated parcel
            AddParcel(updated.Number, updated.Description, updated.PositionA, updated.PositionB);
        }

        public void EditProperty(Property actual, Property updated)
        {
            if (updated.PositionA is null || updated.PositionB is null)
            {
                throw new ArgumentException("The updated property must have both positions.");
            }

            if ((actual.PositionA?.Equals(updated.PositionA) ?? false) && (actual.PositionB?.Equals(updated.PositionB) ?? false))
            {
                // update only the property details
                actual.StreetNumber = updated.StreetNumber;
                actual.Description = updated.Description;

                return;
            }

            // remove the actual property
            DeleteProperty(actual);

            // add the updated property
            AddProperty(updated.StreetNumber, updated.Description, updated.PositionA, updated.PositionB);
        }

        #endregion
    }
}
