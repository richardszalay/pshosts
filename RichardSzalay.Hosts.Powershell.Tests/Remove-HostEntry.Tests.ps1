#$hostsFile = $env:temp + "\pshosts_hosts"

& "$PSScriptRoot\ImportModule.ps1"
. "$PSScriptRoot\TestUtils.ps1"

$hostsFile = [System.IO.Path]::GetTempFileName()
$global:PSHostsFilePath = $hostsFile

Describe "Remove-HostEntry" {
    BeforeEach {
        "10.10.10.10 hostname`n127.0.0.1 hostname2`n127.0.0.1 somethingelse.unrelated" > $hostsFile
    }

    AfterEach {
        Set-Content $hostsFile ""
    }

    Context "Supplying a hostname that doesn't exist" {
        BeforeEach {
            Remove-HostEntry -Name "hostname3" -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Emits an error" {
            $err | Should Not BeNullOrEmpty
        }

        It "Does not remove anything" {
            $results = Get-HostEntry

            $results.length | Should Be 3
        }
    }

    Context "Supplying a hostname that exists" {
        BeforeEach {
            Remove-HostEntry -Name "hostname"
        }

        It "Removes the matching entry" {
            $results = Get-HostEntry

            $results.length | Should Be 2
        }
    }

    Context "Supplying a wildcard that matches" {
        BeforeEach {
            Remove-HostEntry -Name "hostname*"
        }

        It "Removes the matching entries" {
            $results = Get-HostEntry

            $results.length | Should Be 1
        }
    }

    Context "Supplying a wildcard that doesn't exist" {
        BeforeEach {
            Remove-HostEntry -Name "somethingelse.entirely*" -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Does not emit an error" {
            $err | Should BeNullOrEmpty
        }

        It "Does not remove anything" {
            $results = Get-HostEntry

            $results.length | Should Be 3
        }
    }
}

Describe "Remove-HostEntry Tab completion" {
    
    Add-HostEntry -Name "cat.local" -Loopback | Out-Null
    Add-HostEntry -Name "car.local" -Loopback | Out-Null
    
    @{
        ExpectedResults = @(
            @{CompletionText = "cat.local"; ResultType = "ParameterValue"}
            @{CompletionText = "car.local"; ResultType = "ParameterValue"}
        )
        TestInput = "Remove-HostEntry "
    } | Get-CompletionTestCaseData | Test-Completions

    @{
        ExpectedResults = @(
            @{CompletionText = "cat.local"; ResultType = "ParameterValue"}
        )
        TestInput = "Remove-HostEntry cat"
    } | Get-CompletionTestCaseData | Test-Completions

    @{
        ExpectedResults = @()
        TestInput = "Remove-HostEntry caz"
    } | Get-CompletionTestCaseData | Test-Completions
}

Remove-Item $hostsFile
