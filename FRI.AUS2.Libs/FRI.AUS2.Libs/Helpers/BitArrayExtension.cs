using System.Collections;

namespace FRI.AUS2.Libs.Helpers
{
    public static class BitArrayExtension
    {
        public static BitArray ReverseBits(this BitArray bits)
        {
            int length = bits.Length;
            BitArray reversed = new BitArray(length);
            for (int i = 0; i < length; i++)
            {
                reversed[i] = bits[length - 1 - i];
            }
            return reversed;
        }

        public static bool IsSameAs(this BitArray a, BitArray b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }
    }
}
