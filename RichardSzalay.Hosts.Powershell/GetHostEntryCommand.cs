using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace RichardSzalay.Hosts.Powershell
{
    [Cmdlet(VerbsCommon.Get, Nouns.HostEntry)]
    public class GetHostEntryCommand : HostEntryCommandBase
    {
        [Parameter(Position = 0)]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            ICollection<HostEntry> hostEntries;

            if (TryGetHostEntries(GetHostsFile(), Name, -1, out hostEntries))
            {
                foreach (var entry in hostEntries)
                {
                    WriteObject(entry);
                }
            }

            base.ProcessRecord();
        }
    }
}
