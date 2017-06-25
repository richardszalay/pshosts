using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace RichardSzalay.Hosts.Powershell
{
    [Cmdlet(VerbsCommon.Remove, Nouns.HostEntry, SupportsShouldProcess=true)]
    public class RemoveHostEntryCommand : WriteHostEntryCommandBase
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public int Line { get; set; }

        protected override void ProcessRecord()
        {
            ICollection<HostEntry> hostEntries;

            if (!TryGetHostEntries(HostsFile, Name, Line, true, out hostEntries))
            {
                return;
            }

            foreach (var hostEntry in hostEntries)
            {
                if (ShouldProcess(String.Format("{0} ({1})", hostEntry.Name, hostEntry.Address), "Remove host entry"))
                {
                    HostsFile.DeleteEntry(hostEntry);
                }
            }
        }
    }
}
