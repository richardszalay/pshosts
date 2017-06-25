using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace RichardSzalay.Hosts.Powershell
{
    [Cmdlet("Test", Nouns.HostEntry)]
    public class TestHostEntryCommand : HostEntryCommandBase
    {
        [Parameter(Position = 0)]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            ICollection<HostEntry> hostEntries;
            bool hasEntries = TryGetHostEntries(GetHostsFile(), Name, -1, false, out hostEntries);

            WriteObject(hasEntries);

            base.ProcessRecord();
        }
    }
}
