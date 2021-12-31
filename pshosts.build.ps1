#
# To build, make sure you've installed InvokeBuild
#   Install-Module -Repository PowerShellGallery -Name InvokeBuild -RequiredVersion 3.1.0
#
# Then:
#   Invoke-Build
#
# Or:
#   Invoke-Build -Task ZipRelease
#
# Or:
#   Invoke-Build -Configuration Debug
#
# etc.
#

[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = (property Configuration Release),

    [ValidateSet("netstandard2.0", "net5.0", "net6.0")]
    [string]$Framework = "net6.0",

    [switch]$CheckHelpContent
)

Import-Module "$PSScriptRoot/tools/helper.psm1"

# Final bits to release go here
$targetDir = "bin/$Configuration/PsHosts"

if (-not $Framework)
{
    $Framework = if ($PSVersionTable.PSEdition -eq "Core") { "net6.0" } else { "net461" }
}

Write-Verbose "Building for '$Framework'" -Verbose

function ConvertTo-CRLF([string] $text) {
    $text.Replace("`r`n","`n").Replace("`n","`r`n")
}

$binaryModuleParams = @{
    Inputs  = { Get-ChildItem RichardSzalay.Hosts/*.cs, RichardSzalay.Hosts/RichardSzalay.Hosts.csproj, RichardSzalay.Hosts.PowerShell/*.cs, RichardSzalay.Hosts.PowerShell/RichardSzalay.Hosts.PowerShell.csproj }
    Outputs = "RichardSzalay.Hosts.PowerShell/bin/$Configuration/netstandard2.0/RichardSzalay.Hosts.PowerShell.dll"
}

$libTestParams = @{
    Inputs = { Get-ChildItem RichardSzalay.Hosts.Tests/*.cs, RichardSzalay.Hosts.Tests/RichardSzalay.Hosts.Tests.csproj }
    Outputs = "RichardSzalay.Hosts.Tests/bin/$Configuration/$Framework/RichardSzalay.Hosts.Tests.dll"
}

<#
Synopsis: Build main binary module
#>
task BuildMainModule @binaryModuleParams {
    exec { dotnet publish -c $Configuration RichardSzalay.Hosts.PowerShell }
}

<#
Synopsis: Build xUnit tests
#>
task BuildLibTests @libTestParams {
    exec { dotnet publish -f $Framework -c $Configuration RichardSzalay.Hosts.Tests }
}


<#
Synopsis: Run the unit tests
#>
task RunLibTests BuildLibTests, {
	$testResultFolder = Join-Path $pwd 'TestResults'
	$testResultFile = "MSpecTestResults.xml" -f $Layout
	$testResultFile = Join-Path $testResultFolder $testResultFile
	
	exec { dotnet test --no-build -c $Configuration -f $Framework --logger "trx;LogFileName=$testResultFile" RichardSzalay.Hosts.Tests }
	
	Test-TrxTestResults $testResultFile > $null
}

<#
Synopsis: Run the unit tests
#>
task RunLibTests BuildLibTests, {
	$testResultFolder = Join-Path $pwd 'TestResults'
	$testResultFile = "MSpecTestResults.xml" -f $Layout
	$testResultFile = Join-Path $testResultFolder $testResultFile
	
	exec { dotnet test --no-build -c $Configuration -f $Framework --logger "trx;LogFileName=$testResultFile" RichardSzalay.Hosts.Tests }
	
	Test-TrxTestResults $testResultFile > $null
}

<#
Synopsis: Run the Pester tests
#>
task RunPesterTests LayoutModule, {
    & (Get-PSExePath) -NoProfile -Command {
      param($scriptRoot, $targetDir)
	  
	$modulePath = "$scriptRoot/$targetDir/PsHosts.psd1"
	$pattern = "$scriptRoot/RichardSzalay.Hosts.Powershell.Tests/*"
	  
	Import-Module "$scriptRoot/tools/helper.psm1"
        Import-Module $modulePath -Force      

        if (-not (Get-Module -Name Pester -ListAvailable)) {
          Write-Log -Warning "Module 'Pester' is missing. Installing 'Pester' ..."
          Install-Module -Name Pester -Scope CurrentUser -Force -RequiredVersion 4.10.1
        }

        $res = Invoke-Pester -Script @{ Path = $pattern } -OutputFormat NUnitXml -OutputFile .\ps-results.xml -PassThru

        if ($env:APPVEYOR_JOB_ID)
        {
          $wc = New-Object 'System.Net.WebClient'
          $wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit/$($env:APPVEYOR_JOB_ID)", (Resolve-Path .\ps-results.xml))
        }

        if ($res.FailedCount -gt 0) { 
          throw "$($res.FailedCount) tests failed."
        }
  } -args "$PSScriptRoot", $targetDir
}

<#
Synopsis: Run all tests
#>
task RunTests RunLibTests, RunPesterTests

<#
Synopsis: Check if the help content is in sync.
#>
task CheckHelpContent -If $CheckHelpContent {
    # This step loads the dll that was just built, so only do that in another process
    # so the file isn't locked in any way for the rest of the build.
    $psExePath = Get-PSExePath
    & $psExePath -NoProfile -NonInteractive -File $PSScriptRoot/tools/CheckHelp.ps1 $Configuration
    assert ($LASTEXITCODE -eq 0) "Checking help and function signatures failed"
}

<#
Synopsis: Copy all of the files that belong in the module to one place in the layout for installation
#>
task LayoutModule BuildMainModule, {
    if (-not (Test-Path $targetDir -PathType Container)) {
        New-Item $targetDir -ItemType Directory -Force > $null
    }

    $extraFiles =
        'RichardSzalay.Hosts.Powershell/PsHosts.psd1',
        'RichardSzalay.Hosts.Powershell/PsHosts.ParameterCompletion.psm1',
        'RichardSzalay.Hosts.Powershell/PsHosts.format.ps1xml'

    foreach ($file in $extraFiles) {
        # ensure files have \r\n line endings as the signing tool only uses those endings to avoid mixed endings
        $content = Get-Content -Path $file -Raw
        Set-Content -Path (Join-Path $targetDir (Split-Path $file -Leaf)) -Value (ConvertTo-CRLF $content) -Force
    }

    $binPath = "RichardSzalay.Hosts.Powershell/bin/$Configuration/netstandard2.0/publish"
    Copy-Item $binPath/RichardSzalay.Hosts.Powershell.dll $targetDir
    Copy-Item $binPath/RichardSzalay.HostEntry.dll $targetDir

    if ($Configuration -eq 'Debug') {
        Copy-Item $binPath/*.pdb $targetDir
    }

    # Copy module manifest, but fix the version to match what we've specified in the binary module.
    $moduleManifestContent = ConvertTo-CRLF (Get-Content -Path 'RichardSzalay.Hosts.Powershell/PsHosts.psd1' -Raw)
    $versionInfo = (Get-ChildItem -Path $targetDir/RichardSzalay.Hosts.Powershell.dll).VersionInfo
    $version = $versionInfo.FileVersion
    $semVer = $versionInfo.ProductVersion

    if ($semVer -match "(.*)-(.*)") {
        # Make sure versions match
        if ($matches[1] -ne $version) { throw "AssemblyFileVersion mismatch with AssemblyInformationalVersion" }
        $prerelease = $matches[2]

        # Put the prerelease tag in private data
        $moduleManifestContent = [regex]::Replace($moduleManifestContent, "}", "PrivateData = @{ PSData = @{ Prerelease = '$prerelease'; ProjectUri = 'https://github.com/richardszalay/pshosts' } }$([System.Environment]::Newline)}")
    }

    $moduleManifestContent = [regex]::Replace($moduleManifestContent, "ModuleVersion = '.*'", "ModuleVersion = '$version'")
    $moduleManifestContent | Set-Content -Path $targetDir/PsHosts.psd1

    # Make sure we don't ship any read-only files
    foreach ($file in (Get-ChildItem -Recurse -File $targetDir)) {
        $file.IsReadOnly = $false
    }
}, CheckHelpContent

<#
Synopsis: Zip up the binary for release.
#>
task ZipRelease LayoutModule, {
    Compress-Archive -Force -LiteralPath $targetDir -DestinationPath "bin/$Configuration/PsHosts.zip"
}

<#
Synopsis: Install newly built PsHosts
#>
task Install LayoutModule, {

    function Install($InstallDir) {
        if (!(Test-Path -Path $InstallDir))
        {
            New-Item -ItemType Directory -Force $InstallDir
        }

        try
        {
            if (Test-Path -Path $InstallDir\PsHosts)
            {
                Remove-Item -Recurse -Force $InstallDir\PsHosts -ErrorAction Stop
            }
            Copy-Item -Recurse $targetDir $InstallDir
        }
        catch
        {
            Write-Error -Message "Can't install, module is probably in use."
        }
    }

    Install "$HOME\Documents\WindowsPowerShell\Modules"
    Install "$HOME\Documents\PowerShell\Modules"
}

<#
Synopsis: Publish to PSGallery
#>
task Publish -If ($Configuration -eq 'Release') {

    $binDir = "$PSScriptRoot/bin/Release/PsHosts"

	<#
    # Check signatures before publishing
    Get-ChildItem -Recurse $binDir -Include "*.dll","*.ps*1" | Get-AuthenticodeSignature | ForEach-Object {
        if ($_.Status -ne 'Valid') {
            throw "$($_.Path) is not signed"
        }
        if ($_.SignerCertificate.Subject -notmatch 'CN=Microsoft Corporation.*') {
            throw "$($_.Path) is not signed with a Microsoft signature"
        }
    }

    # Check newlines in signed files before publishing
    Get-ChildItem -Recurse $binDir -Include "*.ps*1" | Get-AuthenticodeSignature | ForEach-Object {
        $lines = (Get-Content $_.Path | Measure-Object).Count
        $fileBytes = [System.IO.File]::ReadAllBytes($_.Path)
        $toMatch = ($fileBytes | ForEach-Object { "{0:X2}" -f $_ }) -join ';'
        $crlf = ([regex]::Matches($toMatch, ";0D;0A") | Measure-Object).Count

        if ($lines -ne $crlf) {
            throw "$($_.Path) appears to have mixed newlines"
        }
    }
	#>

    $manifest = Import-PowerShellDataFile $binDir/PsHosts.psd1

    $version = $manifest.ModuleVersion
    if ($null -ne $manifest.PrivateData)
    {
        $psdata = $manifest.PrivateData['PSData']
        if ($null -ne $psdata)
        {
            $prerelease = $psdata['Prerelease']
            if ($null -ne $prerelease)
            {
                $version = $version + '-' + $prerelease
            }
        }
    }

    $yes = Read-Host "Publish version $version (y/n)"

    if ($yes -ne 'y') { throw "Publish aborted" }

    $nugetApiKey = Read-Host -AsSecureString "Nuget api key for PSGallery"

    $publishParams = @{
        Path = $binDir
        NuGetApiKey = [PSCredential]::new("user", $nugetApiKey).GetNetworkCredential().Password
        Repository = "PSGallery"
        ProjectUri = 'https://github.com/richardszalay/pshosts'
    }

    Publish-Module @publishParams
}

<#
Synopsis: Remove temporary items.
#>
task Clean {
    git clean -fdx
}

<#
Synopsis: Default build rule - build and create module layout
#>
task . LayoutModule, RunTests
