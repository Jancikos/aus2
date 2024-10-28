namespace FRI.AUS2.Libs.Structures.Trees.Interfaces
{
    public interface IKdTreeData
    {
        /// <summary>
        /// compare the position of this object within the tree 
        ///
        /// -1 - this is smaller
        ///  0 - this is equal
        /// +1 - this is bigger 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public int Compare(int level, IKdTreeData other);
        public int GetDiminesionsCount();

        /// <summary>
        /// checks if this object is equal to the other object (dont have to be by position, but also by some other properties)
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(IKdTreeData other);
    }
}
