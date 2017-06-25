using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Text;

namespace RichardSzalay.Hosts.Powershell
{
    [Cmdlet(VerbsCommon.Set, Nouns.HostEntry, SupportsShouldProcess = true)]
    public class SetHostEntryCommand : WriteHostEntryCommandBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public int Line { get; set; }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "SpecificAddress")]
        public string Address { get; set; }

        [Parameter(ParameterSetName = "IPv4LoopbackAddress")]
        public SwitchParameter Loopback { get; set; }

        [Parameter(ParameterSetName = "IPv6LoopbackAddress")]
        public SwitchParameter IPv6Loopback { get; set; }

        [Parameter]
        public string Comment { get; set; }

        [Parameter]
        public bool Enabled { get; set; }

        protected override void ProcessRecord()
        {
            ICollection<HostEntry> hostEntries;

            if (!TryGetHostEntries(HostsFile, Name, Line, true, out hostEntries))
            {
                return;
            }

            foreach (var hostEntry in hostEntries)
            {
                if (ShouldProcess(hostEntry.ToShortString(), GetActionString()))
                {
                    if (IsSettingAddress())
                    {
                        hostEntry.Address = ValidateAddress();
                    }

                    if (this.MyInvocation.BoundParameters.ContainsKey("Comment"))
                    {
                        hostEntry.Comment = Comment;
                    }

                    if (this.MyInvocation.BoundParameters.ContainsKey("Enabled"))
                    {
                        hostEntry.Enabled = Enabled;
                    }

                    if (Line != -1 && !String.IsNullOrEmpty(Name))
                    {
                        hostEntry.Name = Name;
                    }
                }
            }
        }

        bool IsSettingAddress()
        {
            var boundParameters = this.MyInvocation.BoundParameters;

            return boundParameters.ContainsKey("Address") ||
                boundParameters.ContainsKey("Loopback") ||
                boundParameters.ContainsKey("IPv6Loopback");
        }

        string ValidateAddress()
        {
            if (this.MyInvocation.BoundParameters.ContainsKey("Loopback") && Loopback.IsPresent)
            {
                return IPAddress.Loopback.ToString();
            }

            if (this.MyInvocation.BoundParameters.ContainsKey("IPv6Loopback") && IPv6Loopback.IsPresent)
            {
                return IPAddress.IPv6Loopback.ToString();
            }

            IPAddress ipAddress;

            if (!IPAddress.TryParse(Address, out ipAddress))
            {
                WriteWarning(String.Format("'{0}' is not a valid IP address", Address));
            }

            return Address;
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
