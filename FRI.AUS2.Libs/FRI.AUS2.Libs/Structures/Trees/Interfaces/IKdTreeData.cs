﻿namespace FRI.AUS2.Libs.Structures.Trees.Interfaces
{
    public interface IKdTreeData
    {
        public int Compare(int level, IKdTreeData other);
        public int GetDiminesionsCount();
        public double GetDiminesionValue(int dim);
    }
}
