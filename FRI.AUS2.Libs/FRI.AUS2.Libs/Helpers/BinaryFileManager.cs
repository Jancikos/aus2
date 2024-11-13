namespace FRI.AUS2.Libs.Helpers
{
    public class BinaryFileManager : IDisposable
    {
        private FileStream _fileStream;

        public BinaryFileManager(FileInfo file)
        {
            _fileStream = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        public int Length => (int)_fileStream.Length;

        public void Dispose()
        {
            _fileStream.Dispose();
        }

        public byte[] ReadAllBytes()
        {
            return ReadBytes(0, Length);
        }

        public byte[] ReadBytes(int offset, int count)
        {
            byte[] buffer = new byte[count];

            _fileStream.Seek(offset, SeekOrigin.Begin);
            _fileStream.Read(buffer, 0, count);

            return buffer;
        }

        public void WriteBytes(int offset, byte[] buffer)
        {
            _fileStream.Seek(offset, SeekOrigin.Begin);
            _fileStream.Write(buffer, 0, buffer.Length);
            _fileStream.Flush();
        }

        public void Truncate(int length)
        {
            _fileStream.SetLength(length);
        }
    }
}
