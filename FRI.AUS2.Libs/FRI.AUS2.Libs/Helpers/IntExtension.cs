using System.Collections;

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

        // TODO - prerobit na bitove operacie
        public static int ResetNthBit(this int value, int n)
        {
            BitArray bits = new BitArray(new int[] { value });

            bits.Set(n, !bits.Get(n));

            int[] array = new int[1];
            bits.CopyTo(array, 0);
            return array[0];
        }

        public static int CutFirtsNBits(this int value, int n)
        {
            var bits = new BitArray(new int[] { value });

            var mask = new BitArray(sizeof(int) * 8);
            for (int i = 0; i < mask.Length; i++)
            {
                mask.Set(i, i >= n);
            }

            var result = bits.And(mask);

            int[] array = new int[1];
            result.CopyTo(array, 0);
            return array[0];
        }
    }
}
