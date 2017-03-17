using Machine.Specifications;
using RichardSzalay.Hosts.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RichardSzalay.Hosts.Tests
{
    public class HostsFileSpec
    {
        [Fact]
        public void Parsed_basic_hosts_file_has_expected_entries()
        {
            var hostsFileContent = "127.0.0.1    host1 # comment 1\n192.168.0.1    host2";
            var result = new HostsFile(new StringResource(hostsFileContent));

            var expectedEntries = new HostEntry[]
            {
                HostEntry.FromFileEntry(0, "127.0.0.1    host1 # comment 1", "    ", true, "host1", "127.0.0.1", "comment 1"),
                HostEntry.FromFileEntry(1, "192.168.0.1    host2", "    ", true, "host2", "192.168.0.1", null),
            };

            Assert.Equal(expectedEntries, result.Entries);
        }

        [Fact]
        public void Parsed_basic_hosts_file_is_not_in_dirty_state()
        {
            var hostsFileContent = "127.0.0.1    host1 # comment 1\n192.168.0.1    host2";
            var result = new HostsFile(new StringResource(hostsFileContent));

            var expectedEntries = new HostEntry[]
            {
                HostEntry.FromFileEntry(0, "127.0.0.1    host1 # comment 1", "    ", true, "host1", "127.0.0.1", "comment 1"),
                HostEntry.FromFileEntry(1, "192.168.0.1    host2", "    ", true, "host2", "192.168.0.1", null),
            };

            Assert.False(result.IsDirty);
        }

        [Fact]
        public void Host_entries_containing_hyphens_are_supported()
        {
            var hostsFileContent = "127.0.0.1    host1-localhost # comment 1";
            var result = new HostsFile(new StringResource(hostsFileContent));

            var expectedEntries = new HostEntry[]
            {
                    HostEntry.FromFileEntry(0, "127.0.0.1    host1-localhost # comment 1", "    ", true, "host1-localhost", "127.0.0.1", "comment 1"),
            };

            Assert.Equal(expectedEntries, result.Entries);
        }

        [Fact]
        public void Built_in_sample_hosts_are_ignored()
        {
            var hostsFileContent = Resources.DefaultHostsFile;
            var result = new HostsFile(new StringResource(hostsFileContent));

            var expectedEntries = new HostEntry[]
            {
            };

            Assert.Equal(expectedEntries, result.Entries);
        }

        private string ModifyHostsFile(string initialContent, Action<HostsFile> changeAction)
        {
            StringResource stringResource = new StringResource(initialContent);

            var hostsFile = new HostsFile(stringResource);

            changeAction(hostsFile);

            hostsFile.Save();

            return stringResource.ToString();
        }

        [Fact]
        public void When_updating_the_hosts_file_by_modifying_enabled_states()
       {
            var result = ModifyHostsFile(Resources.SampleHostsFile, hostsFile =>
            {
                hostsFile.Entries.First(c => c.Name == "host1.localhost").Enabled = false;
                hostsFile.Entries.First(c => c.Name == "host2.localhost").Enabled = true;
            });

            Assert.Equal(Resources.SampleHostsFile_Disable, result);
        }

        [Fact]
        public void When_updating_the_hosts_file_by_swapping_the_lines_of_two_entries()
        {
            var result = ModifyHostsFile(Resources.SampleHostsFile, hostsFile =>
            {
                hostsFile.Entries.First(c => c.Name == "host1.localhost")
                        .SwapLine(hostsFile.Entries.First(c => c.Name == "host2.localhost"));
            });

            Assert.Equal(Resources.SampleHostsFile_Reorder, result);
        }

        [Fact]
        public void When_updating_the_hosts_file_by_deleting_an_entry()
        {
            var result = ModifyHostsFile(Resources.SampleHostsFile, hostsFile =>
            {
                hostsFile.DeleteEntry(
                        hostsFile.Entries.First(c => c.Name == "host1.localhost"));
            });

            Assert.Equal(Resources.SampleHostsFile_Delete, result);
        }

        [Fact]
        public void When_making_numerous_seemingly_conflicting_changes()
        {
            var result = ModifyHostsFile(Resources.ComplexHostsFile_Before, hostsFile =>
            {
                var entry1 = hostsFile.Entries.First(c => c.Name == "host1.localhost");
                var entry2 = hostsFile.Entries.First(c => c.Name == "host2.localhost");
                var entry3 = hostsFile.Entries.First(c => c.Name == "host3.localhost");
                var entry4 = hostsFile.Entries.First(c => c.Name == "host4.localhost");
                var entry5 = hostsFile.Entries.First(c => c.Name == "host5.localhost");
                var entry6 = new HostEntry("host6.localhost", "127.0.0.1", "comment 6");

                entry1.Enabled = false;
                entry2.Enabled = true;
                entry3.Enabled = false;

                entry3.SwapLine(entry5); // swap two with a deleted in between
                entry6.SwapLine(entry2); // new swapped with existing

                hostsFile.DeleteEntry(entry4);
                hostsFile.AddEntry(entry6);
            });

            Assert.Equal(Resources.ComplexHostsFile_Expected, result);
        }

        [Fact]
        public void Adding_an_entry_includes_it_in_the_list()
        {
            var sut = new HostsFile(new StringResource());

            var newEntry = new HostEntry("host.localhost", "1.0.0.0", null);

            sut.AddEntry(newEntry);

            Assert.Contains(newEntry, sut.Entries);
        }

        [Fact]
        public void Adding_an_entry_marks_the_file_as_dirty()
        {
            var sut = new HostsFile(new StringResource());

            var newEntry = new HostEntry("host.localhost", "1.0.0.0", null);

            sut.AddEntry(newEntry);

            Assert.True(sut.IsDirty);
        }

        [Fact]
        public void Adding_a_null_entry_throws_NullArgumentException()
        {
            var sut = new HostsFile(new StringResource());

            Assert.Throws<ArgumentNullException>(() => sut.AddEntry(null));
        }

        [Fact]
        public void Adding_an_ignored_hostname_throws_ArgumentException()
        {
            var sut = new HostsFile(new StringResource());

            var newEntry = new HostEntry("localhost", "1.0.0.0", null);

            Assert.Throws<ArgumentException>(() => sut.AddEntry(newEntry));
        }

        [Fact]
        public void Deleting_an_entry_removes_it_from_the_list()
        {
            var sut = new HostsFile(new StringResource("127.0.0.1    host1 # comment 1\n192.168.0.1    host2"));
            var entryToDelete = sut.Entries.First();

            sut.DeleteEntry(sut.Entries.First());

            Assert.DoesNotContain(entryToDelete,  sut.Entries);
        }

        [Fact]
        public void Deleting_an_entry_marks_the_file_as_dirty()
        {
            var sut = new HostsFile(new StringResource("127.0.0.1    host1 # comment 1\n192.168.0.1    host2"));
            var entryToDelete = sut.Entries.First();

            sut.DeleteEntry(sut.Entries.First());

            Assert.True(sut.IsDirty);
        }

        [Fact]
        public void Deleting_an_entry_that_does_not_exist_does_not_throw_exception()
        {
            var sut = new HostsFile(new StringResource());

            sut.DeleteEntry(new HostEntry("host.localhost", "1.0.0.0", null));
        }

        [Fact]
        public void Deleting_a_null_entry_throws_NullArgumentException()
        {
            var sut = new HostsFile(new StringResource());

            Assert.Throws<ArgumentNullException>(() => sut.DeleteEntry(null));
        }

        [Fact]
        public void Modifying_an_entry_marks_the_file_as_dirty()
        {
            var sut = new HostsFile(new StringResource("127.0.0.1    host1 # comment 1\n192.168.0.1    host2"));

            sut.Entries.First().Enabled = false;

            Assert.True(sut.IsDirty);
        }
    }
}
