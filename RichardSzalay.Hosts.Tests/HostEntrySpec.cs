using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RichardSzalay.Hosts.Tests
{
    public class HostEntrySpec
    {
        [Fact]
        public void Constructing_a_new_host_entry_assigns_appropriate_properties()
        {
            var result = new HostEntry("hostname", "address", "comment");

            Assert.Equal("hostname", result.Name);
            Assert.Equal("address", result.Address);
            Assert.Equal("comment", result.Comment);
            Assert.True(result.Enabled);
            Assert.Equal(-1, result.Line);
            Assert.True(result.IsDirty);
            Assert.True(result.IsNew);
        }

        [Fact]
        public void Loading_a_host_entry_assigns_appropriate_properties()
        {
            var result = HostEntry.FromFileEntry(5, "original-line", " ", false, "hostname", "address", "comment");

            Assert.Equal("hostname", result.Name);
            Assert.Equal("address", result.Address);
            Assert.Equal("comment", result.Comment);
            Assert.False(result.Enabled);
            Assert.Equal(5, result.Line);
            Assert.False(result.IsDirty);
            Assert.False(result.IsNew);
        }

        [Fact]
        public void Entries_constructed_as_line_zero_are_not_new()
        {
            var result = HostEntry.FromFileEntry(0, "original-line", " ", false, "hostname", "address", "comment");

            Assert.False(result.IsNew);
        }

        [Fact]
        public void Modifying_an_instance_without_changing_values_does_not_mark_it_as_dirty()
        {
            var result = HostEntry.FromFileEntry(0, "original-line", " ", false, "hostname", "address", "comment");

            result.Enabled = false;
            result.Name = "hostname";
            result.Address = "address";
            result.Comment = "comment";

            Assert.False(result.IsDirty);
        }

        [Fact]
        public void Modifying_hostname_marks_instance_as_dirty()
        {
            var result = HostEntry.FromFileEntry(0, "original-line", " ", false, "hostname", "address", "comment");

            result.Name = "hostname2";

            Assert.True(result.IsDirty);
        }

        [Fact]
        public void Modifying_address_marks_instance_as_dirty()
        {
            var result = HostEntry.FromFileEntry(0, "original-line", " ", false, "hostname", "address", "comment");

            result.Address = "address2";

            Assert.True(result.IsDirty);
        }

        [Fact]
        public void Modifying_comment_marks_instance_as_dirty()
        {
            var result = HostEntry.FromFileEntry(0, "original-line", " ", false, "hostname", "address", "comment");

            result.Comment = "comment2";

            Assert.True(result.IsDirty);
        }

        [Fact]
        public void Modifying_enabled_marks_instance_as_dirty()
        {
            var result = HostEntry.FromFileEntry(0, "original-line", " ", false, "hostname", "address", "comment");

            result.Enabled = true;

            Assert.True(result.IsDirty);
        }

        [Fact]
        public void Swapping_positions_changes_both_line_numbers()
        {
            var entryA = HostEntry.FromFileEntry(5, "original-line", " ", false, "hostname", "address", "comment");
            var entryB = HostEntry.FromFileEntry(10, "original-line", " ", false, "hostname", "address", "comment");

            entryA.SwapLine(entryB);

            Assert.True(entryA.IsDirty);
            Assert.True(entryB.IsDirty);

            Assert.Equal(10, entryA.Line);
            Assert.Equal(5, entryB.Line);
        }

        [Fact]
        public void Swapping_with_a_new_entry_swaps_new_status()
        {
            var entryA = HostEntry.FromFileEntry(-1, "original-line", " ", false, "hostname", "address", "comment");
            var entryB = HostEntry.FromFileEntry(10, "original-line", " ", false, "hostname", "address", "comment");

            entryA.SwapLine(entryB);

            Assert.False(entryA.IsNew);
            Assert.True(entryB.IsNew);

            Assert.Equal(10, entryA.Line);
        }

        [Fact]
        public void Disabled_host_entries_self_format_with_hash()
        {
            var sut = HostEntry.FromFileEntry(0, null, " ", false, "hostname", "address", "comment");

            var result = sut.ToString();

            Assert.StartsWith("#", result);
        }

        [Fact]
        public void Enabled_host_entries_do_not_self_format_with_hash()
        {
            var sut = HostEntry.FromFileEntry(0, null, " ", true, "hostname", "address", "comment");

            var result = sut.ToString();

            Assert.StartsWith("address", result);
        }

        [Fact]
        public void Existing_spacing_policy_is_honoured()
        {
            var sut = HostEntry.FromFileEntry(0, null, "\t", false, "hostname", "address", "comment");

            var result = sut.ToString();

            Assert.Equal("# address\thostname # comment", result);
        }

        [Fact]
        public void Comments_are_added_to_the_end_of_the_formatted_line()
        {
            var sut = HostEntry.FromFileEntry(0, null, "\t", true, "hostname", "address", "comment");

            var result = sut.ToString();

            Assert.EndsWith(" # comment", result);
        }

        [Fact]
        public void Commentless_entries_do_not_include_comment_character()
        {
            var sut = HostEntry.FromFileEntry(0, null, "\t", true, "hostname", "address", null);

            var result = sut.ToString();

            Assert.EndsWith("hostname", result);
        }


        [Fact]
        public void Dirty_entries_regenerate_line()
        {
            var sut = HostEntry.FromFileEntry(0, "original-line", " ", false, "hostname", "address", "comment");
            sut.Name = "hostname2";

            var result = sut.ToString();

            Assert.Equal("# address hostname2 # comment", result);
        }

        [Fact]
        public void Entries_with_no_original_line_regenerate_line()
        {
            var sut = HostEntry.FromFileEntry(0, null, " ", false, "hostname", "address", "comment");

            var result = sut.ToString();

            Assert.Equal("# address hostname # comment", result);
        }

        [Fact]
        public void Unchanged_entries_format_using_original_line()
        {
            var sut = HostEntry.FromFileEntry(0, "original-line", " ", true, "hostname", "address", "comment");

            var result = sut.ToString();

            Assert.Equal("original-line", result);
        }

        [Fact]
        public void When_comparing_two_identical_host_entries()
        {
            var entryA = HostEntry.FromFileEntry(5, null, "    ", false, "hostname", "address", "comment");
            var entryB = HostEntry.FromFileEntry(5, null, "    ", false, "hostname", "address", "comment");

            var result = entryA.Equals(entryB);

            Assert.True(result);
        }

        [Fact]
        public void When_comparing_two_host_entries_with_different_hostnames()
        {
            var entryA = HostEntry.FromFileEntry(5, null, "    ", false, "hostname1", "address", "comment");
            var entryB = HostEntry.FromFileEntry(5, null, "    ", false, "hostname2", "address", "comment");

            var result = entryA.Equals(entryB);

            Assert.False(result);
        }

        [Fact]
        public void When_comparing_two_host_entries_with_different_addresses()
        {
            var entryA = HostEntry.FromFileEntry(5, null, "    ", false, "hostname", "address", "comment");
            var entryB = HostEntry.FromFileEntry(5, null, "    ", false, "hostname", "address2", "comment");

            var result = entryA.Equals(entryB);

            Assert.False(result);
        }

        [Fact]
        public void When_comparing_two_host_entries_with_different_comments()
        {
            var entryA = HostEntry.FromFileEntry(5, null, "    ", false, "hostname", "address", "comment");
            var entryB = HostEntry.FromFileEntry(5, null, "    ", false, "hostname", "address", "comment2");

            var result = entryA.Equals(entryB);

            Assert.False(result);
        }

        [Fact]
        public void Known_sample_hostnames_are_ignored()
        {
            var result = HostEntry.IsIgnoredHostname("rhino.acme.com");

            Assert.True(result);
        }

        [Fact]
        public void Other_hostnames_are_not_ignored()
        {
            var result = HostEntry.IsIgnoredHostname("www.google.com");

            Assert.False(result);
        }
    }
}
