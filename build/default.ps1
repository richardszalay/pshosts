

properties {
  if (-not $configuration) {
    $configuration = "Release"
  }

  $toolsDir = "$PSScriptRoot\.tools"
  $nugetDir = "$toolsDir\nuget.exe"
  $solutionPath = Join-Path $PSScriptRoot "..\RichardSzalay.Hosts.sln"
  $testFilePattern = "$PSScriptRoot\..\RichardSzalay.Hosts.Powershell.Tests\*"
  $libPath = "$PSScriptRoot\..\RichardSzalay.Hosts.Tests\bin\$configuration\RichardSzalay.Hosts.Tests.dll"
  $mspecCliPath = "$PSScriptRoot\..\packages\Machine.Specifications.Runner.Console.0.9.3\tools\mspec-clr4.exe"
}

task default -depends Test

task TestLib -depends Compile {
  & $mspecCliPath $libPath
}

task TestCmdlets -depends Compile {
  # Run powershell with -NoProfile to avoid collisions with installed PsHosts
  powershell.exe -NoProfile -Command {
      param($modulePath, $pattern)

      Import-Module $modulePath -Force      
      Invoke-Pester -Script @{ Path = $pattern } -OutputFormat NUnitXml -OutputFile .\ps-results.xml

      if ($env:APPVEYOR_JOB_ID)
      {
        $wc = New-Object 'System.Net.WebClient'
        $wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit/$($env:APPVEYOR_JOB_ID)", (Resolve-Path .\ps-results.xml))
      }

  } -args "$PSScriptRoot\..\RichardSzalay.Hosts.Powershell\bin\$configuration\PsHosts\PsHosts.psd1",$testFilePattern
}

task Test -depends TestLib,TestCmdlets

task GetNuget {
    if (-not (Test-Path $nugetDir)) {
        if (-not (Test-Path $toolsDir)) {
            mkdir $toolsDir | Out-Null
        }

        Invoke-WebRequest -OutFile "$toolsDir\nuget.exe" -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
    }
}

task Restore -depends GetNuGet {
    & ".\.tools\nuget.exe" restore $solutionPath
}

task Compile -depends Restore, Clean { 
  msbuild $solutionPath /p:Configuration=$configuration /v:m /nologo
}

task Clean {
  msbuild $solutionPath /t:Clean /v:m /nologo
}
