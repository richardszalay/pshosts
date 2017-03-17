using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace RichardSzalay.Hosts.Powershell
{
    public abstract class HostEntryCommandBase : PSCmdlet
    {
        [Parameter]
        public string HostsPath { get; set; }

        protected HostsFile GetHostsFile()
        {
            return String.IsNullOrEmpty(HostsPath)
                ? new HostsFile()
                : new HostsFile(HostsPath);
        }

        protected bool TryGetHostEntries(HostsFile hostsFile, string name, int line, out ICollection<HostEntry> hostEntries)
        {
            hostEntries = hostsFile.Entries.ToList();

            if (line != -1 && MyInvocation.BoundParameters.Keys.Contains("Line"))
            {
                hostEntries = hostEntries.Where(e => e.Line == line).ToList();
                return hostEntries.Count == 1;
            }

            if (String.IsNullOrEmpty(name))
            {
                hostEntries = hostsFile.Entries.ToList();
                return true;
            }

            if (WildcardPattern.ContainsWildcardCharacters(name))
            {
                var pattern = new WildcardPattern(name, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
                hostEntries = hostsFile.Entries.Where(e => pattern.IsMatch(e.Name)).ToList();
                return true;
            }
            else
            {
                hostEntries = hostsFile.Entries.Where(e => String.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase)).ToList();

                if (hostEntries.Count == 0)
                {
                    WriteError(new ErrorRecord(new ItemNotFoundException(String.Format("Host entry '{0}' not found", name)),
                        "ItemNotFound", ErrorCategory.ObjectNotFound, name));
                    return false;
                }

                return true;
            }
        }
    }
}
