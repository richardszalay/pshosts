#$hostsFile = $env:temp + "\pshosts_hosts"

& "$PSScriptRoot\ImportModule.ps1"

$hostsFile = [System.IO.Path]::GetTempFileName()

Describe "Disable-HostEntry" {
    BeforeEach {
        "10.10.10.10 hostname`n127.0.0.1 hostname2`n127.0.0.1 somethingelse.unrelated" > $hostsFile
    }

    AfterEach {
        Set-Content $hostsFile ""
    }

    Context "Supplying a hostname that doesn't exist" {
        BeforeEach {
            Disable-TestHostEntry -Name "hostname3" -HostsPath $hostsFile -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Emits an error" {
            $err | Should Not BeNullOrEmpty
        }

        It "Does not disable anything" {
            $results = Get-TestHostEntry -HostsPath $hostsFile | Where-Object { -not $_.Enabled }

            $results.length | Should Be 0
        }
    }

    Context "Supplying a hostname that exists" {
        BeforeEach {
            Disable-TestHostEntry -Name "hostname" -HostsPath $hostsFile
        }

        It "Disables the matching entry" {
            $result = Get-TestHostEntry hostname -HostsPath $hostsFile

            $result.Enabled | Should Be $false
        }

        It "Does not disable similar entries" {
            $result = Get-TestHostEntry hostname2 -HostsPath $hostsFile

            $result.Enabled | Should Be $true
        }
    }

    Context "Supplying a wildcard that matches" {
        BeforeEach {
            Disable-TestHostEntry -Name "hostname*" -HostsPath $hostsFile
        }

        It "Disables the matching entries" {
            $result = Get-TestHostEntry hostname -HostsPath $hostsFile
            $result.Enabled | Should Be $false

            $result = Get-TestHostEntry hostname2 -HostsPath $hostsFile
            $result.Enabled | Should Be $false
        }
    }

    Context "Supplying a wildcard that doesn't exist" {
        BeforeEach {
            Disable-TestHostEntry -Name "somethingelse.entirely*" -HostsPath $hostsFile -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Does not emit an error" {
            $err | Should BeNullOrEmpty
        }

        It "Does not disable anything" {
            $results = Get-TestHostEntry -HostsPath $hostsFile | Where-Object { -not $_.Enabled }

            $results.length | Should Be 0
        }
    }
}