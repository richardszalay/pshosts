using FluentAssertions;
using Machine.Specifications;
using RichardSzalay.Hosts.Tests.Infrastructure;
using RichardSzalay.Hosts.Tests.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RichardSzalay.Hosts.Tests
{
    class HostsFileSpec
    {
        class StringHostsFileContext
        {
            Because of = () =>
                result = new HostsFile(new StringResource(hostsFileContent));

            protected static string hostsFileContent;
            protected static HostsFile result;
        }

        [Subject(typeof(HostsFile))]
        class When_parsing_a_basic_hosts_file : StringHostsFileContext
        {
            Establish context = () =>
            {
                hostsFileContent = "127.0.0.1    host1 # comment 1\n192.168.0.1    host2";

                expectedEntries = new HostEntry[]
                {
                    new HostEntry(0, "127.0.0.1    host1 # comment 1", "    ", true, "host1", "127.0.0.1", "comment 1"),
                    new HostEntry(1, "192.168.0.1    host2", "    ", true, "host2", "192.168.0.1", null),
                };
            };

            It should_parse_individual_host_entries = () =>
                result.Entries.Should().Equal(expectedEntries);

            It should_not_be_in_a_dirty_state = () =>
                result.IsDirty.Should().Be(false);

            static HostEntry[] expectedEntries;
        }

        [Subject(typeof(HostsFile))]
        class When_parsing_a_host_name_with_a_hyphen : StringHostsFileContext
        {
            Establish context = () =>
            {
                hostsFileContent = "127.0.0.1    host1-localhost # comment 1";

                expectedEntries = new HostEntry[]
                {
                    new HostEntry(0, "127.0.0.1    host1-localhost # comment 1", "    ", true, "host1-localhost", "127.0.0.1", "comment 1"),
                };
            };

            It should_parse_correctly = () =>
                result.Entries.Should().Equal(expectedEntries);

            static HostEntry[] expectedEntries;
        }

        [Subject(typeof(HostsFile))]
        class When_parsing_a_hosts_file_that_contains_the_system_samples : StringHostsFileContext
        {
            Establish context = () =>
            {
                hostsFileContent = Resources.DefaultHostsFile;

                expectedEntries = new HostEntry[]
                {
                };
            };

            It should_ignore_the_sample_hostnames = () =>
                result.Entries.Should().Equal(expectedEntries);

            static HostEntry[] expectedEntries;
        }

        class HostFileChangesContext
        {
            Because of = () =>
            {
                StringResource stringResource = new StringResource(hostsFileContent);

                var hostsFile = new HostsFile(stringResource);

                changeAction(hostsFile);

                hostsFile.Save();

                result = stringResource.ToString();
            };

            protected static string hostsFileContent;
            protected static Action<HostsFile> changeAction;

            protected static string result;
        }

        [Subject(typeof(HostsFile))]
        class When_updating_the_hosts_file_by_modifying_enabled_states : HostFileChangesContext
        {
            Establish context = () =>
            {
                hostsFileContent = Resources.SampleHostsFile;

                changeAction = hostsFile =>
                {
                    hostsFile.Entries.First(c => c.Name == "host1.localhost").Enabled = false;
                    hostsFile.Entries.First(c => c.Name == "host2.localhost").Enabled = true;
                };
            };

            It should_match_the_expected_output = () =>
                result.Should().Be(Resources.SampleHostsFile_Disable);

            static HostEntry[] expectedEntries;
        }

        [Subject(typeof(HostsFile))]
        class When_updating_the_hosts_file_by_swapping_the_lines_of_two_entries : HostFileChangesContext
        {
            Establish context = () =>
            {
                hostsFileContent = Resources.SampleHostsFile;

                changeAction = hostsFile =>
                {
                    hostsFile.Entries.First(c => c.Name == "host1.localhost")
                        .SwapLine(hostsFile.Entries.First(c => c.Name == "host2.localhost"));
                };
            };

            It should_match_the_expected_output = () =>
                result.Should().Be(Resources.SampleHostsFile_Reorder);

            static HostEntry[] expectedEntries;
        }

        [Subject(typeof(HostsFile))]
        class When_updating_the_hosts_file_by_deleting_an_entry : HostFileChangesContext
        {
            Establish context = () =>
            {
                hostsFileContent = Resources.SampleHostsFile;

                changeAction = hostsFile =>
                {
                    hostsFile.DeleteEntry(
                        hostsFile.Entries.First(c => c.Name == "host1.localhost"));
                };
            };

            It should_match_the_expected_output = () =>
                result.Should().Be(Resources.SampleHostsFile_Delete);

            static HostEntry[] expectedEntries;
        }

        [Subject(typeof(HostsFile))]
        class When_making_numerous_seemingly_conflicting_changes : HostFileChangesContext
        {
            Establish context = () =>
            {
                hostsFileContent = Resources.ComplexHostsFile_Before;

                changeAction = hostsFile =>
                {
                    var entry1 = hostsFile.Entries.Where(c => c.Name == "host1.localhost").First();
                    var entry2 = hostsFile.Entries.Where(c => c.Name == "host2.localhost").First();
                    var entry3 = hostsFile.Entries.Where(c => c.Name == "host3.localhost").First();
                    var entry4 = hostsFile.Entries.Where(c => c.Name == "host4.localhost").First();
                    var entry5 = hostsFile.Entries.Where(c => c.Name == "host5.localhost").First();
                    var entry6 = new HostEntry("host6.localhost", "127.0.0.1", "comment 6");

                    entry1.Enabled = false;
                    entry2.Enabled = true;
                    entry3.Enabled = false;

                    entry3.SwapLine(entry5); // swap two with a deleted in between
                    entry6.SwapLine(entry2); // new swapped with existing

                    hostsFile.DeleteEntry(entry4);
                    hostsFile.AddEntry(entry6);
                };
            };

            It should_match_the_expected_output = () =>
                result.Should().Be(Resources.ComplexHostsFile_Expected);

            static HostEntry[] expectedEntries;
        }

        [Subject(typeof(HostsFile), "AddEntry")]
        class When_adding_an_entry
        {
            Establish context = () =>
            {
                sut = new HostsFile(new StringResource());

                newEntry = new HostEntry("host.localhost", "1.0.0.0", null);
            };

            Because of = () =>
                sut.AddEntry(newEntry);

            It should_contain_the_new_entry = () =>
                sut.Entries.Should().Contain(newEntry);

            It should_be_in_a_dirty_state = () =>
                sut.IsDirty.Should().Be(true);

            static HostsFile sut;
            static HostEntry newEntry;
        }

        [Subject(typeof(HostsFile), "AddEntry")]
        class When_adding_a_null_entry
        {
            Establish context = () =>
            {
                sut = new HostsFile(new StringResource());
            };

            Because of = () =>
                result = Catch.Exception(() => sut.AddEntry(null));

            It should_throw_an_exception = () =>
                result.Should().BeOfType<ArgumentNullException>();

            static HostsFile sut;
            static Exception result;
        }

        [Subject(typeof(HostsFile), "AddEntry")]
        class When_adding_a_entry_that_uses_an_ignored_hostname
        {
            Establish context = () =>
            {
                sut = new HostsFile(new StringResource());
                newEntry = new HostEntry("localhost", "1.0.0.0", null);
            };

            Because of = () =>
                result = Catch.Exception(() => sut.AddEntry(newEntry));

            It should_throw_an_exception = () =>
                result.Should().BeOfType<ArgumentException>();

            static HostsFile sut;
            static Exception result;
            static HostEntry newEntry;
        }

        [Subject(typeof(HostsFile), "DeleteEntry")]
        class When_deleting_an_entry
        {
            Establish context = () =>
            {
                sut = new HostsFile(new StringResource("127.0.0.1    host1 # comment 1\n192.168.0.1    host2"));
                entryToDelete = sut.Entries.First();
            };

            Because of = () =>
                sut.DeleteEntry(sut.Entries.First());

            It should_not_contain_the_new_entry = () =>
                sut.Entries.Should().NotContain(entryToDelete);

            It should_be_in_a_dirty_state = () =>
                sut.IsDirty.Should().Be(true);

            static HostsFile sut;
            static HostEntry entryToDelete;
        }

        [Subject(typeof(HostsFile), "DeleteEntry")]
        class When_deleting_an_entry_that_does_not_exist
        {
            Establish context = () =>
            {
                sut = new HostsFile(new StringResource());
            };

            Because of = () =>
                result = Catch.Exception(() => sut.DeleteEntry(new HostEntry("host.localhost", "1.0.0.0", null)));

            It should_not_throw_an_exception = () =>
                result.Should().BeNull();

            static HostsFile sut;
            static Exception result;
        }

        [Subject(typeof(HostsFile), "DeleteEntry")]
        class When_deleting_a_null_entry
        {
            Establish context = () =>
            {
                sut = new HostsFile(new StringResource());
            };

            Because of = () =>
                result = Catch.Exception(() => sut.DeleteEntry(null));

            It should_throw_an_exception = () =>
                result.Should().BeOfType<ArgumentNullException>();

            static HostsFile sut;
            static Exception result;
        }

        [Subject(typeof(HostsFile), "IsDirty")]
        class When_an_entry_has_been_modified
        {
            Establish context = () =>
            {
                sut = new HostsFile(new StringResource("127.0.0.1    host1 # comment 1\n192.168.0.1    host2"));
            };

            Because of = () =>
                sut.Entries.First().Enabled = false;

            It should_be_in_a_dirty_state = () =>
                sut.IsDirty.Should().Be(true);

            static HostsFile sut;
        }

        [Subject(typeof(HostsFile), "IsDirty")]
        class When_saving_changes
        {
            Establish context = () =>
            {
                sut = new HostsFile(new StringResource());

                sut.AddEntry(new HostEntry("host.localhost", "1.0.0.0", null));
            };

            Because of = () =>
                sut.Save();

            It should_no_longer_be_in_a_dirty_state = () =>
                sut.IsDirty.Should().Be(false);

            static HostsFile sut;
            static HostEntry newEntry;
        }

        [Subject(typeof(HostsFile))]
        class When_saving_changes_after_another_process_has_made_unconflicting_changes
        {
            Establish context = () =>
            {
                resource = new StringResource(Resources.SampleHostsFile);
                sut = new HostsFile(resource);

                sut.DeleteEntry(
                        sut.Entries.First(c => c.Name == "host1.localhost"));

                var other = new HostsFile(resource);
                other.Entries.First(c => c.Name == "host2.localhost").Enabled = true;
                other.Save();
            };

            Because of = () =>
                sut.Save();

            It should_include_both_sets_of_changes = () =>
                resource.ToString().Should().Be(Resources.SampleHostsFile_MultiProcess);

            static HostsFile sut;
            static StringResource resource;
        }


        [Subject(typeof(HostsFile))]
        class When_saving_a_deleted_entry_that_has_been_changed_by_another_process
        {
            Establish context = () =>
            {
                resource = new StringResource(Resources.SampleHostsFile);
                sut = new HostsFile(resource);

                sut.DeleteEntry(
                    sut.Entries.First(c => c.Name == "host1.localhost"));

                var other = new HostsFile(resource);
                other.Entries.First(c => c.Name == "host1.localhost").Enabled = false;
                other.Save();
            };

            Because of = () =>
                result = Catch.Exception(() => sut.Save());

            It should_fail_to_save = () =>
                result.Message.Should().Contain("Host file write conflict: Line 22 has been modified by another process");

            static HostsFile sut;
            static StringResource resource;
            static Exception result;
        }

        [Subject(typeof(HostsFile))]
        class When_saving_a_deleted_entry_that_has_been_deleted_by_another_process
        {
            Establish context = () =>
            {
                resource = new StringResource(Resources.SampleHostsFile);
                sut = new HostsFile(resource);

                sut.DeleteEntry(
                    sut.Entries.First(c => c.Name == "host2.localhost"));

                var other = new HostsFile(resource);
                other.DeleteEntry(
                    other.Entries.First(c => c.Name == "host2.localhost"));
                other.Save();
            };

            Because of = () =>
                result = Catch.Exception(() => sut.Save());

            It should_fail_to_save = () =>
                result.Message.Should().Contain("Host file write conflict: Line 23 has been modified by another process");

            static HostsFile sut;
            static StringResource resource;
            static Exception result;
        }

        [Subject(typeof(HostsFile))]
        class When_saving_an_updated_entry_that_has_been_changed_by_another_process
        {
            Establish context = () =>
            {
                resource = new StringResource(Resources.SampleHostsFile);
                sut = new HostsFile(resource);

                sut.Entries.First(c => c.Name == "host1.localhost").Address = "127.0.0.2";

                var other = new HostsFile(resource);
                other.Entries.First(c => c.Name == "host1.localhost").Enabled = false;
                other.Save();
            };

            Because of = () =>
                result = Catch.Exception(() => sut.Save());

            It should_fail_to_save = () =>
                result.Message.Should().Contain("Host file write conflict: Line 22 has been modified by another process");

            static HostsFile sut;
            static StringResource resource;
            static Exception result;
        }

        [Subject(typeof(HostsFile))]
        class When_saving_an_updated_entry_that_has_been_deleted_by_another_process
        {
            Establish context = () =>
            {
                resource = new StringResource(Resources.SampleHostsFile);
                sut = new HostsFile(resource);

                sut.Entries.First(c => c.Name == "host2.localhost").Address = "127.0.0.2";

                var other = new HostsFile(resource);
                other.DeleteEntry(
                    other.Entries.First(c => c.Name == "host2.localhost"));
                other.Save();
            };

            Because of = () =>
                result = Catch.Exception(() => sut.Save());

            It should_fail_to_save = () =>
                result.Message.Should().Contain("Host file write conflict: Line 23 has been modified by another process");

            static HostsFile sut;
            static StringResource resource;
            static Exception result;
        }

        [Subject(typeof(HostsFile))]
        class When_file_lock_is_released_before_timeout_for_read
        {
            Establish context = () =>
            {
                var hostsFilePath = Path.GetTempFileName();
                File.WriteAllText(hostsFilePath, "127.0.0.1 host1.localhost");

                var hostsFile = new FileInfo(hostsFilePath);
                resource = new FileInfoResource(hostsFile);

                var externalLock = resource.OpenWrite();
                Task.Delay(500).ContinueWith(_ => externalLock.Dispose());
            };

            Because of = () =>
                sut = new HostsFile(resource);

            It complete_successfully = () =>
                sut.Entries.Should().Contain(c => c.Name == "host1.localhost");

            static HostsFile sut;
            static FileInfoResource resource;
        }

        [Subject(typeof(HostsFile))]
        class When_file_lock_is_released_after_timeout_for_read
        {
            Establish context = () =>
            {
                var hostsFilePath = Path.GetTempFileName();

                var hostsFile = new FileInfo(hostsFilePath);
                resource = new FileInfoResource(hostsFile);

                var externalLock = resource.OpenWrite();
                Task.Delay(1000).ContinueWith(_ => externalLock.Dispose());
            };

            Because of = () =>
                result = Catch.Exception(() => new HostsFile(resource, maxFileLockWaitSeconds: 0.5));

            It fails = () =>
                result.Message.Should().Be("Unable to acquire file lock after 0.5 seconds");

            static HostsFile sut;
            static Exception result;
            static FileInfoResource resource;
        }

        [Subject(typeof(HostsFile))]
        class When_file_lock_is_released_before_timeout_for_write
        {
            Establish context = () =>
            {
                var hostsFilePath = Path.GetTempFileName();
                File.WriteAllText(hostsFilePath, "127.0.0.1 host1.localhost");

                var hostsFile = new FileInfo(hostsFilePath);
                resource = new FileInfoResource(hostsFile);

                sut = new HostsFile(resource, maxFileLockWaitSeconds: 1.5);
                sut.AddEntry(
                    new HostEntry("host1.localhost", "127.0.0.1", null));

                var externalLock = resource.OpenWrite();
                Task.Delay(1000).ContinueWith(_ => externalLock.Dispose());
            };

            Because of = () =>
                sut.Save();

            It complete_successfully = () =>
                sut.Entries.Should().Contain(c => c.Name == "host1.localhost");

            static HostsFile sut;
            static FileInfoResource resource;
        }

        [Subject(typeof(HostsFile))]
        class When_file_lock_is_released_after_timeout_for_write
        {
            Establish context = () =>
            {
                var hostsFilePath = Path.GetTempFileName();

                var hostsFile = new FileInfo(hostsFilePath);
                resource = new FileInfoResource(hostsFile);

                sut = new HostsFile(resource, maxFileLockWaitSeconds: 0.5);
                sut.AddEntry(
                    new HostEntry("host1.localhost", "127.0.0.1", null));

                var externalLock = resource.OpenWrite();
                Task.Delay(1000).ContinueWith(_ => externalLock.Dispose());
            };

            Because of = () =>
                result = Catch.Exception(() => sut.Save());

            It fails = () =>
                result.Message.Should().Be("Unable to acquire file lock after 0.5 seconds");

            static HostsFile sut;
            static Exception result;
            static FileInfoResource resource;
        }
    }
}
