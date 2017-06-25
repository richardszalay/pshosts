#$hostsFile = $env:temp + "\pshosts_hosts"

# TODO: Should this occur outside the tests so that the tests can be run against any build/instance of the module?
Import-Module "$PSScriptRoot\..\RichardSzalay.Hosts.Powershell\bin\Debug\PsHosts.psd1" -Prefix Test -Force

$hostsFile = [System.IO.Path]::GetTempFileName()
Describe "Test-HostEntry" {
    AfterEach {
        Set-Content $hostsFile ""
    }

    Context "With a hostname that exists" {
        "127.0.0.1 hostname`n127.0.0.1 hostname2" > $hostsFile
        $result = Test-TestHostEntry hostname -HostsPath $hostsFile

        It "returns true" {
            $result | Should Be $true
        }
    }

    Context "With a name that does not exist" {
        "127.0.0.1 hostname`n127.0.0.1 hostname2" > $hostsFile
        $result = Test-TestHostEntry hostname3 -HostsPath $hostsFile

        It "returns false" {
            $result | Should Be $false
        }
    }

    Context "With a wildcard that matches" {
        "127.0.0.1 hostname`n127.0.0.1 hostname2" > $hostsFile
        $result = Test-TestHostEntry hostna* -HostsPath $hostsFile

        It "returns true" {
            $result | Should Be $true
        }
    }

    Context "With a wildcard that does not match" {
        "127.0.0.1 hostname`n127.0.0.1 hostname2" > $hostsFile
        $result = Test-TestHostEntry hostname3* -HostsPath $hostsFile

        It "returns false" {
            $result | Should Be $false
        }
    }

}