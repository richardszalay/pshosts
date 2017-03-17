using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace RichardSzalay.Hosts.Powershell
{
    [Cmdlet(VerbsLifecycle.Disable, Nouns.HostEntry)]
    public class DisableHostEntryCommand : WriteHostEntryCommandBase
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public int Line { get; set; }

        protected override void ProcessRecord()
        {
            if (!base.TryGetHostEntries(HostsFile, Name, Line, out ICollection<HostEntry> hostEntries))
            {
                return;
            }

            foreach (var hostEntry in hostEntries)
            {
                if (ShouldProcess(hostEntry.ToShortString(), "Disable host entry"))
                {
                    hostEntry.Enabled = false;
                }
            }

            base.ProcessRecord();
        }
    }
}
