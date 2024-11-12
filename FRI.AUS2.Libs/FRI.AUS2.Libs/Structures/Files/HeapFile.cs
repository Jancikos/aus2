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
        /// count of items that can be stored in the block
        /// </summary>
        /// <value></value>
        public int BlockFactor
        {
            get => Size / (TDataSize + 1);
        }

        /// <summary>
        /// number of valid items in the block (from the beginning of the block)
        /// </summary>
        /// <value></value>
        public int ValidCount { get;  set; }

        /// <summary>
        /// items stored in the block
        /// </summary>
        /// <value></value>
        public TData[] Items { get; set; }

        public int? PreviousBlock { get; protected set; }
        public int? NextBlock { get; protected set; }

        public int MetedataSize => 3 * sizeof(int);
        public int TDataSize =>  (new TData()).Size;
        public int DataSize => BlockFactor * TDataSize;

        private int _blockSize;
        /// <summary>
        /// size of the whole block in bytes
        /// </summary>
        /// <value></value>
        public int Size => _blockSize;

        public HeapFileBlock(int blockSize)
        {
            _blockSize = blockSize;

            Items = new TData[BlockFactor];
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
            byte[] buffer = new byte[Size];
            int offset = 0;

            // metadata
            BitConverter.GetBytes(ValidCount).CopyTo(buffer, offset);
            offset += sizeof(int);

            BitConverter.GetBytes(PreviousBlock ?? -1).CopyTo(buffer, offset);
            offset += sizeof(int);

            BitConverter.GetBytes(NextBlock ?? -1).CopyTo(buffer, offset);
            offset += sizeof(int);

            // data
            for (int i = 0; i < ValidCount; i++)
            {
                Items[i].ToBytes().CopyTo(buffer, offset);

                // if (i < ValidCount)
                // {
                //     Items[i].ToBytes().CopyTo(buffer, offset);
                // }
                // else
                // {
                //     new TData().ToBytes().CopyTo(buffer, offset);
                // }

                offset += TDataSize;
            }

            return buffer;
        }

        public void FromBytes(byte[] bytes)
        {
            ResetBlock();
            int offset = 0;

            // metadata
            ValidCount = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);

            PreviousBlock = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);

            NextBlock = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);

            // data
            for (int i = 0; i < ValidCount; i++)
            {
                TData item = new();
                item.FromBytes(bytes[offset..(offset + TDataSize)]);
                Items[i] = item;

                offset += TDataSize;
            }
        }
    }
}
