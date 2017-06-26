#$hostsFile = $env:temp + "\pshosts_hosts"

& "$PSScriptRoot\ImportModule.ps1"

$hostsFile = [System.IO.Path]::GetTempFileName()

Describe "Enable-HostEntry" {
    BeforeEach {
        "#10.10.10.10 hostname`n127.0.0.1 hostname2`n127.0.0.1 somethingelse.unrelated" > $hostsFile
    }

    AfterEach {
        Set-Content $hostsFile ""
    }

    Context "Supplying a hostname that doesn't exist" {
        BeforeEach {
            Enable-TestHostEntry -Name "hostname3" -HostsPath $hostsFile -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Emits an error" {
            $err | Should Not BeNullOrEmpty
        }

        It "Does not enable anything" {
            $results = Get-TestHostEntry -HostsPath $hostsFile | Where-Object { -not $_.Enabled }

            $results.length | Should Be 1
        }
    }

    Context "Supplying a hostname that exists" {
        BeforeEach {
            Enable-TestHostEntry -Name "hostname" -HostsPath $hostsFile
        }

        It "Enables the matching entry" {
            $result = Get-TestHostEntry hostname -HostsPath $hostsFile

            $result.Enabled | Should Be $true
        }
    }

    Context "Supplying a wildcard that matches" {
        BeforeEach {
            Enable-TestHostEntry -Name "hostname*" -HostsPath $hostsFile
        }

        It "Enabled the matching entries" {
            $result = Get-TestHostEntry hostname -HostsPath $hostsFile

            $result.Enabled | Should Be $true
        }
    }

    Context "Supplying a wildcard that doesn't exist" {
        BeforeEach {
            Enable-TestHostEntry -Name "somethingelse.entirely*" -HostsPath $hostsFile -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Does not emit an error" {
            $err | Should BeNullOrEmpty
        }

        It "Does not enable anything" {
            $results = Get-TestHostEntry -HostsPath $hostsFile | Where-Object { -not $_.Enabled }

            $results.length | Should Be 1
        }
    }
}