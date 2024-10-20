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

        private Random _random;
        private List<OperationType> _randomOperations;
        Func<Random, T> _craeteRandomT;

        private List<T> _structureData;

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

                _beforeOperation(operation);
                _makeOperation(operation);
                _afterOperation(operation);
            }
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

        }

        private void _beforeOperation(OperationType operation)
        {
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
        }

        private void _afterOperation(OperationType operation)
        {
        }

        private void _makeInsert()
        {
            Structure.Insert(_craeteRandomT(_random));
        }

        private void _makeDelete()
        {
            var filter = _getRandomKey();
            if (filter is null)
            {
                // structure is empty
                return;
            }

            try
            {
                Structure.RemoveException(filter);
            }
            catch (InvalidOperationException e)
            {
                // key not found
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

            var result = Structure.Find(filter);
            if (result.Count == 0)
            {
                // key not found
            }
        }


        private T? _getRandomKey()
        {
            if (_structureData.Count == 0)
            {
                return null;
            }

            return _structureData[_random.Next(0, _structureData.Count)];
        }
    }

    public enum OperationType
    {
        Insert,
        Delete,
        Find
    }
}
