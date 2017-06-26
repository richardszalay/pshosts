#$hostsFile = $env:temp + "\pshosts_hosts"

& "$PSScriptRoot\ImportModule.ps1"

$hostsFile = [System.IO.Path]::GetTempFileName()

Describe "Set-HostEntry" {
    BeforeEach {
        "10.10.10.10 hostname`n127.0.0.1 hostname2" > $hostsFile
    }

    AfterEach {
        Set-Content $hostsFile ""
    }

    Context "Supplying a hostname that doesn't already exist" {
        BeforeEach {
            Set-TestHostEntry -Name "hostname3" -Address "10.10.10.10" -HostsPath $hostsFile -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Emits an error" {
            $err | Should Not BeNullOrEmpty
        }

        It "Does not add the entry" {
            $results = Get-TestHostEntry -HostsPath $hostsFile

            $results.length | Should Be 2
        }
    }

    Context "Supplying address" {
        BeforeEach {
            Set-TestHostEntry -Name "hostname" -Address "10.10.10.10" -HostsPath $hostsFile
        }

        It "Sets entry address as specified" {
            $results = Get-TestHostEntry hostname -HostsPath $hostsFile

            $results.length | Should Be 1
            $results[0].Name | Should Be "hostname"
            $results[0].Address | Should Be "10.10.10.10"
        }
    }

    Context "Supplying Loopback" {
        BeforeEach {
            Set-TestHostEntry -Name "hostname" -Loopback -HostsPath $hostsFile
        }

        It "Set entry address to 127.0.0.1" {
            $results = Get-TestHostEntry hostname -HostsPath $hostsFile

            $results[0].Address | Should Be "127.0.0.1"
        }
    }

    Context "Supplying IPv6Loopback" {
        BeforeEach {
            Set-TestHostEntry -Name "hostname" -IPv6Loopback -HostsPath $hostsFile
        }

        It "Sets entry address to ::1" {
            $results = Get-TestHostEntry hostname -HostsPath $hostsFile

            $results[0].Address | Should Be "::1"
        }
    }

    Context "Supplying Loopback and Address" {
        It "Throws an error" {
            { Set-TestHostEntry -Name "hostname" -Address "127.0.0.1" -Loopback -HostsPath $hostsFile } | Should Throw
        }
    }

    Context "Supplying IPv6Loopback and Address" {
        It "Throws an error" {
            { Set-TestHostEntry -Name "hostname" -Address "127.0.0.1" -IPv6Loopback -HostsPath $hostsFile } | Should Throw
        }
    }

    Context "Supplying Loopback and IPv6Loopback" {
        It "Throws an error" {
            { Set-TestHostEntry -Name "hostname" -Loopback -IPv6Loopback -HostsPath $hostsFile } | Should Throw
        }
    }

    Context "Supplying an invalid address" {
        BeforeEach {
            Set-TestHostEntry -Name "hostname" -Address "abc.def.hij" -HostsPath $hostsFile -WarningVariable warn 2>&1 3>&1 | Out-Null
        }

        It "Emits a warning" {
            $warn | Should Not BeNullOrEmpty
        }

        # TODO: This is actually at ends with "Get-HostEntry Excludes entries with invalid IP Addresses"
        It "Sets the entry address anyway" {
            $contents = Get-Content $hostsFile -Raw
            $contents | Should Match "abc.def.hij"
        }
    }
}