using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Machine.Specifications;
using Microsoft.PowerShell.Commands;
using RichardSzalay.Hosts.Powershell.Tests.Infrastructure;

namespace RichardSzalay.Hosts.Powershell.Tests
{
    public class GetHostEntrySpec
    {
        [Subject(typeof(GetHostEntryCommand))]
        class When_something
        {
            Establish context = () =>
            {
                var host = new TestPSHost();

                var runspace = RunspaceFactory.CreateRunspace(host);
                runspace.Open();

                var importModuleCommand = new ImportModuleCommand();
                importModuleCommand.Name = new [] { @"D:\Dev\OSS\pshosts\RichardSzalay.Hosts.Powershell\bin\Debug\PsHosts.psd1" };

                ICollection<PSObject> importResults;

                using (var importModulePipe = runspace.CreatePipeline())
                {
                    var command = new Command("Import-Module");
                    command.Parameters.Add(new CommandParameter("Name",
                        @"D:\Dev\OSS\pshosts\RichardSzalay.Hosts.Powershell\bin\Debug\PsHosts.psd1"));

                    importModulePipe.Commands.Add(command);
                    importModulePipe.Invoke();
                }

                using (var importModulePipe = runspace.CreatePipeline())
                {
                    var command = new Command("Get-Module");

                    importModulePipe.Commands.Add(command);
                    importResults = importModulePipe.Invoke();
                }

                Pipeline pipe = runspace.CreatePipeline();

                pipe.Commands.AddScript("Get-HostEntry");

                results = pipe.Invoke();
            };

            It should_do_something = () =>
                true.ShouldBeTrue();

            static ICollection<PSObject> results;
        }
    }
}
