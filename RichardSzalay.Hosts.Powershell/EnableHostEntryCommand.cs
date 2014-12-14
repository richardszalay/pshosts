using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace RichardSzalay.Hosts.Powershell
{
    [Cmdlet(VerbsLifecycle.Enable, Nouns.HostEntry)]
    public class EnableHostEntryCommand : WriteHostEntryCommandBase
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public int Line { get; set; }

        protected override void ProcessRecord()
        {
            ICollection<HostEntry> hostEntries;

            if (!base.TryGetHostEntries(HostsFile, Name, Line, out hostEntries))
            {
                return;
            }

            foreach (var hostEntry in hostEntries)
            {
                if (ShouldProcess(hostEntry.ToShortString(), "Enabled host entry"))
                {
                    hostEntry.Enabled = true;
                }
            }

            base.ProcessRecord();
        }
    }
}
