using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.Hosts.Powershell
{
    public class WriteHostEntryCommandBase : HostEntryCommandBase
    {
        protected HostsFile HostsFile { get; private set; }

        protected override void BeginProcessing()
        {
            HostsFile = GetHostsFile();
        }

        protected override void EndProcessing()
        {
            if (HostsFile.IsDirty)
            {
                HostsFile.Save();
            }
        }
    }
}
