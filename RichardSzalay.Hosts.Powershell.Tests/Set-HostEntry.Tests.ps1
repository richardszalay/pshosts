#$hostsFile = $env:temp + "\pshosts_hosts"

& "$PSScriptRoot\ImportModule.ps1"
. "$PSScriptRoot\TestUtils.ps1"

$hostsFile = [System.IO.Path]::GetTempFileName()
$global:PSHostsFilePath = $hostsFile

Describe "Set-HostEntry" {
    BeforeEach {
        "10.10.10.10 hostname`n127.0.0.1 hostname2" > $hostsFile
    }

    AfterEach {
        Set-Content $hostsFile ""
    }

    Context "Supplying a hostname that doesn't already exist" {
        BeforeEach {
            Set-HostEntry -Name "hostname3" -Address "10.10.10.10" -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Emits an error" {
            $err | Should Not BeNullOrEmpty
        }

        It "Does not add the entry" {
            $results = Get-HostEntry

            $results.length | Should Be 2
        }
    }

    Context "Supplying Force but not Address with a hostname that doesn't already exist" {
        BeforeEach {
            Set-HostEntry -Name "hostname3" -Comment "Test" -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Emits an error" {
            $err | Should Not BeNullOrEmpty
        }

        It "Does not add the entry" {
            $results = Get-HostEntry

            $results.length | Should Be 2
        }
    }

    Context "Supplying Force and Address with a hostname that doesn't already exist" {
        BeforeEach {
            Set-HostEntry -Name "hostname3" -Address "10.10.10.10" -Force -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Does not emit an error" {
            $err | Should BeNullOrEmpty
        }

        It "Adds the entry" {
            $results = Get-HostEntry

            $results.length | Should Be 3
        }
    }

    Context "Supplying Force and Loopback with a hostname that doesn't already exist" {
        BeforeEach {
            Set-HostEntry -Name "hostname3" -Loopback -Force -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Does not emit an error" {
            $err | Should BeNullOrEmpty
        }

        It "Adds the entry" {
            $results = Get-HostEntry

            $results.length | Should Be 3
        }
    }

    Context "Supplying Force and IPv6Loopback with a hostname that doesn't already exist" {
        BeforeEach {
            Set-HostEntry -Name "hostname3" -IPv6Loopback -Force -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Does not emit an error" {
            $err | Should BeNullOrEmpty
        }

        It "Adds the entry" {
            $results = Get-HostEntry

            $results.length | Should Be 3
        }
    }

    Context "Supplying address" {
        BeforeEach {
            Set-HostEntry -Name "hostname" -Address "10.10.10.10"
        }

        It "Sets entry address as specified" {
            $results = Get-HostEntry hostname

            $results.length | Should Be 1
            $results[0].Name | Should Be "hostname"
            $results[0].Address | Should Be "10.10.10.10"
        }
    }

    Context "Supplying Loopback" {
        BeforeEach {
            Set-HostEntry -Name "hostname" -Loopback
        }

        It "Set entry address to 127.0.0.1" {
            $results = Get-HostEntry hostname

            $results[0].Address | Should Be "127.0.0.1"
        }
    }

    Context "Supplying IPv6Loopback" {
        BeforeEach {
            Set-HostEntry -Name "hostname" -IPv6Loopback
        }

        It "Sets entry address to ::1" {
            $results = Get-HostEntry hostname

            $results[0].Address | Should Be "::1"
        }
    }

    Context "Supplying Loopback and Address" {
        It "Throws an error" {
            { Set-HostEntry -Name "hostname" -Address "127.0.0.1" -Loopback } | Should Throw
        }
    }

    Context "Supplying IPv6Loopback and Address" {
        It "Throws an error" {
            { Set-HostEntry -Name "hostname" -Address "127.0.0.1" -IPv6Loopback } | Should Throw
        }
    }

    Context "Supplying Loopback and IPv6Loopback" {
        It "Throws an error" {
            { Set-HostEntry -Name "hostname" -Loopback -IPv6Loopback } | Should Throw
        }
    }

    Context "Supplying an invalid address" {
        BeforeEach {
            Set-HostEntry -Name "hostname" -Address "abc.def.hij" -WarningVariable warn 2>&1 3>&1 | Out-Null
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

    Context "Supplying HostsPath" {
        $altHostsFile = [System.IO.Path]::GetTempFileName()

        BeforeEach {
            "#10.10.10.10 hostname`n127.0.0.1 hostname2" > $altHostsFile
            

            Set-HostEntry -Name "hostname" -Address "127.0.0.1" -HostsPath $altHostsFile
        }

        AfterEach {
            Set-Content $altHostsFile ""
        }

        It "Modifies the supplied hosts file" {
            $results = Get-HostEntry hostname -HostsPath $altHostsFile

            $results[0].Address | Should Be "127.0.0.1"
        }

        It "Does not modify the default hosts file" {
            $results = Get-HostEntry hostname

            $results[0].Address | Should Be "10.10.10.10"
        }

        Remove-Item $altHostsFile
    }


    Context "Supplying only Comment" {
        BeforeEach {
            Set-HostEntry -Name "hostname" -Comment "Testing" | Out-Null
        }

        It "Updates the comment" {
            $result = Get-HostEntry hostname
            $result.Comment | Should Be "Testing"
        }

        It "Does not update the enabled state" {
            $result = Get-HostEntry hostname
            $result.Enabled | Should Be $true
        }

        It "Does not update the address" {
            $result = Get-HostEntry hostname
            $result.Address | Should Be "10.10.10.10"
        }
    }

    Context "Supplying only Enabled" {
        BeforeEach {
            Set-HostEntry -Name "hostname" -Enabled $false | Out-Null
        }

        It "Does not the comment" {
            $result = Get-HostEntry hostname
            $result.Comment | Should BeNullOrEmpty
        }

        It "Updates the enabled state" {
            $result = Get-HostEntry hostname
            $result.Enabled | Should Be $false
        }

        It "Does not update the address" {
            $result = Get-HostEntry hostname
            $result.Address | Should Be "10.10.10.10"
        }
    }
}

Describe "Set-HostEntry Tab completion" {
    
    Add-HostEntry -Name "cat.local" -Loopback | Out-Null
    Add-HostEntry -Name "car.local" -Loopback | Out-Null
    
    @{
        ExpectedResults = @(
            @{CompletionText = "cat.local"; ResultType = "ParameterValue"}
            @{CompletionText = "car.local"; ResultType = "ParameterValue"}
        )
        TestInput = "Set-HostEntry "
    } | Get-CompletionTestCaseData | Test-Completions

    @{
        ExpectedResults = @(
            @{CompletionText = "cat.local"; ResultType = "ParameterValue"}
        )
        TestInput = "Set-HostEntry cat"
    } | Get-CompletionTestCaseData | Test-Completions

    @{
        ExpectedResults = @()
        TestInput = "Set-HostEntry caz"
    } | Get-CompletionTestCaseData | Test-Completions
}

Remove-Item $hostsFile
