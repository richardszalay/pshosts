using System;
using System.IO;

namespace RichardSzalay.Hosts
{
    internal class FileInfoResource : IResource
    {
        private FileInfo file;

        public FileInfoResource(string filename)
            : this(new FileInfo(Environment.ExpandEnvironmentVariables(filename)))
        {
        }

        public FileInfoResource(FileInfo file)
        {
            this.file = file;
        }

        #region IResource Members

        public Stream OpenRead()
        {
            return this.file.OpenRead();
        }

        public Stream OpenWrite()
        {
            return new FileStream(this.file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }

        #endregion
    }
}
