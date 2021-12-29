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

        [Parameter]
        public double LockWaitSeconds { get; set; } = HostsFile.DefaultFileLockWait;

        private string GetHostsPath()
        {
            return String.IsNullOrEmpty(HostsPath)
                ? GetVariableValue("PSHostsFilePath") as string
                : HostsPath;
        }

        protected HostsFile GetHostsFile()
        {
            var suppliedHostsPath = GetHostsPath();
            
            return String.IsNullOrEmpty(suppliedHostsPath)
                ? new HostsFile(LockWaitSeconds)
                : new HostsFile(suppliedHostsPath, LockWaitSeconds);
        }

        protected bool TryGetHostEntries(HostsFile hostsFile, string name, int line, bool requireMatch, out ICollection<HostEntry> hostEntries)
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
                return hostEntries.Count > 0;
            }
            else
            {
                hostEntries = hostsFile.Entries.Where(e => String.Equals(e.Name, name, StringComparison.InvariantCultureIgnoreCase)).ToList();

                if (hostEntries.Count == 0)
                {
                    if (requireMatch)
                    {
                        WriteError(new ErrorRecord(new ItemNotFoundException(String.Format(MissingHostEntryMessage, name)),
                            "ItemNotFound", ErrorCategory.ObjectNotFound, name));
                    }
                    return false;
                }

                return true;
            }
        }

        protected virtual string MissingHostEntryMessage
        {
            get { return "Host entry '{0}' not found"; }
        }
    }
}
