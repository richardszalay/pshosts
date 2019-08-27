using System;
using System.IO;
using System.Text;

namespace RichardSzalay.Hosts.Tests.Infrastructure
{
    public class EmbeddedResource : IResource
    {
        private MemoryStream stream;

        public static string GetText(string name)
        {
            var assembly = typeof(StringResource).Assembly;

            var resourceName = $"{assembly.GetName().Name}.Resources.{name}.txt";

            using (var reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
            {
                return NormalizeLineEndings(reader.ReadToEnd());
            }
        }

        static string NormalizeLineEndings(string input)
        {
            return input.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }

        public EmbeddedResource(string name)
            : this()
        {
            byte[] initialValueBytes = Encoding.UTF8.GetBytes(GetText(name));

            stream.Write(initialValueBytes, 0, initialValueBytes.Length);

        }

        public EmbeddedResource()
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
