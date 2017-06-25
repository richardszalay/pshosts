#$hostsFile = $env:temp + "\pshosts_hosts"

# TODO: Should this occur outside the tests so that the tests can be run against any build/instance of the module?
Import-Module "$PSScriptRoot\..\RichardSzalay.Hosts.Powershell\bin\Debug\PsHosts.psd1" -Prefix Test -Force

$hostsFile = [System.IO.Path]::GetTempFileName()
Describe "Get-HostEntry" {
    AfterEach {
        Set-Content $hostsFile ""
    }

    Context "Without arguments when there are entries" {
        "127.0.0.1 hostname`n127.0.0.1 hostname2" > $hostsFile
        $results = Get-TestHostEntry -HostsPath $hostsFile

        It "returns all entries" {
            $results.length | Should Be 2
        }
    }

    Context "Without arguments when an entry has an invalid IP addresses" {
        "abc.def.hij hostname`n127.0.0.1 hostname2" > $hostsFile
        $results = Get-TestHostEntry -HostsPath $hostsFile

        # TODO: This is actually at ends with "Add-HostEntry Supplying an invalid address Adds the entry anyway"
        It "excludes entries with invalid IP addresses" {
            $results.length | Should Be 1
        }
    }

    Context "With named host when there is a match" {
        "127.0.0.1 hostname`n127.0.0.1 hostname2" > $hostsFile
        $results = Get-TestHostEntry hostname -HostsPath $hostsFile

        It "returns matching entry" {
            $results.length | Should Be 1
        }
    }

    Context "With named host when there are duplicate matches" {
        "127.0.0.1 hostname`n127.0.0.1 hostname" > $hostsFile
        $results = Get-TestHostEntry hostname -HostsPath $hostsFile

        It "returns matching entries" {
            $results.length | Should Be 2
        }
    }

    Context "With wildcard name when there are entries" {
        "127.0.0.1 hostname`n127.0.0.1 hostname2" > $hostsFile
        $results = Get-TestHostEntry hostname* -HostsPath $hostsFile

        It "returns matching entries" {
            $results.length | Should Be 2
        }
    }
}