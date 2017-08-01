using FluentAssertions;
using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace RichardSzalay.Hosts.Tests
{
    public class HostEntrySpec
    {
        [Subject(typeof(HostEntry))]
        public class When_constructing_a_host_entry_from_the_public_api
        {
            Because of = () =>
                result = new HostEntry("hostname", "address", "comment");

            It should_assign_the_given_hostname = () =>
                result.Name.Should().Be("hostname");

            It should_assign_the_given_address = () =>
                result.Address.Should().Be("address");

            It should_assign_the_given_comment = () =>
                result.Comment.Should().Be("comment");

            It should_be_enabled = () =>
                result.Enabled.Should().Be(true);

            It should_not_refer_to_a_valid_line = () =>
                result.Line.Should().Be(-1);

            It should_be_dirty = () =>
                result.IsDirty.Should().Be(true);

            It should_identify_itself_as_new = () =>
                result.IsNew.Should().Be(true);

            static HostEntry result;
        }

        [Subject(typeof(HostEntry))]
        public class When_constructing_a_host_entry_from_the_hosts_file
        {
            Because of = () =>
                result = new HostEntry(5, "original-line", " ", false, "hostname", "address", "comment");

            It should_assign_the_given_hostname = () =>
                result.Name.Should().Be("hostname");

            It should_assign_the_given_address = () =>
                result.Address.Should().Be("address");

            It should_assign_the_given_comment = () =>
                result.Comment.Should().Be("comment");

            It should_assign_the_given_enabled_State = () =>
                result.Enabled.Should().Be(false);

            It should_assign_the_given_line_number = () =>
                result.Line.Should().Be(5);

            It should_not_be_dirty = () =>
                result.IsDirty.Should().Be(false);

            It should_not_identify_itself_as_new = () =>
                result.IsNew.Should().Be(false);

            static HostEntry result;
        }

        [Subject(typeof(HostEntry), "IsNew")]
        public class When_constructing_a_host_entry_from_line_zero
        {
            Because of = () =>
                result = new HostEntry(0, "original-line", " ", false, "hostname", "address", "comment");

            It should_not_identify_itself_as_new = () =>
                result.IsNew.Should().Be(false);

            static HostEntry result;
        }

        [Subject(typeof(HostEntry), "IsDirty")]
        public class When_modifying_an_instance_without_changing_values
        {
            Establish context = () =>
                result = new HostEntry(0, "original-line", " ", false, "hostname", "address", "comment");

            Because of = () =>
            {
                result.Enabled = false;
                result.Name = "hostname";
                result.Address = "address";
                result.Comment = "comment";
            };

            It should_not_identify_itself_as_dirty = () =>
                result.IsDirty.Should().Be(false);

            static HostEntry result;
        }

        [Subject(typeof(HostEntry), "IsDirty")]
        public class When_modifying_an_entrys_hostname
        {
            Establish context = () =>
                result = new HostEntry(0, "original-line", " ", false, "hostname", "address", "comment");

            Because of = () =>
                result.Name = "hostname2";

            It should_identify_itself_as_dirty = () =>
                result.IsDirty.Should().Be(true);

            static HostEntry result;
        }

        [Subject(typeof(HostEntry), "IsDirty")]
        public class When_modifying_an_entrys_address
        {
            Establish context = () =>
                result = new HostEntry(0, "original-line", " ", false, "hostname", "address", "comment");

            Because of = () =>
                result.Address = "address2";

            It should_identify_itself_as_dirty = () =>
                result.IsDirty.Should().Be(true);

            static HostEntry result;
        }

        [Subject(typeof(HostEntry), "IsDirty")]
        public class When_modifying_an_entrys_comment
        {
            Establish context = () =>
                result = new HostEntry(0, "original-line", " ", false, "hostname", "address", "comment");

            Because of = () =>
                result.Comment = "comment2";

            It should_identify_itself_as_dirty = () =>
                result.IsDirty.Should().Be(true);

            static HostEntry result;
        }

        [Subject(typeof(HostEntry), "IsDirty")]
        public class When_modifying_an_entrys_enabled_state
        {
            Establish context = () =>
                result = new HostEntry(0, "original-line", " ", false, "hostname", "address", "comment");

            Because of = () =>
                result.Enabled = true;

            It should_identify_itself_as_dirty = () =>
                result.IsDirty.Should().Be(true);

            static HostEntry result;
        }

        [Subject(typeof(HostEntry), "SwapLine")]
        public class When_swapping_the_position_of_two_entries
        {
            Establish context = () =>
            {
                entryA = new HostEntry(5, "original-line", " ", false, "hostname", "address", "comment");
                entryB = new HostEntry(10, "original-line", " ", false, "hostname", "address", "comment");
            };

            Because of = () =>
                entryA.SwapLine(entryB);

            It should_mark_the_swapper_as_dirty = () =>
                entryA.IsDirty.Should().Be(true);

            It should_assign_the_swapees_line_number_to_the_swapper = () =>
                entryA.Line.Should().Be(10);

            It should_mark_the_swappee_as_dirty = () =>
                entryB.IsDirty.Should().Be(true);

            It should_assign_the_swapers_line_number_to_the_swappee = () =>
                entryB.Line.Should().Be(5);

            static HostEntry entryA;
            static HostEntry entryB;
        }

        [Subject(typeof(HostEntry), "SwapLine")]
        public class When_swapping_position_with_a_new_entry
        {
            Establish context = () =>
            {
                entryA = new HostEntry(-1, "original-line", " ", false, "hostname", "address", "comment");
                entryB = new HostEntry(10, "original-line", " ", false, "hostname", "address", "comment");
            };

            Because of = () =>
                entryA.SwapLine(entryB);

            It should_also_swap_the_new_status = () =>
            {
                entryA.IsNew.Should().Be(false);
                entryB.IsNew.Should().Be(true);
            };

            static HostEntry entryA;
            static HostEntry entryB;
        }

        [Subject(typeof(HostEntry), "ToString")]
        public class When_formatting_a_host_entry_that_is_disabled
        {
            Establish context = () =>
                sut = new HostEntry(0, null, " ", false, "hostname", "address", "comment");

            Because of = () =>
                result = sut.ToString();

            It should_prefix_the_result_with_a_comment_character = () =>
                result.Should().StartWith("#");

            static HostEntry sut;
            static string result;
        }

        [Subject(typeof(HostEntry), "ToString")]
        public class When_formatting_a_host_entry_that_is_enabled
        {
            Establish context = () =>
                sut = new HostEntry(0, null, " ", true, "hostname", "address", "comment");

            Because of = () =>
                result = sut.ToString();

            It should_not_prefix_the_result_with_a_comment_character = () =>
                result.Should().StartWith("address");

            static HostEntry sut;
            static string result;
        }

        [Subject(typeof(HostEntry), "ToString")]
        public class When_formatting_a_host_entry_with_a_previous_spacing_policy
        {
            Establish context = () =>
                sut = new HostEntry(0, null, "\t", false, "hostname", "address", "comment");

            Because of = () =>
                result = sut.ToString();

            It should_regenerate_using_the_same_spacing_policy = () =>
                result.Should().Be("# address\thostname # comment");

            static HostEntry sut;
            static string result;
        }

        [Subject(typeof(HostEntry), "ToString")]
        public class When_formatting_a_host_entry_with_a_comment
        {
            Establish context = () =>
                sut = new HostEntry(0, null, "\t", true, "hostname", "address", "comment");

            Because of = () =>
                result = sut.ToString();

            It should_regenerate_using_the_same_spacing_policy = () =>
                result.Should().EndWith(" # comment");

            static HostEntry sut;
            static string result;
        }

        [Subject(typeof(HostEntry), "ToString")]
        public class When_formatting_a_host_entry_with_no_comment
        {
            Establish context = () =>
                sut = new HostEntry(0, null, "\t", true, "hostname", "address", null);

            Because of = () =>
                result = sut.ToString();

            It should_not_include_an_empty_comment = () =>
                result.Should().EndWith("hostname");

            static HostEntry sut;
            static string result;
        }


        [Subject(typeof(HostEntry), "ToString")]
        public class When_formatting_a_host_entry_that_is_dirty
        {
            Establish context = () =>
            {
                sut = new HostEntry(0, "original-line", " ", false, "hostname", "address", "comment");
                sut.Name = "hostname2";
            };

            Because of = () =>
                result = sut.ToString();

            It should_regenerate_the_line = () =>
                result.Should().Be("# address hostname2 # comment");

            static HostEntry sut;
            static string result;
        }

        [Subject(typeof(HostEntry), "ToString")]
        public class When_formatting_a_host_entry_that_is_not_dirty_but_has_no_original_line
        {
            Establish context = () =>
                sut = new HostEntry(0, null, " ", false, "hostname", "address", "comment");

            Because of = () =>
                result = sut.ToString();

            It should_generate_the_line = () =>
                result.Should().Be("# address hostname # comment");

            static HostEntry sut;
            static string result;
        }

        [Subject(typeof(HostEntry), "ToString")]
        public class When_formatting_a_host_entry_that_is_not_dirty
        {
            Establish context = () =>
                sut = new HostEntry(0, "original-line", " ", true, "hostname", "address", "comment");

            Because of = () =>
                result = sut.ToString();

            It should_not_regenerate_the_line = () =>
                result.Should().Be("original-line");

            static HostEntry sut;
            static string result;
        }

        [Subject(typeof(HostEntry), "Equals")]
        class When_comparing_two_identical_host_entries : HostEntryComparisonContext
        {
            It should_return_true = () =>
                result.Should().Be(true);
        }

        [Subject(typeof(HostEntry), "Equals")]
        class When_comparing_two_host_entries_with_different_hostnames : HostEntryComparisonContext
        {
            Establish context = () =>
                entryB.Name = "hostname2";

            It should_return_false = () =>
                result.Should().Be(false);
        }

        [Subject(typeof(HostEntry), "Equals")]
        class When_comparing_two_host_entries_with_different_addresses : HostEntryComparisonContext
        {
            Establish context = () =>
                entryB.Address = "address2";

            It should_return_false = () =>
                result.Should().Be(false);
        }

        [Subject(typeof(HostEntry), "Equals")]
        class When_comparing_two_host_entries_with_different_comments : HostEntryComparisonContext
        {
            Establish context = () =>
            {
                entryA = new HostEntry(5, null, "    ", false, "hostname", "address", "comment");
                entryB = new HostEntry(6, null, "    ", false, "hostname", "address", "comment");
            };

            It should_return_false = () =>
                result.Should().Be(false);
        }

        class HostEntryComparisonContext
        {
            Establish context = () =>
            {
                entryA = new HostEntry(5, null, "    ", false, "hostname", "address", "comment");
                entryB = new HostEntry(5, null, "    ", false, "hostname", "address", "comment");
            };

            Because of = () =>
                result = entryA.Equals(entryB);

            protected static HostEntry entryA;
            protected static HostEntry entryB;
            protected static bool result;
        }

        [Subject(typeof(HostEntry), "IsIgnoredHostname")]
        class When_checking_if_an_ignored_hostname_is_ignored
        {
            Because of = () =>
                result = HostEntry.IsIgnoredHostname("rhino.acme.com");

            It should_be_ignored = () =>
                result.Should().Be(true);

            static bool result;
        }

        [Subject(typeof(HostEntry), "IsIgnoredHostname")]
        class When_checking_if_a_regular_hostname_is_ignored
        {
            Because of = () =>
                result = HostEntry.IsIgnoredHostname("www.google.com");

            It should_not_be_ignored = () =>
                result.Should().Be(false);

            static bool result;
        }

        [Subject(typeof(HostEntry), "IPAddress")]
        class When_address_is_valid_ipv4_address
        {
            Because of = () =>
                result = new HostEntry("tempuri.org", "10.10.10.10", null);

            It should_return_an_IPAddress_object = () =>
                result.IPAddress.Should().Be(IPAddress.Parse("10.10.10.10"));

            static HostEntry result;
        }

        [Subject(typeof(HostEntry), "IPAddress")]
        class When_address_is_valid_ipv6_address
        {
            Because of = () =>
                result = new HostEntry("tempuri.org", "2001:db8::ff00:42:8329", null);

            It should_return_an_IPAddress_object = () =>
                result.IPAddress.Should().Be(IPAddress.Parse("2001:db8::ff00:42:8329"));

            static HostEntry result;
        }

        [Subject(typeof(HostEntry), "IPAddress")]
        class When_address_is_an_invalid_ip_address
        {
            Because of = () =>
                result = new HostEntry("tempuri.org", "abc.efg.hij.klm", null);

            It should_return_an_IPAddress_object = () =>
                result.IPAddress.Should().BeNull();

            static HostEntry result;
        }

        [Subject(typeof(HostEntry), "IPAddress")]
        class When_address_is_null
        {
            Because of = () =>
                result = new HostEntry("tempuri.org", null, null);

            It should_return_an_IPAddress_object = () =>
                result.IPAddress.Should().BeNull();

            static HostEntry result;
        }

        [Subject(typeof(HostEntry), "IPAddress")]
        class When_address_is_changed
        {
            Establish context = () =>
                sut = new HostEntry("tempuri.org", null, null);

            Because of = () =>
                sut.Address = "10.10.10.10";

            It should_recalculate_IPAddress = () =>
                sut.IPAddress.Should().Be(IPAddress.Parse("10.10.10.10"));

            static HostEntry sut;
        }

        [Subject(typeof(HostEntry), "IsLoopback")]
        class When_address_is_ipv4_loopback
        {
            Because of = () =>
                result = new HostEntry("tempuri.org", "127.0.0.1", null);

            It should_be_true = () =>
                result.IsLoopback.Should().Be(true);

            static HostEntry result;
        }

        [Subject(typeof(HostEntry), "IsLoopback")]
        class When_address_is_ipv6_loopback
        {
            Because of = () =>
                result = new HostEntry("tempuri.org", "::1", null);

            It should_be_true = () =>
                result.IsLoopback.Should().Be(true);

            static HostEntry result;
        }

        [Subject(typeof(HostEntry), "IsLoopback")]
        class When_address_is_not_loopback
        {
            Because of = () =>
                result = new HostEntry("tempuri.org", "10.10.10.10", null);

            It should_be_true = () =>
                result.IsLoopback.Should().Be(false);

            static HostEntry result;
        }

        [Subject(typeof(HostEntry), "IsLoopback")]
        class When_address_is_not_valid_ipaddress
        {
            Because of = () =>
                result = new HostEntry("tempuri.org", "abc.def.hij", null);

            It should_be_true = () =>
                result.IsLoopback.Should().Be(false);

            static HostEntry result;
        }
    }
}
