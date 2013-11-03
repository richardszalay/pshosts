using System.IO;

namespace RichardSzalay.Hosts
{
    internal interface IResource
    {
        Stream OpenRead();
        Stream OpenWrite();
    }
}
