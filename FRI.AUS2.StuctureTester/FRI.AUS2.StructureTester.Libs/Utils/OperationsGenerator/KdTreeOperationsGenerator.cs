using FRI.AUS2.Libs.Structures.Trees;
using FRI.AUS2.Libs.Structures.Trees.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRI.AUS2.StructureTester.Libs.Utils.OperationsGenerator
{
    public class KdTreeOperationsGenerator<T> : OperationsGenerator<T> where T : class, IKdTreeData
    {
        public KdTree<T> KdTree { get; init; }

        public override int StructureItemsCount => KdTree.NodesCount;

        public override string LogsFileNamePrefix => "kdtree";

        private Func<Random, T?, T> _craeteRandomT;

        public KdTreeOperationsGenerator(KdTree<T> structure, int count, int seed, Func<Random, T?, T> craeteRandomT) : base(count, seed)
        {
            KdTree = structure;
            _craeteRandomT = craeteRandomT;
        }


        protected override string _getStructureStatictics()
        {
            return $"Nodes count: {KdTree.NodesCount}, Depth: {KdTree.Depth}, Root: {KdTree.RootNode?.Data?.ToString() ?? "null"}";
        }

        protected override void _structureInsert(T t)
        {
            KdTree.Insert(t);
        }

        protected override T _createRandomT(Random random, T? filter)
        {
            return _craeteRandomT(random, filter);
        }

        protected override void _structureRemove(T filter)
        {
            KdTree.Remove(filter, true);
        }

        protected override void _structureRemoveSpecific(T filter)
        {
            KdTree.RemoveSpecific(filter, true);
        }

        protected override IList<T> _structureFind(T filter)
        {
            return KdTree.Find(filter);
        }

        protected override IList<T> _structureFindSpecific(T filter)
        {
            return KdTree.FindSpecific(filter);
        }

        protected override IList<T> _findAllData(T filter, bool specific)
        {
            return _structureData.FindAll(
                x => KdTree<T>.CompareAllDimensions(x, filter)
                    && (specific
                        ? x.Equals(filter)
                        : true));
        }

        protected override void _initializeStructureData()
        {
            _structureData.AddRange(KdTree);
        }
    }
}
