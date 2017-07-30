#$hostsFile = $env:temp + "\pshosts_hosts"

& "$PSScriptRoot\ImportModule.ps1"

$hostsFile = [System.IO.Path]::GetTempFileName()
$global:PSHostsFilePath = $hostsFile

Describe "Add-HostEntry" {
    AfterEach {
        Set-Content $hostsFile ""
    }

    Context "Supplying address" {
        BeforeEach {
            $result = Add-HostEntry -Name "hostname" -Address "10.10.10.10"
        }

        It "Adds entry with address" {
            $results = Get-HostEntry hostname

            $results.length | Should Be 1
            $results[0].Name | Should Be "hostname"
            $results[0].Address | Should Be "10.10.10.10"
        }

        It "Emits added entry" {
            $result.Name | Should be "hostname"
        }
    }

    Context "Supplying Loopback" {
        BeforeEach {
            Add-HostEntry -Name "hostname" -Loopback 
        }

        It "Adds entry with 127.0.0.1" {
            $results = Get-HostEntry hostname 

            $results[0].Address | Should Be "127.0.0.1"
        }
    }

    # This is known to fail - https://github.com/richardszalay/pshosts/issues/8
    # Context "Piping into Set" {
    #     BeforeEach {
    #         Add-HostEntry -Name "hostname" -Loopback  | `
    #             Set-HostEntry -Address "127.0.0.2" 
    #     }

    #     It "Should update new entry" {
    #         $results = Get-HostEntry hostname 

    #         $results.Length | Should be 1
    #     }
    # }

    Context "Supplying IPv6Loopback" {
        BeforeEach {
            $result = Add-HostEntry -Name "hostname" -IPv6Loopback 
        }

        It "Adds entry with ::1" {
            $results = Get-HostEntry hostname 

            $results[0].Address | Should Be "::1"
        }
    }

    Context "Supplying Loopback and Address" {
        It "Throws an error" {
            { Add-HostEntry -Name "hostname" -Address "127.0.0.1" -Loopback  } | Should Throw
        }
    }

    Context "Supplying IPv6Loopback and Address" {
        It "Throws an error" {
            { Add-HostEntry -Name "hostname" -Address "127.0.0.1" -IPv6Loopback  } | Should Throw
        }
    }

    Context "Supplying Loopback and IPv6Loopback" {
        It "Throws an error" {
            { Add-HostEntry -Name "hostname" -Loopback -IPv6Loopback  } | Should Throw
        }
    }

    Context "Supplying an invalid address" {
        BeforeEach {
            Add-HostEntry -Name "hostname" -Address "abc.def.hij"  -WarningVariable warn 2>&1 3>&1 | Out-Null
        }

        It "Emits a warning" {
            $warn | Should Not BeNullOrEmpty
        }

        # TODO: This is actually at ends with "Get-HostEntry Excludes entries with invalid IP Addresses"
        It "Adds the entry anyway" {
            $contents = Get-Content $hostsFile -Raw
            $contents | Should Match "abc.def.hij"
        }
    }
}

Remove-Item $hostsFile
