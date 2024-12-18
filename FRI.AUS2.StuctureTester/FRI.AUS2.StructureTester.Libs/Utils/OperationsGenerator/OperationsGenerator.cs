﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRI.AUS2.StructureTester.Libs.Utils.OperationsGenerator
{
    public abstract class OperationsGenerator<T> where T : class
    {
        /// <summary>
        /// total count of operations
        /// </summary>
        /// <value></value>
        public int Count { get; set; }

        public abstract int StructureItemsCount { get; }

        private int _seed;
        public int Seed
        {
            get
            {
                return _seed;
            }
            set
            {
                _seed = value;
                _random = new Random(value);
            }
        }

        public Uri LogsPath { get; set; } = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\AUS2\");
        public abstract string LogsFileNamePrefix { get; }
        public string LogsFileName { get => $"{LogsFileNamePrefix}-operations.log"; }
        public Uri LogsFileUri => new Uri(LogsPath + @"\" + LogsFileName);
        public int LogsVerbosity { get; set; } = 1;
        public int LogsStatsFrequency { get; set; } = 0;

        private Random _random;
        private List<OperationType> _randomOperations;

        protected List<T> _structureData;
        private List<T> _structureDataWithFindProblems;
        protected bool _foundUpdateProblem = false;
        private string? _structureDataStaticticsBefore;
        private int? _structureItemsCountBefore;
        private int _expectedItemsCount;
        private Dictionary<OperationType, int> _operationStatistics = new Dictionary<OperationType, int>();

        public OperationsGenerator()
        {
            Count = 10;
            Seed = 0;
            _random = new Random(Seed);
            _randomOperations = new List<OperationType>();
            _structureData = new List<T>();
            _structureDataWithFindProblems = new List<T>();
        }

        protected abstract void _initializeStructureData();

        protected abstract void _structureInsert(T t);

        /// <summary>
        /// T? is used because when we are inserting new item with the specified key
        /// </summary>
        protected abstract T _createRandomT(Random random, T? filter);
        /// <summary>
        /// used for updating item not key parts of the item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected T _changeItem(T item) => item;

        protected abstract void _structureRemove(T filter);
        protected abstract void _structureRemoveSpecific(T filter);
        protected virtual void _structureUpdate(T item, T updatedItem) => throw new NotImplementedException("UpdateStats is not implemented");

        protected abstract IList<T> _findAllData(T filter, bool specific = false);

        protected abstract IList<T> _structureFind(T filter);
        protected abstract IList<T> _structureFindSpecific(T filter);

        protected abstract string _getStructureStatictics();


        /// <summary>
        /// Add operation to the list of operations, that will be randomly used
        /// </summary>
        /// <param name="operation"></param>
        public void AddOperation(OperationType operation)
        {
            _randomOperations.Add(operation);
        }
        public void ResetOperations()
        {
            _randomOperations.Clear();
        }

        public void Generate()
        {
            var randomOperationsCount = _randomOperations.Count;

            _beforeGeneration();

            for (int i = 0; i < Count; i++)
            {
                OperationType operation = _structureData.Count == 0
                    ? OperationType.Insert
                    : _randomOperations[_random.Next(0, randomOperationsCount)];

                _beforeOperation(i, operation);
                _makeOperation(operation);
                _afterOperation(i, operation);
            }

            _afterGeneration();
        }


        private void _beforeGeneration()
        {
            // Get all data from the structure (to be able key all keys)
            _structureData = new List<T>();

            // Save structure data before operations
            _initializeStructureData();
            _structureDataStaticticsBefore = _getStructureStatictics();
            _structureItemsCountBefore = _expectedItemsCount = StructureItemsCount;

            // Initialize operation statistics
            _operationStatistics = new Dictionary<OperationType, int>
            {
                { OperationType.Insert, 0 },
                { OperationType.InsertDuplicate, 0 },
                { OperationType.Delete, 0 },
                { OperationType.DeleteSpecific, 0 },
                { OperationType.Update, 0 },
                { OperationType.Find, 0 },
                { OperationType.FindSpecific, 0 }
            };
            _structureDataWithFindProblems = new List<T>();

            // Create logs directory if not exists
            if (!System.IO.Directory.Exists(LogsPath.LocalPath))
            {
                System.IO.Directory.CreateDirectory(LogsPath.LocalPath);
            }
            // Create log file if not exists
            if (!System.IO.File.Exists(LogsFileUri.LocalPath))
            {
                System.IO.File.Create(LogsFileUri.LocalPath).Close();
            }

            // Clear log file
            System.IO.File.WriteAllText(LogsFileUri.LocalPath, string.Empty);

            _log("OperationsGenerator:");
            _log($"Seed: {Seed}", 1);
            _log($"Operations count: {Count}", 1);
            _log($"Operations ratio: {string.Join(", ", _randomOperations)}", 1);
            _log("");
            _log("Structure:");
            _log(_getStructureStatictics(), 1);
        }

        protected virtual void _afterGeneration()
        {
            _log("");
            _log("Operations generation finished");

            _log("Structure statistics:");
            _log("Before:", 1);
            _log(_structureDataStaticticsBefore ?? "null", 2);
            _log("After:", 1);
            _log(_getStructureStatictics(), 2);
            _log("");

            _log("Structure test:");
            _log("Before items count: " + (_structureItemsCountBefore ?? 0), 1);
            _log($"Operations done: {Count}", 1);
            foreach (var operation in _operationStatistics)
            {
                _log($"{operation.Key}: {operation.Value}", 2);
            }
            _log("");

            _log($"After items count should be: {_expectedItemsCount}", 1);
            _log("After items count is: " + StructureItemsCount, 1);
            _log(_expectedItemsCount != StructureItemsCount
                ? "!!! Items count is not correct !!!"
                : "Items count is correct",
                2
            );

            if (_structureDataWithFindProblems.Count > 0)
            {
                _log("!!! Find problems !!!", 1);
                _log($"Found {_structureDataWithFindProblems.Count} problems", 2);
                _log(string.Join(", ", _structureDataWithFindProblems.Select(x => x.ToString())), 2);
            }
            else
            {
                _log("No find problems detected", 1);
            }

            if (_foundUpdateProblem)
            {
                _log("!!! Find update problems !!!", 1);
                _log("You should check which items failed to update", 2);
            }
            else
            {
                _log("No update problems detected", 1);
            }
        }

        private void _beforeOperation(int index, OperationType operation)
        {
            _log($"#{index} - {operation}", 0, 2);
        }

        private void _makeOperation(OperationType operation)
        {
            switch (operation)
            {
                case OperationType.Insert:
                    _makeInsert();
                    break;
                case OperationType.InsertDuplicate:
                    _makeInsert(_getRandomKey());
                    break;
                case OperationType.Delete:
                    _makeDelete();
                    break;
                case OperationType.DeleteSpecific:
                    _makeDelete(true);
                    break;
                case OperationType.Find:
                    _makeFind();
                    break;
                case OperationType.FindSpecific:
                    _makeFind(true);
                    break;
                case OperationType.Update:
                    _makeUpdate();
                    break;
                default:
                    throw new NotImplementedException($"{operation} is not implemented");
            }

            _operationStatistics[operation]++;
        }

        protected virtual void _afterOperation(int index, OperationType operation)
        {
            if (LogsStatsFrequency > 0)
            {
                if (index % LogsStatsFrequency == 0)
                {
                    _log("");
                    _log($"#{index} structure statistics:");
                    _log(_getStructureStatictics(), 1);
                    _log("");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter">ak je zadany, tak urcuje kluc prvku, ktory bude vlozeny</param>
        private void _makeInsert(T? filter = null)
        {
            var itemsCountBefore = StructureItemsCount;
            var t = _createRandomT(_random, filter);

            _log($"Inserting: {t}" + (filter is not null ? " (DUPLICATE)" : ""), 1, 2);

            ++_expectedItemsCount;
            _structureInsert(t);
            _structureData.Add(t);

            _checkItemsCount(itemsCountBefore + 1);

        }


        private void _makeDelete(bool specific = false)
        {
            var filter = _getRandomKey();
            if (filter is null)
            {
                _log("No key to delete", 1, 2);
                // structure is empty
                return;
            }

            try
            {
                _log($"Deleting {(specific ? "specific" : "all")}: {filter}", 1, 2);

                var itemsCountBefore = StructureItemsCount;
                var itemsFromList = _findAllData(filter, specific);
                _log($"Found: {itemsFromList.Count} items in list", 1, 2);

                _expectedItemsCount -= itemsFromList.Count;

                Action<T> removeAction = specific
                    ? _structureRemoveSpecific
                    : _structureRemove;
                removeAction(filter);

                _structureData.RemoveAll(itemsFromList.Contains);

                _checkItemsCount(itemsCountBefore - itemsFromList.Count);
            }
            catch (InvalidOperationException e)
            {
                // key not found
                _log($"Key not found!!! ({e.Message})", 1, 2);
            }
        }

        private void _makeUpdate()
        {
            var filter = _getRandomKey();
            if (filter is null)
            {
                _log("No key to update", 1, 2);
                // structure is empty
                return;
            }

            _log($"Updating: {filter}", 1, 2);

            var itemsFromList = _findAllData(filter, true);
            _log($"Found {itemsFromList.Count} items in list with that key", 2, 2);

            if (itemsFromList.Count == 0)
            {
                // key not found
                _log("Key not found in help strucuture!!!", 1, 2);
                return;
            }

            var item = itemsFromList.First();
            var updatedItem = _changeItem(item);

            _log($"Updating data: {item} -> {updatedItem}", 2, 2);

            _structureUpdate(item, updatedItem);
            
            _structureData.Remove(item);
            _structureData.Add(updatedItem);

            _checkItemsCount(StructureItemsCount);

            // try find updated item
            var result = _structureFindSpecific(updatedItem);
            if (result.Count == 0)
            {
                _log("Updated item not found !!!", 1, 2);
                _foundUpdateProblem = true;
                return;
            }
            _log("Updated item found", 1, 2);
            _log(result.First().ToString() ?? "null", 2, 2);

            // check if updated item is the same as in the list
            if (!_compareItems(result.First(), updatedItem))
            {
                _log("Updated item is not the same as in the list !!!", 1, 2);
                _foundUpdateProblem = true;
                return;
            }
        }

        private void _makeFind(bool specific = false)
        {
            var filter = _getRandomKey();
            if (filter is null)
            {
                // structure is empty
                return;
            }

            _log($"Finding {(specific ? "specific" : "")}: {filter}", 1, 2);

            // check if all found items are the same as in the list
            var itemsFromList = _findAllData(filter, specific);
            _log($"Found: {itemsFromList.Count} items in list", 1, 2);
            _log(string.Join(", ", itemsFromList.Select(x => x.ToString())), 2, 2);

            var result = specific
                ? _structureFindSpecific(filter)
                : _structureFind(filter);
            if (result.Count == 0)
            {
                // key not found
                _log("Key not found!!!", 1, 2);
                return;
            }

            _log($"Found: {result.Count} items in tree", 1, 2);
            _log(string.Join(", ", result.Select(x => x.ToString())), 2, 2);


            if (itemsFromList.Count != result.Count)
            {
                _log("Items count in tree and list is not the same !!!", 1, 2);
                _structureDataWithFindProblems.Add(filter);
            }

            if (specific)
            {
                result.ToList().ForEach(x =>
                {
                    if (!_compareItems(x, filter))
                    {
                        _log("Found item is not the same as searched item !!!", 1, 2);
                        _structureDataWithFindProblems.Add(filter);
                    }
                });
            }

        }

        protected virtual bool _compareItems(T item1, T item2)
        {
            return item1.Equals(item2);
        }

        private T? _getRandomKey()
        {
            if (_structureData.Count == 0)
            {
                return null;
            }

            return _structureData[_random.Next(0, _structureData.Count)];
        }

        private void _checkItemsCount(int expectedItemCount)
        {
            if (expectedItemCount != StructureItemsCount)
            {
                _log($"!!! Items count is not correct !!!", 1, 2);
                _log($"Expected: {expectedItemCount}, Actual: {StructureItemsCount}", 2, 2);
            }
        }

        protected void _log(string message, int indentLevel = 0, int verbosityLevel = 0)
        {
            var logMessage = $"{new string(' ', indentLevel * 2)}{message}" + Environment.NewLine;

            // premysliet ci to neurobit efektivnejsie (vzhladom na to, ze sa bude casto otvarat a zatvarat subor)
            if (verbosityLevel >= LogsVerbosity || verbosityLevel == 0)
            {
                System.IO.File.AppendAllText(LogsFileUri.LocalPath, logMessage);
            }

            Debug.WriteLine(logMessage);
        }
    }

    public enum OperationType
    {
        Insert,
        InsertDuplicate,
        Delete,
        DeleteSpecific,
        Find,
        FindSpecific,
        Update
    }
}
