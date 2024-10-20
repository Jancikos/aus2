using FRI.AUS2.Libs.Structures.Trees;
using FRI.AUS2.Libs.Structures.Trees.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace FRI.AUS2.StuctureTester.Utils
{
    class OperationsGenerator<T> where T : class, IKdTreeData
    {
        public KdTree<T> Structure { get; init; }

        public int Count { get; init; }

        private int _seed;
        public int Seed
        {
            get
            {
                return _seed;
            }
            init
            {
                _seed = value;
                _random = new Random(value);
            }
        }
        
        public Uri LogsPath { get; set; } = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\AUS2\");
        public string LogsFileName { get; set; } = "operations.log";
        public Uri LogsFileUri => new Uri(LogsPath + @"\" + LogsFileName);
        private Random _random;
        private List<OperationType> _randomOperations;
        Func<Random, T> _craeteRandomT;

        private List<T> _structureData;
        private string? _structureDataStaticticsBefore;
        private Dictionary<OperationType, int> _operationStatistics = new Dictionary<OperationType, int>();

        public OperationsGenerator(KdTree<T> structure, int count, int seed, Func<Random, T> craeteRandomT )
        {
            Structure = structure;
            Count = count;
            Seed = seed;
            _random = new Random(seed);
            _randomOperations = new List<OperationType>();
            _structureData = new List<T>();
            _craeteRandomT = craeteRandomT;
        }

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
                OperationType operation = _randomOperations[_random.Next(0, randomOperationsCount)];

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

            if (Structure?.RootNode?.Data is null)
            {
                return;
            }

            var it = Structure.GetInOrderIterator(Structure.RootNode.Data);
            if (it is null)
            {
                return;
            }

            while (it.MoveNext())
            {
                _structureData.Add(it.Current);
            }
            _structureDataStaticticsBefore = _getStructureStatictics();

            // Initialize operation statistics
            _operationStatistics = new Dictionary<OperationType, int>
            {
                { OperationType.Insert, 0 },
                { OperationType.Delete, 0 },
                { OperationType.Find, 0 }
            };

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
            _log($"Operations: {string.Join(", ", _randomOperations)}", 1);
            _log("");
            _log("Structure:");
            _log(_getStructureStatictics(), 1);
        }

        private void _afterGeneration()
        {
            _log("");
            _log("Operations generation finished");
            _log("");
            _log("Operation statistics:");
            foreach (var operation in _operationStatistics)
            {
                _log($"{operation.Key}: {operation.Value}", 1);
            }

            _log("Structure statistics:");
            _log("Before:", 1);
            _log(_structureDataStaticticsBefore ?? "null", 2);
            _log("After:", 1);
            _log(_getStructureStatictics(), 2);
            _log("");
        }

        private void _beforeOperation(int index, OperationType operation)
        {
            _log($"#{index} - {operation}");
        }

        private void _makeOperation(OperationType operation)
        {
            switch (operation)
            {
                case OperationType.Insert:
                    _makeInsert();
                    break;
                case OperationType.Delete:
                    _makeDelete();
                    break;
                case OperationType.Find:
                    _makeFind();
                    break;
                default:
                    throw new NotImplementedException($"{operation} is not implemented");
            }

            _operationStatistics[operation]++;
        }

        private void _afterOperation(int index, OperationType operation)
        {
            if (index % 5 == 0)
            {
                _log("");
                _log("Structure statistics:");
                _log(_getStructureStatictics(), 1);
                _log("");
            }
        }

        private void _makeInsert()
        {
            var t = _craeteRandomT(_random);

            _log($"Inserting: {t}", 1);

            Structure.Insert(t);
            _structureData.Add(t);
        }

        private void _makeDelete()
        {
            var filter = _getRandomKey();
            if (filter is null)
            {
                _log("No key to delete", 1);
                // structure is empty
                return;
            }

            try
            {
                _log($"Deleting: {filter}", 1);
                Structure.RemoveException(filter);
                _structureData.Remove(filter);
            }
            catch (InvalidOperationException e)
            {
                // key not found
                _log($"Key not found!!! ({e.Message})", 1);
            }
        }

        private void _makeFind()
        {
            var filter = _getRandomKey();
            if (filter is null)
            {
                // structure is empty
                return;
            }

            _log($"Finding: {filter}", 1);
            var result = Structure.Find(filter);
            if (result.Count == 0)
            {
                // key not found
                _log("Key not found!!!", 1);
                return;
            }

            _log($"Found: {result.Count} items", 1);
            _log(string.Join(", ", result.Select(x => x.ToString())), 2);
        }

        private T? _getRandomKey()
        {
            if (_structureData.Count == 0)
            {
                return null;
            }

            return _structureData[_random.Next(0, _structureData.Count)];
        }

        private void _log(string message, int indentLevel = 0)
        {
            // premysliet ci to neurobit efektivnejsie (vzhladom na to, ze sa bude casto otvarat a zatvarat subor)

            System.IO.File.AppendAllText(LogsFileUri.LocalPath, $"{new string(' ', indentLevel * 2)}{message}" + Environment.NewLine);
        }

        private string _getStructureStatictics()
        {
            return $"Nodes count: {Structure.NodesCount}, Depth: {Structure.Depth}, Root: {Structure.RootNode?.Data?.ToString() ?? "null"}";
        }
    }

    public enum OperationType
    {
        Insert,
        Delete,
        Find
    }
}
