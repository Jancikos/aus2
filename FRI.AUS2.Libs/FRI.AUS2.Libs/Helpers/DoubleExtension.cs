namespace FRI.AUS2.Libs.Helpers
{
    public static class DoubleExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="me"></param>
        /// <param name="other"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static int CompareE(this double me, double other, double epsilon = 0.000001)
        {
            double diff = me - other;
            if (Math.Abs(diff) < epsilon)
            {
                return 0;
            }

            return diff > 0 ? 1 : -1;
        }
    }
}
