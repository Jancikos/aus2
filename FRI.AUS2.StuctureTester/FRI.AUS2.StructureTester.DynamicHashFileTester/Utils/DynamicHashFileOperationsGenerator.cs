using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRI.AUS2.Libs.Structures.Files;
using FRI.AUS2.StructureTester.HeapFileTester.Models;
using FRI.AUS2.StructureTester.HeapFileTester.Utils;
using FRI.AUS2.StructureTester.Libs.Utils.OperationsGenerator;

namespace FRI.AUS2.StructureTester.DynamicHashFileTester.Utils
{
    internal class DynamicHashFileOperationsGenerator : HeapFileOperationsGenerator
    {
        public DynamicHashFile<HeapData> Structure { get; init; }


        public override string LogsFileNamePrefix => "dynamicHashFile";

        public DynamicHashFileOperationsGenerator(DynamicHashFile<HeapData> structure) : base(structure.HeapFile)
        {
            Structure = structure;
        }

        protected override string _getStructureStatictics()
        {
            return $"Depth: {Structure.Depth}, {base._getStructureStatictics()}";
        }

        protected override IList<HeapData> _structureFindSpecific(HeapData filter)
        {
            return [Structure.Find(filter)];
        }

        protected override void _structureInsert(HeapData t)
        {
            Structure.Insert(t);
        }

        protected override void _structureRemoveSpecific(HeapData filter)
        {
            Structure.Delete(filter);
        }
    }
}
