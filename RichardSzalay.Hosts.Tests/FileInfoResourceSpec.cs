using FluentAssertions;
using Machine.Specifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RichardSzalay.Hosts.Tests
{
    public class FileInfoResourceSpec
    {
        [Subject(typeof(FileInfoResource), "OpenWrite")]
        public class When_writing_to_the_resource
        {
            Establish context = () =>
            {
                tempFile = new FileInfo(Path.GetTempFileName());

                File.WriteAllText(tempFile.FullName, "Test original longer text");

                sut = new FileInfoResource(tempFile);
            };

            Because of = () =>
            {
                using (Stream stream = sut.OpenWrite())
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write("Test 2");

                    stream.Flush();
                }

                result = File.ReadAllText(tempFile.FullName);
            };

            It should_overwrite_existing_contents = () =>
                result.Should().Be("Test 2");

            static FileInfoResource sut;
            static FileInfo tempFile;
            static string result;
        }
    }
}
