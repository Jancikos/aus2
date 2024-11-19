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

        private Dictionary<int, (int address, HeapData data)> _structureDataDict = new Dictionary<int, (int address, HeapData data)>();

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
            return $"FileSize: {HeapFile.FileSize}, BlocksCount: {HeapFile.BlocksCount}, ValidItemsCount: {HeapFile.ValidItemsCount}, NextFreeBlock: {HeapFile.NextFreeBlock?.ToString() ?? "?"} [{HeapFile.FreeBlocksCount}], NextEmptyBlock: {HeapFile.NextEmptyBlock?.ToString() ?? "?"} [{HeapFile.EmptyBlocksCount}]";
        }

        protected override void _initializeStructureData()
        {
            _structureData.Clear();
            _structureDataDict.Clear();

            var i = 1;
            foreach (var block in HeapFile.GetAllDataBlocks())
            {
                foreach (var item in block.ValidItems)
                {
                    _structureData.Add(item);
                    _structureDataDict.Add(item.Id, (HeapFile._getAddressByBlockIndex(i), item));
                }
                i++;
            }
        }

        protected override IList<HeapData> _structureFind(HeapData filter)
        {
            throw new NotImplementedException("Operation find all is not supported for HeapFile");
        }

        protected override IList<HeapData> _structureFindSpecific(HeapData filter)
        {
            return HeapFile.AllData.Where(x => x.Equals(filter)).ToList();
        }

        protected override void _structureInsert(HeapData t)
        {
            var address = HeapFile.Insert(t);
            _structureDataDict.Add(t.Id, (address, t));
        }

        protected override void _structureRemove(HeapData filter)
        {
            throw new NotImplementedException("Operation remove all is not supported for HeapFile");
        }

        protected override void _structureRemoveSpecific(HeapData filter)
        {
            var address = _structureDataDict[filter.Id].address;
            HeapFile.Delete(address, filter);
            _structureDataDict.Remove(filter.Id);
        }
    }
}
