﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace RichardSzalay.Hosts
{
    public class HostEntry : IEquatable<HostEntry>, ICloneable
    {
        private const string CommentPrefix = "# ";

        private const string DefaultSpacer = "\t";

        private bool isDirty = false;

        private int line;
        private string originalLine;
        private string spacer;

        private bool enabled;
        private string hostname;
        private string address;
        private IPAddress ipAddress;
        private string comment;

        public HostEntry(string hostname, string address, string comment)
            : this(-1, null, DefaultSpacer, true, hostname, address, comment)
        {
        }

        internal HostEntry(int line, string originalLine, string spacer, bool enabled, string hostname, string address, string comment)
        {
            this.line = line;
            this.originalLine = originalLine;
            this.spacer = spacer;

            this.enabled = enabled;
            this.hostname = hostname;
            this.address = address;
            this.ipAddress = SafeParseIPAddress(address);
            this.comment = comment;
        }

        public int Line
        {
            get { return line; }
            private set
            {
                if (line != value)
                {
                    line = value;
                    isDirty = true;
                }
            }
        }

        public string Name
        {
            get { return hostname; }
            set
            {
                if (hostname != value)
                {
                    hostname = value;
                    isDirty = true;
                }
            }
        }

        public string Address
        {
            get { return address; }
            set
            {
                if (address != value)
                {
                    address = value;
                    isDirty = true;

                    ipAddress = SafeParseIPAddress(address);
                }
            }
        }

        public IPAddress IPAddress
        {
            get { return ipAddress; }
        }

        public bool IsLoopback
        {
            get { return ipAddress != null && IPAddress.IsLoopback(ipAddress); }
        }

        public string Comment
        {
            get { return comment; }
            set
            {
                if (comment != value)
                {
                    comment = value;
                    isDirty = true;
                }
            }
        }

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    isDirty = true;
                }
            }
        }

        public void SwapLine(HostEntry other)
        {
            int otherLine = other.Line;

            other.Line = this.Line;
            this.Line = otherLine;
        }

        public bool IsDirty
        {
            get { return isDirty || IsNew || String.IsNullOrEmpty(originalLine); }
        }
        
        public bool IsLoopback
        {
            get
            {
                bool result;
                try
                {
                    IPAddress ipAddress = IPAddress.Parse(address);
                    result = IPAddress.IsLoopback(ipAddress);
                }
                catch(Exception)
                {
                    result = false;
                }
                return result;
            }
        }

        public bool IsNew
        {
            get { return Line == -1; }
        }

        public string ToShortString()
        {
            return String.Format("{0} ({1})", Name, Address);
        }

        public override string ToString()
        {
            if (this.IsDirty)
            {
                StringBuilder sb = new StringBuilder(); // TODO: estimate size?

                if (!enabled)
                {
                    sb.Append(CommentPrefix);
                }

                sb.Append(address);
                sb.Append(spacer);
                sb.Append(hostname);

                if (!String.IsNullOrEmpty(comment))
                {
                    sb.Append(" ");
                    sb.Append(CommentPrefix);
                    sb.Append(comment);
                }

                return sb.ToString();
            }
            else
            {
                return originalLine;
            }
        }

        #region IEquatable<HostEntry> Members

        public bool Equals(HostEntry other)
        {
            return other.Line == this.Line &&
                other.originalLine == this.originalLine &&
                other.enabled == this.enabled &&
                other.isDirty == this.isDirty &&
                other.hostname == this.hostname &&
                other.address == this.address &&
                other.comment == this.comment;
        }

        public override bool Equals(object obj)
        {
            HostEntry other = obj as HostEntry;

            if (other != null)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Line ^ hostname.GetHashCode();
        }

        #endregion

        public static bool IsIgnoredHostname(string hostname)
        {
            return ((IList<string>)IgnoredHostnames)
                .Contains(hostname.ToLowerInvariant());
        }

        public static readonly string[] IgnoredHostnames = new string[]
        {
            "rhino.acme.com", "x.acme.com", "localhost"
        };

        private static IPAddress SafeParseIPAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }

            IPAddress ipAddress;
            if (!IPAddress.TryParse(address, out ipAddress))
            {
                return null;
            }

            return ipAddress;
        }

        public HostEntry Clone()
        {
            return new HostEntry(
                line, originalLine, spacer,
                enabled, hostname, address, comment
                );
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
