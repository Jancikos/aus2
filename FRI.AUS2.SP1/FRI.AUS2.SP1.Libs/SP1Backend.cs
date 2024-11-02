using FRI.AUS2.Libs.Structures.Trees;
using FRI.AUS2.SP1.Libs.Models;

namespace FRI.AUS2.SP1.Libs
{
    public class SP1Backend
    {
        public IEnumerable<Property> Properties => _treeProperties
                                                .Select(item => item.Item ?? throw new Exception("Property cant be null"));
        public IEnumerable<Parcel> Parcels => _treeParcels
                                                .Select(item => item.Item ?? throw new Exception("Parcel cant be null"));
        public IEnumerable<GpsPointItem<GeoItem>> Combined => _treeCombined
                                                .Select(item => item);

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

        public void GenerateData(int parcelsCount, int propertiesCount, double propertiesOverlap, int seed, int doublePrecision = 2)
        {
            var random = new Random(seed);
            (int min, int max) posX = (-10, 10);
            (int min, int max) posY = (-10, 10);

            // generate parcels
            var actualParcelsCount = _treeParcels.NodesCount; // sice to je 2x pocet, ale aspon bude cislo jedinecne
            for (int i = 0; i < parcelsCount; i++)
            {
                AddParcel(
                    actualParcelsCount + i + 1,
                    Parcel.ParcelTypes[random.Next(0, Parcel.ParcelTypes.Length)],
                    _getRandomGpsPoint(random, posX, posY, doublePrecision),
                    _getRandomGpsPoint(random, posX, posY, doublePrecision)
                );
            }

            // generate properties
            var actualPropertiesCount = _treeProperties.NodesCount; // sice to je 2x pocet, ale aspon bude cislo jedinecne
            for (int i = 0; i < propertiesCount; i++)
            {
                AddProperty(
                    actualPropertiesCount + i + 1,
                    Property.Cities[random.Next(0, Property.Cities.Length)],
                    random.NextDouble() < propertiesOverlap
                        ? _getGpsPointFromExistingParcel(random, parcelsCount)
                        : _getRandomGpsPoint(random, posX, posY, doublePrecision),
                    random.NextDouble() < propertiesOverlap
                        ? _getGpsPointFromExistingParcel(random, parcelsCount)
                        : _getRandomGpsPoint(random, posX, posY, doublePrecision)
                );
            }
        }

        private GpsPoint _getGpsPointFromExistingParcel(Random random, int parcelsCount)
        {
            var parcelIndex = random.Next(0, parcelsCount);

            var parcel = _treeParcels
                            .ElementAt(parcelIndex)
                            .Item;

            if (parcel is null)
            {
                throw new Exception($"Parcel at index {parcelIndex} cant be found");
            }

            return random.NextDouble() < 0.5
                ? parcel.PositionA ?? throw new Exception("Parcel must have PositionA")
                : parcel.PositionB ?? throw new Exception("Parcel must have PositionB")
            ;
        }

        private GpsPoint _getRandomGpsPoint(Random random, (int min, int max) posX, (int min, int max) posY, int precision = 2)
        {
            return new GpsPoint(
                _getRandomDouble(random, posX, precision),
                _getRandomDouble(random, posY, precision)
            );
        }

        private double _getRandomDouble(Random random, (int min, int max) range, int precision = 2)
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

        private IList<T> _findItems<T>(GpsPoint point, KdTree<GpsPointItem<T>> tree) where T : GeoItem
        {
            var data = tree.Find(new GpsPointItem<T>(point, default))
                            .Where(item => item.Item is not null)
                            .Select(item => item.Item);

            // can use ! because the data is not null
            return data!.ToList<T>();
        }
        #endregion

        #region Deleting

        public void ClearData()
        {
            _treeProperties.Clear();
            _treeParcels.Clear();
            _treeCombined.Clear();
        }

        public void DeleteParcel(Parcel parcel)
        {
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
                        tree.RemoveSpecific(new GpsPointItem<T>(gpsPoint, itemByGpsPos));
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
