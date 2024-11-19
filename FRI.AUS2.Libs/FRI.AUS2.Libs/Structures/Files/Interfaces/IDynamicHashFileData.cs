﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRI.AUS2.Libs.Structures.Trees.Interfaces;

namespace FRI.AUS2.Libs.Structures.Files
{
    public interface IDynamicHashFileData : IHeapFileData
    {
        public int GetHash();
    }

}