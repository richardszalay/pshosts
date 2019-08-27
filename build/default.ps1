$psversionTable | Out-String | Write-Host
Get-PSRepository | Out-String | Write-Host

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

if ($env:APPVEYOR) {
  task default -depends Appveyor
} else {
  task default -depends Test
}

task TestLib -depends Compile {
  dotnet test $solutionPath
}

task TestCmdlets -depends Compile {
  # Run powershell with -NoProfile to avoid collisions with installed PsHosts
  
  $pwsh = if ($env:_) { $env:_ } else { "powershell.exe" }
  
  
  & $pwsh -NoProfile -Command {
      param($modulePath, $pattern)

      Import-Module $modulePath -Force      
      $res = Invoke-Pester -Script @{ Path = $pattern } -OutputFormat NUnitXml -OutputFile .\ps-results.xml -PassThru

      if ($env:APPVEYOR_JOB_ID)
      {
        $wc = New-Object 'System.Net.WebClient'
        $wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit/$($env:APPVEYOR_JOB_ID)", (Resolve-Path .\ps-results.xml))
      }

      if ($res.FailedCount -gt 0) { 
          throw "$($res.FailedCount) tests failed."
      }

  } -args (Resolve-Path (Join-Path $PSScriptRoot "../RichardSzalay.Hosts.Powershell/bin/$configuration/PsHosts/net40/PsHosts.psd1")),$testFilePattern
}

task Test -depends TestLib,TestCmdlets

task Restore {
  dotnet restore $solutionPath
}

task Compile -depends Restore, Clean { 
  dotnet build $solutionPath /p:Configuration=$configuration /v:m /nologo
}

task Clean {
  dotnet build $solutionPath /t:Clean /v:m /nologo

  if (Test-Path "./output") {
    Remove-Item -Recurse "./output"
  }
}

task RegisterGallery {
  if (-not (Test-Path "./output")) {
    mkdir output | Out-Null
  }

  Unregister-PSRepository -Name pshosts-nupkg -ErrorAction SilentlyContinue

  $outputPath = [string](Resolve-Path "./output")

  Register-PSRepository -Name pshosts-nupkg -SourceLocation $outputPath -PublishLocation $outputPath
}

task UpdateModuleVersion -depends Compile {
  $moduleVersion = $env:APPVEYOR_BUILD_VERSION

  if ($moduleVersion)
  {
    Push-Location (Resolve-Path $PSScriptRoot\..\RichardSzalay.Hosts.Powershell\bin\Release\PsHosts\)
    Update-ModuleManifest -Path .\PsHosts.psd1 -ModuleVersion $moduleVersion
    Pop-Location
  }
}


task Package -depends Test, UpdateModuleVersion, RegisterGallery {
  Publish-Module -Repository pshosts-nupkg -Path (Resolve-Path ..\RichardSzalay.Hosts.Powershell\bin\Release\PsHosts)

  Unregister-PSRepository -Name pshosts-nupkg
}

task Appveyor -depends Package {
  Get-ChildItem -Path .\output\ -Filter *.nupkg | %{ Push-AppveyorArtifact $_.FullName }
}
