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

            return stream;
        }

        public Stream OpenWrite()
        {
            stream = new MemoryStream();

            return stream;
        }

        #endregion

        public override string ToString()
        {
            byte[] buffer = stream.ToArray();

            return Encoding.UTF8.GetString(buffer);
        }
    }
}
