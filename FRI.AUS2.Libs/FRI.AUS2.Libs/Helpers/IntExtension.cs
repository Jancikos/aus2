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

        public static string ToBinaryString(this int value, int padding = 8, bool spaceSeparated = true)
        {
            var binaryString = Convert.ToString(value, 2).PadLeft(padding, '0');

            if (spaceSeparated && binaryString.Length > 4)
            {
                return string.Join(
                    " ",
                    Enumerable
                        .Range(0, binaryString.Length / 4)
                        .Select(i => binaryString.Substring(i * 4, 4))
                    );
            }

            return binaryString;
        }

        public static bool GetNthBit(this int value, int n)
        {
            return (value & (1 << n)) != 0;
        }
    }
}
