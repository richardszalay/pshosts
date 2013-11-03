using Machine.Specifications;
using RichardSzalay.Hosts.Tests.Infrastructure;
using RichardSzalay.Hosts.Tests.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                result.Entries.ShouldEqual(expectedEntries);

            It should_not_be_in_a_dirty_state = () =>
                result.IsDirty.ShouldBeFalse();

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
                result.Entries.ShouldEqual(expectedEntries);

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
                result.Entries.ShouldEqual(expectedEntries);

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
                result.ShouldEqual(Resources.SampleHostsFile_Disable);

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
                result.ShouldEqual(Resources.SampleHostsFile_Reorder);

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
                result.ShouldEqual(Resources.SampleHostsFile_Delete);

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
                result.ShouldEqual(Resources.ComplexHostsFile_Expected);

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
                sut.Entries.ShouldContain(newEntry);

            It should_be_in_a_dirty_state = () =>
                sut.IsDirty.ShouldBeTrue();

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
                result.ShouldBeOfType<ArgumentNullException>();

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
                result = Catch.Exception(() => sut.AddEntry(null));

            It should_throw_an_exception = () =>
                result.ShouldBeOfType<ArgumentException>();

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
                sut.Entries.ShouldNotContain(entryToDelete);

            It should_be_in_a_dirty_state = () =>
                sut.IsDirty.ShouldBeTrue();

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
                result.ShouldBeNull();

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
                result.ShouldBeOfType<ArgumentNullException>();

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
                sut.IsDirty.ShouldBeTrue();

            static HostsFile sut;
        }
    }
}
