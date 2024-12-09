using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRI.AUS2.Libs.Helpers;
using FRI.AUS2.Libs.Structures.Files;
using FRI.AUS2.StructureTester.HeapFileTester.Models;
using FRI.AUS2.StructureTester.HeapFileTester.Utils;
using FRI.AUS2.StructureTester.Libs.Utils.OperationsGenerator;

namespace FRI.AUS2.StructureTester.ExtendableHashFileTester.Utils
{
    internal class ExtendableHashFileOperationsGenerator : HeapFileOperationsGenerator
    {
        public ExtendableHashFile<HeapData> Structure { get; init; }


        public override string LogsFileNamePrefix => "ExtendableHashFile";

        public ExtendableHashFileOperationsGenerator(ExtendableHashFile<HeapData> structure) : base(structure.HeapFile)
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
            _logAddressIndex(t);
            Structure.Insert(t);
        }

        protected override void _structureRemoveSpecific(HeapData filter)
        {
            _logAddressIndex(filter);
            Structure.Delete(filter);
        }

        private void _logAddressIndex(HeapData filter)
        {
            var index = Structure._getAddressIndex(filter.GetHash());
            _log($"Address index: {index}", 1);
        }

        protected override void _structureUpdate(HeapData filter, HeapData updateItem)
        {
            Structure.Update(filter, updateItem);
        }

        protected override void _afterOperation(int index, OperationType operation)
        {
            base._afterOperation(index, operation);

            if (operation == OperationType.Find || operation == OperationType.FindSpecific)
            {
                return;
            }

            // _printAddress();
            // _checkValidCounts();
        }

        protected override void _afterGeneration()
        {
            base._afterGeneration();

            _checkValidCounts();
        }

        private void _printAddress()
        {
            _log($"Addresses ({Structure.Depth}):", 1, 3);
            int i = 0;
            foreach (var ehfBlock in Structure.Addresses)
            {
                _log($"{i}. {i.ToBinaryString(Structure.Depth, false)}: {ehfBlock}", 2, 3);
                i++;
            }
        }

        protected void _checkValidCounts()
        {
            bool problem = false;
            int i = 0;
            foreach (var ehfBlock in Structure.Addresses)
            {
                if (ehfBlock.ValidCount != (ehfBlock.Block?.ValidCount ?? 0))
                {
                    _log($"{i}. EhfBlock valid count mismatch: {ehfBlock.ValidCount} != {ehfBlock.Block?.ValidCount.ToString() ?? "NULL"}");
                    problem = true;
                }

                i++;
            }

            _log(problem 
                ? "!!! Valid counts mismatch"
                : "Valid counts OK"
            );
        }
    }
}
