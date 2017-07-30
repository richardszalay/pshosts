

properties {
  if (-not $configuration) {
    $configuration = "Release"
  }

  $toolsDir = "$PSScriptRoot\.tools"
  $nugetDir = "$toolsDir\nuget.exe"
  $solutionPath = Join-Path $PSScriptRoot "..\RichardSzalay.Hosts.sln"
  $testFilePattern = "$PSScriptRoot\..\RichardSzalay.Hosts.Powershell.Tests\*"
  $mspecCliPath = "$PSScriptRoot\..\packages\Machine.Specifications.0.5.16\tools\mspec-clr4.exe"
  $libPath = "$PSScriptRoot\..\RichardSzalay.Hosts.Tests\bin\$configuration\RichardSzalay.Hosts.Tests.dll"
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
      Invoke-Pester -Script @{ Path = $pattern }
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
