using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRI.AUS2.Libs.Structures.Files;

namespace FRI.AUS2.StructureTester.Libs.Utils.OperationsGenerator
{
    public class HeapFileOperationsGenerator<T> : OperationsGenerator<T> where T : class, IHeapFileData, new()
    {

        public HeapFile<T> HeapFile { get; init; }

        private Func<Random, T?, T> _craeteRandomT;

        public override int StructureItemsCount => HeapFile.ValidItemsCount;

        public override string LogsFileNamePrefix => "heapfile";

        protected IList<int> _structureDataAddresses = new List<int>();

        public HeapFileOperationsGenerator(HeapFile<T> structure, Func<Random, T?, T> craeteRandomT) : base()
        {
            HeapFile = structure;
            _craeteRandomT = craeteRandomT;
        }

        protected override T _createRandomT(Random random, T? filter)
        {
            return _craeteRandomT(random, filter);
        }

        protected override IList<T> _findAllData(T filter, bool specific = false)
        {
            return _structureData.FindAll(x => x.Equals(filter));
        }

        protected override string _getStructureStatictics()
        {
            return "TODO";
        }

        protected override void _initializeStructureData()
        {
            _structureData.Clear();
            _structureDataAddresses.Clear();

            var i = 1;
            foreach (var block in HeapFile.GetAllDataBlocks())
            {
                foreach (var item in block.Items)
                {
                    _structureData.Add(item);
                    _structureDataAddresses.Add(HeapFile._getAddressByBlockIndex(i));
                }
                i++;
            }

            Debug.WriteLine($"Data count: {_structureData.Count}");
        }

        protected override IList<T> _structureFind(T filter)
        {
            return HeapFile.AllData.Where(x => x.Equals(filter)).ToList();
        }

        protected override IList<T> _structureFindSpecific(T filter)
        {
            throw new NotImplementedException();
        }

        protected override void _structureInsert(T t)
        {
            var address = HeapFile.Insert(t);
            _structureDataAddresses.Add(address);
        }

        protected override void _structureRemove(T filter)
        {
            var address = _popAddressByData(filter);
            HeapFile.Delete(address, filter);
        }

        protected override void _structureRemoveSpecific(T filter)
        {
            throw new NotImplementedException();
        }

        protected int _getAddressByData(T data)
        {
            return _structureDataAddresses[_structureData.IndexOf(data)];
        }
        protected int _popAddressByData(T data)
        {
            var index = _structureData.IndexOf(data);
            var address = _structureDataAddresses[index];
            _structureDataAddresses.RemoveAt(index);
            return address;
        }
    }
}
