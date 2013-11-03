using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
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
                result.Name.ShouldEqual("hostname");

            It should_assign_the_given_address = () =>
                result.Address.ShouldEqual("address");

            It should_assign_the_given_comment = () =>
                result.Comment.ShouldEqual("comment");

            It should_be_enabled = () =>
                result.Enabled.ShouldBeTrue();

            It should_not_refer_to_a_valid_line = () =>
                result.Line.ShouldEqual(-1);

            It should_be_dirty = () =>
                result.IsDirty.ShouldBeTrue();

            It should_identify_itself_as_new = () =>
                result.IsNew.ShouldBeTrue();

            static HostEntry result;
        }

        [Subject(typeof(HostEntry))]
        public class When_constructing_a_host_entry_from_the_hosts_file
        {
            Because of = () =>
                result = new HostEntry(5, "original-line", " ", false, "hostname", "address", "comment");

            It should_assign_the_given_hostname = () =>
                result.Name.ShouldEqual("hostname");

            It should_assign_the_given_address = () =>
                result.Address.ShouldEqual("address");

            It should_assign_the_given_comment = () =>
                result.Comment.ShouldEqual("comment");

            It should_assign_the_given_enabled_State = () =>
                result.Enabled.ShouldBeFalse();

            It should_assign_the_given_line_number = () =>
                result.Line.ShouldEqual(5);

            It should_not_be_dirty = () =>
                result.IsDirty.ShouldBeFalse();

            It should_not_identify_itself_as_new = () =>
                result.IsNew.ShouldBeFalse();

            static HostEntry result;
        }

        [Subject(typeof(HostEntry), "IsNew")]
        public class When_constructing_a_host_entry_from_line_zero
        {
            Because of = () =>
                result = new HostEntry(0, "original-line", " ", false, "hostname", "address", "comment");

            It should_not_identify_itself_as_new = () =>
                result.IsNew.ShouldBeFalse();

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
                result.IsDirty.ShouldBeFalse();

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
                result.IsDirty.ShouldBeTrue();

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
                result.IsDirty.ShouldBeTrue();

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
                result.IsDirty.ShouldBeTrue();

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
                result.IsDirty.ShouldBeTrue();

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
                entryA.IsDirty.ShouldBeTrue();

            It should_assign_the_swapees_line_number_to_the_swapper = () =>
                entryA.Line.ShouldEqual(10);

            It should_mark_the_swappee_as_dirty = () =>
                entryB.IsDirty.ShouldBeTrue();

            It should_assign_the_swapers_line_number_to_the_swappee = () =>
                entryB.Line.ShouldEqual(5);

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
                entryA.IsNew.ShouldBeFalse();
                entryB.IsNew.ShouldBeTrue();
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
                result.ShouldStartWith("#");

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
                result.ShouldStartWith("address");

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
                result.ShouldEqual("# address\thostname # comment");

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
                result.ShouldEndWith(" # comment");

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
                result.ShouldEndWith("hostname");

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
                result.ShouldEqual("# address hostname2 # comment");

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
                result.ShouldEqual("# address hostname # comment");

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
                result.ShouldEqual("original-line");

            static HostEntry sut;
            static string result;
        }

        [Subject(typeof(HostEntry), "Equals")]
        class When_comparing_two_identical_host_entries : HostEntryComparisonContext
        {
            It should_return_true = () =>
                result.ShouldBeTrue();
        }

        [Subject(typeof(HostEntry), "Equals")]
        class When_comparing_two_host_entries_with_different_hostnames : HostEntryComparisonContext
        {
            Establish context = () =>
                entryB.Name = "hostname2";

            It should_return_false = () =>
                result.ShouldBeFalse();
        }

        [Subject(typeof(HostEntry), "Equals")]
        class When_comparing_two_host_entries_with_different_addresses : HostEntryComparisonContext
        {
            Establish context = () =>
                entryB.Address = "address2";

            It should_return_false = () =>
                result.ShouldBeFalse();
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
                result.ShouldBeFalse();
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
                result.ShouldBeTrue();

            static bool result;
        }

        [Subject(typeof(HostEntry), "IsIgnoredHostname")]
        class When_checking_if_a_regular_hostname_is_ignored
        {
            Because of = () =>
                result = HostEntry.IsIgnoredHostname("www.google.com");

            It should_not_be_ignored = () =>
                result.ShouldBeFalse();

            static bool result;
        }
    }
}
