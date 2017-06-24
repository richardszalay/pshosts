#$hostsFile = $env:temp + "\pshosts_hosts"

# TODO: Should this occur outside the tests so that the tests can be run against any build/instance of the module?
Import-Module "$PSScriptRoot\..\RichardSzalay.Hosts.Powershell\bin\Debug\PsHosts.psd1" -Prefix Test -Force

$hostsFile = [System.IO.Path]::GetTempFileName()
Describe "Add-HostEntry" {
    AfterEach {
        Remove-Item $hostsFile
    }

    Context "Supplying address" {
        Add-TestHostEntry -Name "hostname" -Address "10.10.10.10" -HostsPath $hostsFile
        $results = Get-TestHostEntry -HostsPath $hostsFile

        It "Adds entry with address" {
            $results.length | Should Be 1
            $results[0].Name | Should Be "hostname"
            $results[0].Address | Should Be "10.10.10.10"
        }
    }

    Context "Supplying an invalid address" {
        Add-TestHostEntry -Name "hostname" -Address "abc.def.hij" -HostsPath $hostsFile
        $results = Get-TestHostEntry -HostsPath $hostsFile

        It "Emits a warning" {
            throw "Not implemented"
        }

        It "Adds the entry" {
            throw "Not implemented"
        }
    }
}