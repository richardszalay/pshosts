#$hostsFile = $env:temp + "\pshosts_hosts"

& "$PSScriptRoot\ImportModule.ps1"
. "$PSScriptRoot\TestUtils.ps1"

$hostsFile = [System.IO.Path]::GetTempFileName()
$global:PSHostsFilePath = $hostsFile

Describe "Disable-HostEntry" {
    BeforeEach {
        "10.10.10.10 hostname`n127.0.0.1 hostname2`n127.0.0.1 somethingelse.unrelated" > $hostsFile
    }

    AfterEach {
        Set-Content $hostsFile ""
    }

    Context "Supplying a hostname that doesn't exist" {
        BeforeEach {
            Disable-HostEntry -Name "hostname3" -ErrorAction SilentlyContinue -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Emits an error" {
            $err | Should Not BeNullOrEmpty
        }

        It "Does not disable anything" {
            $results = Get-HostEntry | Where-Object { -not $_.Enabled }

            $results.length | Should Be 0
        }
    }

    Context "Supplying a hostname that exists" {
        BeforeEach {
            Disable-HostEntry -Name "hostname"
        }

        It "Disables the matching entry" {
            $result = Get-HostEntry hostname

            $result.Enabled | Should Be $false
        }

        It "Does not disable similar entries" {
            $result = Get-HostEntry hostname2

            $result.Enabled | Should Be $true
        }
    }

    Context "Supplying a wildcard that matches" {
        BeforeEach {
            Disable-HostEntry -Name "hostname*"
        }

        It "Disables the matching entries" {
            $result = Get-HostEntry hostname
            $result.Enabled | Should Be $false

            $result = Get-HostEntry hostname2
            $result.Enabled | Should Be $false
        }
    }

    Context "Supplying a wildcard that doesn't exist" {
        BeforeEach {
            Disable-HostEntry -Name "somethingelse.entirely*" -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Does not emit an error" {
            $err | Should BeNullOrEmpty
        }

        It "Does not disable anything" {
            $results = Get-HostEntry | Where-Object { -not $_.Enabled }

            $results.length | Should Be 0
        }
    }

    Context "Supplying HostsPath" {
        $altHostsFile = [System.IO.Path]::GetTempFileName()

        BeforeEach {
            Add-HostEntry -Name "hostname" -Loopback -HostsPath $altHostsFile | Out-Null

            Disable-HostEntry -Name "hostname" -HostsPath $altHostsFile
        }

        AfterEach {
            Set-Content $altHostsFile ""
        }

        It "Modifies the supplied hosts file" {
            $results = Get-HostEntry -HostsPath $altHostsFile | Where-Object { -not $_.Enabled }

            $results.length | Should Be 1
        }

        It "Does not modify the default hosts file" {
            $results = Get-HostEntry | Where-Object { -not $_.Enabled }

            $results.length | Should Be 0
        }

        Remove-Item $altHostsFile
    }
}

Describe "Disable-HostEntry Tab completion" {
    
    Add-HostEntry -Name "cat.local" -Loopback | Out-Null
    Add-HostEntry -Name "car.local" -Loopback | Out-Null
    
    @{
        ExpectedResults = @(
            @{CompletionText = "cat.local"; ResultType = "ParameterValue"}
            @{CompletionText = "car.local"; ResultType = "ParameterValue"}
        )
        TestInput = "Disable-HostEntry "
    } | Get-CompletionTestCaseData | Test-Completions

    @{
        ExpectedResults = @(
            @{CompletionText = "cat.local"; ResultType = "ParameterValue"}
        )
        TestInput = "Disable-HostEntry cat"
    } | Get-CompletionTestCaseData | Test-Completions

    @{
        ExpectedResults = @()
        TestInput = "Disable-HostEntry caz"
    } | Get-CompletionTestCaseData | Test-Completions
}

Remove-Item $hostsFile
