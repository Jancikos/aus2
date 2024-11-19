namespace FRI.AUS2.Libs.Helpers
{
    public static class DateOnlyExtension
    {

        /// <summary>
        /// bytes size = 2 (ushort) + 1 (byte) + 1 (byte)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int Size(this DateOnly date)
        {
            return sizeof(ushort) + 1 + 1;
        }

        public static byte[] ToBytes(this DateOnly date)
        {
            byte[] buffer = new byte[sizeof(ushort) + 1 + 1];
            int offset = 0;

            // Year (2 bytes)
            BitConverter.GetBytes((ushort)date.Year).CopyTo(buffer, offset);
            offset += sizeof(ushort);

            // Month (1 byte) 
            buffer[offset++] = (byte)date.Month;

            // Day (1 byte)
            buffer[offset] = (byte)date.Day;

            return buffer;
        }

        public static DateOnly FromBytes(byte[] bytes)
        {
            return new DateOnly(
                BitConverter.ToUInt16(bytes, 0),
                bytes[2],
                bytes[3]
            );
        }
    }
}
