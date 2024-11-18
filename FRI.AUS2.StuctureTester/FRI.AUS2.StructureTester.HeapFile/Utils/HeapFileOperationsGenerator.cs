using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRI.AUS2.Libs.Structures.Files;
using FRI.AUS2.StructureTester.HeapFileTester.Models;
using FRI.AUS2.StructureTester.Libs.Utils.OperationsGenerator;

namespace FRI.AUS2.StructureTester.HeapFileTester.Utils
{
    public class HeapFileOperationsGenerator : OperationsGenerator<HeapData>
    {

        public HeapFile<HeapData> HeapFile { get; init; }

        public override int StructureItemsCount => HeapFile.ValidItemsCount;

        public override string LogsFileNamePrefix => "heapfile";

        // toto prerobit na tabulku, kde bude HeapData.ID => adresa, objekt
        protected IList<int> _structureDataAddresses = new List<int>();
        // private Dictionary<int, (int address, T data)> _structureDataDict = new Dictionary<int, (int address, T data)>();
        

        public HeapFileOperationsGenerator(HeapFile<HeapData> structure) : base()
        {
            HeapFile = structure;
        }

        protected override HeapData _createRandomT(Random random, HeapData? filter)
        {
            var generator = new HeapDataGenerator(random);
            return generator.GenerateItem();
        }

        protected override IList<HeapData> _findAllData(HeapData filter, bool specific = false)
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

        protected override IList<HeapData> _structureFind(HeapData filter)
        {
            return HeapFile.AllData.Where(x => x.Equals(filter)).ToList();
        }

        protected override IList<HeapData> _structureFindSpecific(HeapData filter)
        {
            throw new NotImplementedException();
        }

        protected override void _structureInsert(HeapData t)
        {
            var address = HeapFile.Insert(t);
            _structureDataAddresses.Add(address);
        }

        protected override void _structureRemove(HeapData filter)
        {
            var address = _popAddressByData(filter);
            HeapFile.Delete(address, filter);
        }

        protected override void _structureRemoveSpecific(HeapData filter)
        {
            throw new NotImplementedException();
        }

        protected int _getAddressByData(HeapData data)
        {
            return _structureDataAddresses[_structureData.IndexOf(data)];
        }
        protected int _popAddressByData(HeapData data)
        {
            var index = _structureData.IndexOf(data);
            var address = _structureDataAddresses[index];
            _structureDataAddresses.RemoveAt(index);
            return address;
        }
    }
}
