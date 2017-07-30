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