using System.IO;

namespace RichardSzalay.Hosts
{
    public interface IResource
    {
        Stream OpenRead();
        Stream OpenWrite();
    }
}
