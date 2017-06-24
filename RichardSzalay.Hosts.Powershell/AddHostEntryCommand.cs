using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Text;

namespace RichardSzalay.Hosts.Powershell
{
    [Cmdlet(VerbsCommon.Add, Nouns.HostEntry, SupportsShouldProcess=true)]
    public class AddHostEntryCommand : WriteHostEntryCommandBase
    {
        public AddHostEntryCommand()
        {
            Enabled = true;
        }

        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "SpecificAddress")]
        public string Address { get; set; }

        [Parameter(ParameterSetName = "IPv4LoopbackAddress")]
        public SwitchParameter Loopback { get; set; }

        [Parameter(ParameterSetName = "IPv6LoopbackAddress")]
        public SwitchParameter IPv6Loopback { get; set; }

        [Parameter(Position = 2)]
        public string Comment { get; set; }

        [Parameter]
        public bool Enabled { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        bool yesToAll, noToAll;

        protected override void BeginProcessing()
        {
            yesToAll = false;
            noToAll = false;

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            string address = ValidateAddress();

            var newEntry = new HostEntry(Name, address, Comment)
            {
                Enabled = Enabled
            };

            if (ShouldProcess(newEntry.ToShortString(), "Add host entry"))
            {
                if (!Force && HostEntryExists(HostsFile.Entries, newEntry.Name))
                {
                    if (!ShouldContinue(String.Format("The host entry '{0}' already exists and the Force paremeter was not specified. If you continue, a duplicate host entry will be created", Name), 
                        "Confirm", ref yesToAll, ref noToAll))
                    {
                        return;
                    }
                }

                HostsFile.AddEntry(newEntry);

                WriteObject(newEntry);
            }
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

        private bool HostEntryExists(IEnumerable<HostEntry> entries, string name)
        {
            return entries.Any(e => String.Equals(e.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }

    }
}
