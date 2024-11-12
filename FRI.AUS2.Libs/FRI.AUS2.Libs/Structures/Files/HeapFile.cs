﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRI.AUS2.Libs.Helpers;

namespace FRI.AUS2.Libs.Structures.Files
{
    public class HeapFile<TData> : IBinaryData where TData : IHeapFileData, new()
    {
        protected BinaryFileManager _fileManager;
        public int BlockSize { get; private set; }

        protected int? NextFreeBlock { get; set; } = null;
        protected int? NextEmptyBlock { get; set; } = null;

        protected int ActiveBlockAddress { get; set; }
        public HeapFileBlock<TData> ActiveBlock { get; protected set; }

        public int Size => _fileManager.Length;

        public HeapFile(int blockSize, FileInfo file)
        {
            _fileManager = new BinaryFileManager(file);
            BlockSize = blockSize;

            ActiveBlock = new HeapFileBlock<TData>(BlockSize);

            if (!file.Exists || _fileManager.Length == 0) {
                // init structure metadata
                _saveMetadata();
            } else {
                // load metadata from first file block
                FromBytes(_fileManager.ReadBytes(0, BlockSize));
            }
        }

        #region Insert
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns>address of the block where the data was inserted</returns>
        public int Insert(TData data)
        {
            var (address, addressType) = _findAddressOfNextFreeBlock();

            _loadActiveBlock(address);

            ActiveBlock.AddItem(data);

            switch (addressType)
            {
                case BlockAdressType.NextFreeBlock:
                    if (ActiveBlock.IsFull)
                    {
                        _dequeNextFreeBlock();
                        _saveMetadata();
                    }
                    break;
                case BlockAdressType.NextEmptyBlock:
                    if (!ActiveBlock.IsEmpty)
                    {
                        _dequeNextEmptyBlock();
                        _saveMetadata();
                    }
                    break;
                case BlockAdressType.NewBlock:
                    if (!ActiveBlock.IsFull)
                    {
                        _enqueNextFreeBlock();
                        _saveMetadata();
                    }
                    break;
            }

            _saveActiveBlock();

            return address;
        }

        #endregion

        #region Blocks management

        private void _enqueNextFreeBlock()
        {
            ActiveBlock.NextBlock = NextFreeBlock;
            NextFreeBlock = ActiveBlockAddress;
        }

        private void _dequeNextFreeBlock()
        {
            NextFreeBlock = ActiveBlock.NextBlock;
            ActiveBlock.NextBlock = null;
        }

        private void _enqueNextEmptyBlock()
        {
            ActiveBlock.NextBlock = NextEmptyBlock;
            NextEmptyBlock = ActiveBlockAddress;
        }

        private void _dequeNextEmptyBlock()
        {
            NextEmptyBlock = ActiveBlock.NextBlock;
            ActiveBlock.NextBlock = null;
        }

        private void _loadActiveBlock(int address)
        {
            if (ActiveBlockAddress == address)
            {
                return;
            }

            ActiveBlock.FromBytes(_fileManager.ReadBytes(address, BlockSize));
            ActiveBlockAddress = address;
        }

        private void _saveActiveBlock()
        {
            _fileManager.WriteBytes(ActiveBlockAddress, ActiveBlock.ToBytes());
        }

        private (int address, BlockAdressType adressType) _findAddressOfNextFreeBlock()
        {
            if (NextFreeBlock is not null)
            {
                return (NextFreeBlock.Value, BlockAdressType.NextFreeBlock);
            }

            if (NextEmptyBlock is not null)
            {
                return (NextEmptyBlock.Value, BlockAdressType.NextEmptyBlock);
            }

            return (_fileManager.Length, BlockAdressType.NewBlock);
        }


        #endregion

        #region Bytes conversion

        protected void _saveMetadata()
        {
            _fileManager.WriteBytes(0, ToBytes());
        }

        /// <summary>
        /// saves metadata to the bytes array
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            byte[] buffer = new byte[BlockSize];
            int offset = 0;

            BitConverter.GetBytes(NextFreeBlock ?? -1).CopyTo(buffer, offset);
            offset += sizeof(int);

            BitConverter.GetBytes(NextEmptyBlock ?? -1).CopyTo(buffer, offset);
            offset += sizeof(int);

            return buffer;
        }

        public void FromBytes(byte[] bytes)
        {
            int offset = 0;

            int nextFreeBlock = BitConverter.ToInt32(bytes, offset);
            NextFreeBlock = nextFreeBlock == -1 ? null : nextFreeBlock;
            offset += sizeof(int);

            int nextEmptyBlock = BitConverter.ToInt32(bytes, offset);
            NextEmptyBlock = nextEmptyBlock == -1 ? null : nextEmptyBlock;
            offset += sizeof(int);
        }
        #endregion
    }

    #region HeapFileBlock

    public enum BlockAdressType
    {
        NextFreeBlock,
        NextEmptyBlock,
        NewBlock
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

        public int? PreviousBlock { get; set; }
        public int? NextBlock { get; set; }

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

        #region Items

        public bool IsFull => ValidCount >= BlockFactor;
        public bool IsEmpty => ValidCount == 0;

        public void AddItem(TData item)
        {
            if (ValidCount >= BlockFactor)
            {
                throw new InvalidOperationException("Block is full");
            }

            Items[ValidCount++] = item;
        }

        public void RemoveItem(int index)
        {
            if (index < 0 || index >= ValidCount)
            {
                throw new IndexOutOfRangeException("Index out of range");
            }

            for (int i = index; i < ValidCount - 1; i++)
            {
                Items[i] = Items[i + 1];
            }

            ValidCount--;
        }


        #endregion

        #region Bytes conversion

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
        #endregion
    }
    #endregion
}
