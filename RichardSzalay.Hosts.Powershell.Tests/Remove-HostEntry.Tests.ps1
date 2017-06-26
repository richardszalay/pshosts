#$hostsFile = $env:temp + "\pshosts_hosts"

& "$PSScriptRoot\ImportModule.ps1"

$hostsFile = [System.IO.Path]::GetTempFileName()

Describe "Remove-HostEntry" {
    BeforeEach {
        "10.10.10.10 hostname`n127.0.0.1 hostname2`n127.0.0.1 somethingelse.unrelated" > $hostsFile
    }

    AfterEach {
        Set-Content $hostsFile ""
    }

    Context "Supplying a hostname that doesn't exist" {
        BeforeEach {
            Remove-TestHostEntry -Name "hostname3" -HostsPath $hostsFile -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Emits an error" {
            $err | Should Not BeNullOrEmpty
        }

        It "Does not remove anything" {
            $results = Get-TestHostEntry -HostsPath $hostsFile

            $results.length | Should Be 3
        }
    }

    Context "Supplying a hostname that exists" {
        BeforeEach {
            Remove-TestHostEntry -Name "hostname" -HostsPath $hostsFile
        }

        It "Removes the matching entry" {
            $results = Get-TestHostEntry -HostsPath $hostsFile

            $results.length | Should Be 2
        }
    }

    Context "Supplying a wildcard that matches" {
        BeforeEach {
            Remove-TestHostEntry -Name "hostname*" -HostsPath $hostsFile
        }

        It "Removes the matching entries" {
            $results = Get-TestHostEntry -HostsPath $hostsFile

            $results.length | Should Be 1
        }
    }

    Context "Supplying a wildcard that doesn't exist" {
        BeforeEach {
            Remove-TestHostEntry -Name "somethingelse.entirely*" -HostsPath $hostsFile -ErrorVariable err 2>&1 3>&1 | Out-Null
        }

        It "Does not emit an error" {
            $err | Should BeNullOrEmpty
        }

        It "Does not remove anything" {
            $results = Get-TestHostEntry -HostsPath $hostsFile

            $results.length | Should Be 3
        }
    }
}