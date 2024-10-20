namespace FRI.AUS2.Libs.Helpers
{
    public static class IntExtension
    {
        public static void Repeat(this int count, Action action)
        {
            for (int i = 0; i < count; i++)
            {
                action();
            }
        }
    }
}
