using System.IO;
using System.Text;

namespace RichardSzalay.Hosts.Tests.Infrastructure
{
    public class StringResource : IResource
    {
        private MemoryStream stream;

        public StringResource(string initialValue)
            : this()
        {
            byte[] initialValueBytes = Encoding.UTF8.GetBytes(initialValue);

            stream.Write(initialValueBytes, 0, initialValueBytes.Length);
        }

        public StringResource()
        {
            stream = new MemoryStream();
        }

        #region IResource Members

        public Stream OpenRead()
        {
            stream.Seek(0L, SeekOrigin.Begin);

            return new NoDisposeStream(stream);
        }

        public Stream OpenWrite()
        {
            stream.Seek(0L, SeekOrigin.Begin);

            return new NoDisposeStream(stream);
        }

        #endregion

        public override string ToString()
        {
            byte[] buffer = stream.ToArray();

            return Encoding.UTF8.GetString(buffer);
        }

        class NoDisposeStream : Stream, System.IDisposable
        {
            private readonly Stream inner;

            public NoDisposeStream(Stream inner)
            {
                this.inner = inner;
            }

            public override bool CanRead => inner.CanRead;

            public override bool CanSeek => inner.CanSeek;

            public override bool CanWrite => inner.CanWrite;

            public override long Length => inner.Length;

            public override long Position
            {
                get => inner.Position;
                set => inner.Position = value;
            }

            public override void Flush()
            {
                inner.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return inner.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return inner.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                inner.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                inner.Write(buffer, offset, count);
            }

            protected override void Dispose(bool disposing)
            {
            }

            void System.IDisposable.Dispose()
            {

            }
        }
    }
}
