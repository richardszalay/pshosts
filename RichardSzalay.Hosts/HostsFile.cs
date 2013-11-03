using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace RichardSzalay.Hosts
{
    public class HostsFile
    {
        private IResource resource;

        private string[] lines;
        private List<HostEntry> entries;

        private List<int> deletedLines;

        private const string AddressExpression = @"[\da-fA-F\.\:]+";
        private const string EntryExpression = @"^(?'Disabled'\#)?\s*" +
            @"(?'Address'" + AddressExpression + @")" +
            @"(?'Spacer'\s+)" +
            @"(?'Hostname'[^\s\#]+)\s*" +
            @"\#?\s*(?'Comment'.+)?$";
            
        private static readonly Regex lineRegex = new Regex(EntryExpression);

        public HostsFile()
            : this(HostsFileLocation)
        {
        }

        public HostsFile(string filename)
            : this(new FileInfoResource(filename))
        {

        }

        internal HostsFile(IResource resource)
        {
            this.resource = resource;

            this.Load();
        }

        public void Load()
        {
            using (Stream stream = this.resource.OpenRead())
            {
                this.Load(stream);
            }
        }

        public void Save()
        {
            using (Stream stream = this.resource.OpenWrite())
            {
                this.Save(stream);

                stream.Flush();
            }
        }

        private void Load(Stream stream)
        {
            deletedLines = new List<int>();

            this.lines = ReadAllLines(stream);

            List<HostEntry> entriesList = new List<HostEntry>();

            for (int i = 0; i < lines.Length; i++)
            {
                HostEntry entry = ParseHostEntry(i, lines[i]);

                if (!(entry == null || HostEntry.IsIgnoredHostname(entry.Name)))
                {
                    entriesList.Add(entry);
                }
            }

            this.entries = entriesList;
        }

        internal void Save(Stream stream)
        {
            this.ApplyChanges();

            StreamWriter writer = new StreamWriter(stream);

            deletedLines.Sort();
            Queue<int> deletedLinesQueue = new Queue<int>(deletedLines);

            for (int i = 0; i < lines.Length; i++)
            {
                if (deletedLinesQueue.Count > 0 && deletedLinesQueue.Peek() == i)
                {
                    deletedLinesQueue.Dequeue();
                }
                else
                {
                    writer.WriteLine(lines[i]);
                }
            }

            writer.Flush();
        }

        private void ApplyChanges()
        {
            List<string> newLines = new List<string>(lines);

            foreach (HostEntry entry in entries)
            {
                if (entry.IsDirty)
                {
                    if (entry.IsNew)
                    {
                        newLines.Add(entry.ToString());
                    }
                    else
                    {
                        newLines[entry.Line] = entry.ToString();
                    }
                }
            }

            lines = newLines.ToArray();
        }

        public void DeleteEntry(HostEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            if (entries.Contains(entry))
            {
                if (!(entry.IsNew || deletedLines.Contains(entry.Line)))
                {
                    deletedLines.Add(entry.Line);
                }

                entries.Remove(entry);
            }
        }

        public void AddEntry(HostEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            if (HostEntry.IsIgnoredHostname(entry.Name))
            {
                throw new ArgumentException("The following hostnames cannot be configured: " + String.Join(", ", HostEntry.IgnoredHostnames));
            }

            if (!entries.Contains(entry))
            {
                entries.Add(entry);
            }
        }

        public IEnumerable<HostEntry> Entries
        {
            get { return entries; }
        }

        public bool IsDirty
        {
            get { return entries.Any(c => c.IsDirty) || deletedLines.Count > 0; }
        }

        private HostEntry ParseHostEntry(int lineIndex, string line)
        {
            if (line.Length == 0)
            {
                return null;
            }

            Match match = lineRegex.Match(line);

            if (match == null || !match.Success)
            {
                return null;
            }

            bool enabled = !match.Groups["Disabled"].Success;
            string address = match.Groups["Address"].Value;
            string spacer = match.Groups["Spacer"].Value;
            string hostname = match.Groups["Hostname"].Value;

            IPAddress tempAddress;
            if (!IPAddress.TryParse(address, out tempAddress))
            {
                return null;
            }

            Group commentGroup = match.Groups["Comment"];

            string comment = (commentGroup != null && commentGroup.Success)
                ? commentGroup.Value
                : null;

            return new HostEntry(lineIndex, line, spacer, enabled,
                hostname, address, comment);
        }



        private string[] ReadAllLines(Stream stream)
        {
            List<string> lines = new List<string>();

            StreamReader reader = new StreamReader(stream);

            string line = reader.ReadLine();

            while (line != null)
            {
                lines.Add(line);

                line = reader.ReadLine();
            }

            return lines.ToArray();
        }

        private const string HostsFileLocation = @"%windir%\system32\drivers\etc\hosts";
    }
}
