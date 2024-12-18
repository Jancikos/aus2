﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRI.AUS2.Libs.Helpers;
using FRI.AUS2.Libs.Structures.Files;

namespace FRI.AUS2.StructureTester.HeapFileTester.Models
{
    public class HeapData : IHeapFileData, IExtendableHashFileData
    {
        private const int _firstnameMax = 15;
        private string _firstname = "";
        public string Firstname
        {
            get => _firstname;
            set
            {
                if (value.Length > _firstnameMax)
                {
                    throw new ArgumentException($"Firstname is max {_firstnameMax} characters long");
                }
                _firstname = value;
            }
        }

        private const int _lastnameMax = 20;
        private string _lastname = "";
        public string Lastname
        {
            get => _lastname;
            set
            {
                if (value.Length > _lastnameMax)
                {
                    throw new ArgumentException($"Lastname is max {_lastnameMax} characters long");
                }
                _lastname = value;
            }
        }

        public int Id;

        public const int ItemsMaxCount = 5;
        private List<NesteHeapDataItem> _items = new List<NesteHeapDataItem>(ItemsMaxCount);
        public List<NesteHeapDataItem> Items
        {
            get => _items;
            set
            {
                if (value.Count > ItemsMaxCount)
                {
                    throw new ArgumentException($"Items count is max {ItemsMaxCount}");
                }
                _items = value;
            }
        }

        
        private const int _ecvMax = 10;
        private string _ecv = "";
        public string ECV
        {
            get => _ecv;
            set
            {
                if (value.Length > _ecvMax)
                {
                    throw new ArgumentException($"ecv is max {_ecvMax} characters long");
                }
                _ecv = value;
            }
        }

        /// <summary>
        /// Id (int) + Firstname (int length + _firstnameMax bytes) + Lastname (int length + _lastnameMax bytes) + Items (int actualLength + _itemsMax * NesteHeapDataItemSize)
        /// </summary>
        /// <returns></returns>
        public int Size => sizeof(int) + sizeof(int) + _firstnameMax + sizeof(int) + _lastnameMax + sizeof(int) + ItemsMaxCount * (new NesteHeapDataItem()).Size;

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[Size];
            int offset = 0;

            // Id (4 bytes)
            BitConverter.GetBytes(Id).CopyTo(buffer, offset);
            offset += sizeof(int);

            // Firstname (4 bytes (length) + 15 bytes)
            BitConverter.GetBytes(Firstname.Length).CopyTo(buffer, offset);
            offset += sizeof(int);
            Encoding.ASCII.GetBytes(Firstname.PadRight(_firstnameMax)).CopyTo(buffer, offset);
            offset += _firstnameMax;

            // Lastname (4 bytes (length) + 20 bytes)
            BitConverter.GetBytes(Lastname.Length).CopyTo(buffer, offset);
            offset += sizeof(int);
            Encoding.ASCII.GetBytes(Lastname.PadRight(_lastnameMax)).CopyTo(buffer, offset);
            offset += _lastnameMax;

            // ECV (4 bytes (length) + 10 bytes)
            BitConverter.GetBytes(ECV.Length).CopyTo(buffer, offset);
            offset += sizeof(int);
            Encoding.ASCII.GetBytes(ECV.PadRight(_ecvMax)).CopyTo(buffer, offset);
            offset += _ecvMax;

            // Items (4 bytes actual length + 5 * NesteHeapDataItemSize)
            int actualItemsLength = Items.Count;
            BitConverter.GetBytes(actualItemsLength).CopyTo(buffer, offset);
            offset += sizeof(int);

            for (int i = 0; i < ItemsMaxCount; i++)
            {
                if (i < actualItemsLength)
                {
                    var item = Items[i];
                    item.ToBytes().CopyTo(buffer, offset);
                }

                offset += (new NesteHeapDataItem()).Size;
            }

            return buffer;
        }

        public void FromBytes(byte[] bytes)
        {
            int offset = 0;

            // Id (4 bytes)
            Id = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);

            // Firstname (4 bytes actual length + 15 bytes)
            int firstnameLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            Firstname = Encoding.ASCII.GetString(bytes, offset, _firstnameMax)[..firstnameLength];
            offset += _firstnameMax;

            // Lastname (4 bytes actual length + 20 bytes)
            int lastnameLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            Lastname = Encoding.ASCII.GetString(bytes, offset, _lastnameMax)[..lastnameLength];
            offset += _lastnameMax;

            // ECV (4 bytes actual length + 10 bytes)
            int ecvLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            ECV = Encoding.ASCII.GetString(bytes, offset, _ecvMax)[..ecvLength];
            offset += _ecvMax;

            // Items (4 bytes actual length + 5 * NesteHeapDataItemSize)
            int actualItemsLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);

            Items.Clear();
            var itemSize = (new NesteHeapDataItem()).Size;
            for (int i = 0; i < ItemsMaxCount; i++)
            {
                if (i < actualItemsLength)
                {
                    var item = new NesteHeapDataItem();
                    item.FromBytes(bytes[offset..(offset + itemSize)]);
                    Items.Add(item);
                }

                offset += itemSize;
            }
        }

        public bool Equals(IHeapFileData other)
        {
            var otherData = other as HeapData;
            if (otherData is null)
            {
                return false;
            }

            return Id == otherData.Id;
        }

        public BitArray GetHash()
        {
            return new BitArray(BitConverter.GetBytes(Id));
        }   

        public override string ToString()
        {
            return $"{Id} - {Firstname} {Lastname} [{Items.Count}]";
        }
    }

    public class NesteHeapDataItem : IBinaryData
    {
        public DateOnly Date { get; set; }
        public double Price { get; set; }

        private const int _descriptionMax = 20;
        private string _description = "";

        public string Description
        {
            get => _description;
            set
            {
                if (value.Length > _descriptionMax)
                {
                    throw new ArgumentException($"Description is max {_descriptionMax} characters long");
                }
                _description = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int Size => sizeof(int) + sizeof(double) + sizeof(int) + _descriptionMax;

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[Size];
            int offset = 0;

            // Date (4 bytes)
            Date.ToBytes().CopyTo(buffer, offset);
            offset += Date.Size();

            // Price (8 bytes)
            BitConverter.GetBytes(Price).CopyTo(buffer, offset);
            offset += sizeof(double);

            // Description (4 bytes (length) + 20 bytes)
            BitConverter.GetBytes(Description.Length).CopyTo(buffer, offset);
            offset += sizeof(int);
            Encoding.ASCII.GetBytes(Description.PadRight(_descriptionMax)).CopyTo(buffer, offset);

            return buffer;
        }

        public void FromBytes(byte[] bytes)
        {
            int offset = 0;

            // Date (4 bytes)
            Date = DateOnlyExtension.FromBytes(bytes);
            offset += Date.Size();

            // Price (8 bytes)
            Price = BitConverter.ToDouble(bytes, offset);
            offset += sizeof(double);

            // Description (4 bytes actual length + 20 bytes)
            int descriptionLength = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            Description = Encoding.ASCII.GetString(bytes, offset, _descriptionMax)[..descriptionLength];
        }
    }
}
