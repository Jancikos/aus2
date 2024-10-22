namespace FRI.AUS2.Libs.Structures.Trees.Interfaces
{
    public interface IKdTreeData
    {
        /// <summary>
        /// -1 - this is smaller
        ///  0 - this is equal
        /// +1 - this is bigger 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public int Compare(int level, IKdTreeData other);
        public int GetDiminesionsCount();

        // POZOR: typ double obmedzuje, ze kluce dimenzii nemozu byt iny typ ako double
        public double GetDiminesionValue(int dim);
    }
}
