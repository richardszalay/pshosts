using namespace System.Management.Automation
using namespace System.Management.Automation.Language
using namespace System.Collections
using namespace System.Collections.Generic

# Thanks https://github.com/PowerShell/PowerShell/blob/master/test/powershell/Language/Parser/ExtensibleCompletion.Tests.ps1

#region Testcase infrastructure

class CompletionTestResult
{
    [string]$CompletionText
    [string]$ListItemText
    [CompletionResultType]$ResultType
    [string]$ToolTip
    [bool]$Found

    [bool] Equals($Other)
    {
        if ($Other -isnot [CompletionTestResult] -and
            $Other -isnot [CompletionResult])
        {
            return $false
        }

        # Comparison is intentionally fuzzy - CompletionText and ResultType must be specified
        # but the other properties don't need to match if they aren't specified

        if ($this.CompletionText -cne $Other.CompletionText -or
            $this.ResultType -ne $Other.ResultType)
        {
            return $false
        }

        if ($this.ListItemText -cne $Other.ListItemText -and
            ![string]::IsNullOrEmpty($this.ListItemText) -and ![string]::IsNullOrEmpty($Other.ListItemText))
        {
            return $false
        }

        if ($this.ToolTip -cne $Other.ToolTip -and
            ![string]::IsNullOrEmpty($this.ToolTip) -and ![string]::IsNullOrEmpty($Other.ToolTip))
        {
            return $false
        }

        return $true
    }
}

class CompletionTestCase
{
    [CompletionTestResult[]]$ExpectedResults
    [string[]]$NotExpectedResults
    [string]$TestInput
}

function Get-Completions
{
    param([string]$inputScript, [int]$cursorColumn = $inputScript.Length)

    $results = [System.Management.Automation.CommandCompletion]::CompleteInput(
        <#inputScript#>  $inputScript,
        <#cursorColumn#> $cursorColumn,
        <#options#>      $null)

    return $results
}

function Get-CompletionTestCaseData
{
    param(
        [Parameter(ValueFromPipeline)]
        [hashtable[]]$Data)

    process
    {
        Write-Output ([CompletionTestCase[]]$Data)
    }
}

function Test-Completions
{
    param(
        [Parameter(ValueFromPipeline)]
        [CompletionTestCase[]]$TestCases)

    process
    {
        foreach ($test in $TestCases)
        {
            Context ("Command line: <" + $test.TestInput + ">") {
                $results = Get-Completions $test.TestInput
                foreach ($result in $results.CompletionMatches)
                {
                    foreach ($expected in $test.ExpectedResults)
                    {
                        if ($expected.Equals($result))
                        {
                            $expected.Found = $true
                        }
                    }
                }
                foreach ($expected in $test.ExpectedResults)
                {
                    $skip = $false
                    if ( $expected.CompletionText -match "System.Management.Automation.PerformanceData|System.Management.Automation.Security" ) { $skip = $true }
                    It ($expected.CompletionText) -skip:$skip {
                        $expected.Found | Should Be $true
                    }
                }

                foreach ($notExpected in $test.NotExpectedResults)
                {
                    It "Not expected: $notExpected" {
                        foreach ($result in $results.CompletionMatches)
                        {
                            ($result.CompletionText -ceq $notExpected) | Should Be $False
                        }
                    }
                }
            }
        }
    }
}

#endregion Testcase infrastructure
