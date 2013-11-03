using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace RichardSzalay.Hosts.Powershell
{
    [Cmdlet(VerbsCommon.Set, Nouns.HostEntry, SupportsShouldProcess = true)]
    public class SetHostEntryCommand : WriteHostEntryCommandBase
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public int Line { get; set; }

        [Parameter(Position = 1)]
        public string Address { get; set; }

        [Parameter]
        public string Comment { get; set; }

        [Parameter]
        public bool Enabled { get; set; }

        protected override void ProcessRecord()
        {
            var hostsFile = GetHostsFile();

            ICollection<HostEntry> hostEntries;

            if (!TryGetHostEntries(HostsFile, Name, Line, out hostEntries))
            {
                return;
            }

            foreach (var hostEntry in hostEntries)
            {
                if (ShouldProcess(hostEntry.ToShortString(), GetActionString()))
                {
                    if (this.MyInvocation.BoundParameters.ContainsKey("Address"))
                    {
                        hostEntry.Address = Address;
                    }

                    if (this.MyInvocation.BoundParameters.ContainsKey("Comment"))
                    {
                        hostEntry.Comment = Comment;
                    }

                    if (this.MyInvocation.BoundParameters.ContainsKey("Enabled"))
                    {
                        hostEntry.Enabled = Enabled;
                    }
                }
            }
        }

        private string GetActionString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Update host entry (");

            var modifiedProperties = MyInvocation.BoundParameters.Keys.Join(
                new [] { "Address", "Comment", "Enabled" },
                x => x, x => x, (x,y) => x);

            sb.Append(String.Join(", ", modifiedProperties));

            sb.Append(")");

            return sb.ToString();
        }
    }
}
