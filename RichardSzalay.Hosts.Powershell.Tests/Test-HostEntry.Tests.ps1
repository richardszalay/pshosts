#$hostsFile = $env:temp + "\pshosts_hosts"

& "$PSScriptRoot\ImportModule.ps1"
. "$PSScriptRoot\TestUtils.ps1"

$hostsFile = [System.IO.Path]::GetTempFileName()
$global:PSHostsFilePath = $hostsFile

Describe "Test-HostEntry" {
    AfterEach {
        Set-Content $hostsFile ""
    }

    Context "With a hostname that exists" {
        "127.0.0.1 hostname`n127.0.0.1 hostname2" > $hostsFile
        $result = Test-HostEntry hostname

        It "returns true" {
            $result | Should Be $true
        }
    }

    Context "With a name that does not exist" {
        "127.0.0.1 hostname`n127.0.0.1 hostname2" > $hostsFile
        $result = Test-HostEntry hostname3

        It "returns false" {
            $result | Should Be $false
        }
    }

    Context "With a wildcard that matches" {
        "127.0.0.1 hostname`n127.0.0.1 hostname2" > $hostsFile
        $result = Test-HostEntry hostna*

        It "returns true" {
            $result | Should Be $true
        }
    }

    Context "With a wildcard that does not match" {
        "127.0.0.1 hostname`n127.0.0.1 hostname2" > $hostsFile
        $result = Test-HostEntry hostname3*

        It "returns false" {
            $result | Should Be $false
        }
    }

    Context "Supplying HostsPath" {
        $altHostsFile = [System.IO.Path]::GetTempFileName()
        "10.10.10.10 hostname3" > $altHostsFile

        It "Tests against the supplied hosts file" {
            $result = Test-HostEntry hostname3 -HostsPath $altHostsFile

            $result | Should Be $true
        }

        Remove-Item $altHostsFile
    }

}

Describe "Test-HostEntry Tab completion" {
    
    Add-HostEntry -Name "cat.local" -Loopback | Out-Null
    Add-HostEntry -Name "car.local" -Loopback | Out-Null
    
    @{
        ExpectedResults = @(
            @{CompletionText = "cat.local"; ResultType = "ParameterValue"}
            @{CompletionText = "car.local"; ResultType = "ParameterValue"}
        )
        TestInput = "Test-HostEntry "
    } | Get-CompletionTestCaseData | Test-Completions

    @{
        ExpectedResults = @(
            @{CompletionText = "cat.local"; ResultType = "ParameterValue"}
        )
        TestInput = "Test-HostEntry cat"
    } | Get-CompletionTestCaseData | Test-Completions

    @{
        ExpectedResults = @()
        TestInput = "Test-HostEntry caz"
    } | Get-CompletionTestCaseData | Test-Completions
}

Remove-Item $hostsFile
