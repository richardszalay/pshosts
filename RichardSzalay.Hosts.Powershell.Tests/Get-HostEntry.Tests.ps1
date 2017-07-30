#$hostsFile = $env:temp + "\pshosts_hosts"

& "$PSScriptRoot\ImportModule.ps1"
. "$PSScriptRoot\TestUtils.ps1"

$hostsFile = [System.IO.Path]::GetTempFileName()
$global:PSHostsFilePath = $hostsFile


Describe "Get-HostEntry" {
    AfterEach {
        Set-Content $hostsFile ""
    }

    Context "Without arguments when there are entries" {
        "127.0.0.1 hostname`n127.0.0.1 hostname2" > $hostsFile
        $results = Get-HostEntry

        It "returns all entries" {
            $results.length | Should Be 2
        }
    }

    Context "Without arguments when an entry has an invalid IP addresses" {
        "abc.def.hij hostname`n127.0.0.1 hostname2" > $hostsFile
        $results = Get-HostEntry

        # TODO: This is actually at ends with "Add-HostEntry Supplying an invalid address Adds the entry anyway"
        It "excludes entries with invalid IP addresses" {
            $results.length | Should Be 1
        }
    }

    Context "With named host when there is a match" {
        "127.0.0.1 hostname`n127.0.0.1 hostname2" > $hostsFile
        $results = Get-HostEntry hostname

        It "returns matching entry" {
            $results.length | Should Be 1
        }
    }

    Context "With named host when there are duplicate matches" {
        "127.0.0.1 hostname`n127.0.0.1 hostname" > $hostsFile
        $results = Get-HostEntry hostname

        It "returns matching entries" {
            $results.length | Should Be 2
        }
    }

    Context "With wildcard name when there are entries" {
        "127.0.0.1 hostname`n127.0.0.1 hostname2" > $hostsFile
        $results = Get-HostEntry hostname*

        It "returns matching entries" {
            $results.length | Should Be 2
        }
    }
}

Describe "Get-HostEntry Tab completion" {
    
    Add-HostEntry -Name "cat.local" -Loopback | Out-Null
    Add-HostEntry -Name "car.local" -Loopback | Out-Null
    
    @{
        ExpectedResults = @(
            @{CompletionText = "cat.local"; ResultType = "ParameterValue"}
            @{CompletionText = "car.local"; ResultType = "ParameterValue"}
        )
        TestInput = "Get-HostEntry "
    } | Get-CompletionTestCaseData | Test-Completions

    @{
        ExpectedResults = @(
            @{CompletionText = "cat.local"; ResultType = "ParameterValue"}
        )
        TestInput = "Get-HostEntry cat"
    } | Get-CompletionTestCaseData | Test-Completions

    @{
        ExpectedResults = @()
        TestInput = "Get-HostEntry caz"
    } | Get-CompletionTestCaseData | Test-Completions
}

Remove-Item $hostsFile