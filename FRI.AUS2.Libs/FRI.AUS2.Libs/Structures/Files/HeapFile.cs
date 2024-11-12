using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRI.AUS2.Libs.Structures.Files
{
    public class HeapFile<TData> where TData : IHeapFileData, new()
    {
        public int BlockSize { get; private set; }

        public HeapFileBlock<TData> ActiveBlock { get; protected set; }

        public HeapFile(int blockSize)
        {
            BlockSize = blockSize;

            ActiveBlock = new HeapFileBlock<TData>(BlockSize);
        }

    }

    public class HeapFileBlock<TData> : IBinaryData where TData : IHeapFileData, new()
    {
        /// <summary>
        /// size of the whole block in bytes
        /// </summary>
        /// <value></value>
        public int BlockSize { get; private init; }

        /// <summary>
        /// count of items that can be stored in the block
        /// </summary>
        /// <value></value>
        public int BlockFactor
        {
            get => BlockSize / (TDataSize + 1);
        }

        /// <summary>
        /// number of valid items in the block (from the beginning of the block)
        /// </summary>
        /// <value></value>
        public int ValidCount { get; protected set; }

        /// <summary>
        /// items stored in the block
        /// </summary>
        /// <value></value>
        protected TData[] Items { get; set; }

        public int? PreviousBlock { get; protected set; }
        public int? NextBlock { get; protected set; }

        public int MetedataSize => 3 * sizeof(int);
        public int TDataSize =>  (new TData()).Size;
        public int DataSize => BlockFactor * TDataSize;
        public int Size => BlockSize;

        public HeapFileBlock(int blockSize)
        {
            BlockSize = blockSize;

            ResetBlock();
        }

        public void ResetBlock()
        {
            ValidCount = 0;
            Items = new TData[BlockFactor];
            PreviousBlock = null;
            NextBlock = null;
        }

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }

        public void FromBytes(byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
