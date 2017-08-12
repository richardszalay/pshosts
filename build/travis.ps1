$psversionTable | out-string | write-host

$ErrorActionPreference = "Stop"

function Find-AppveyorBuild {
    param($Commit, $RepositoryName, $TimeoutSeconds)

    $result = ConvertFrom-Json (Invoke-WebRequest "https://ci.appveyor.com/api/projects/$RepositoryName/history?recordsNumber=50").Content

    $build = $result.builds | Where-Object { $_.commitId -eq $Commit } | Select-Object -First 1

    if ($build) {
        return $build
    }

    $lastBuild = $result.builds | Select-Object -Last 1

    while ($lastBuild -and (-not $build)) {
        Start-Sleep -Seconds 2

        $result = ConvertFrom-Json (Invoke-WebRequest "https://ci.appveyor.com/api/projects/$RepositoryName/history?recordsNumber=50&startBuildId=$($lastBuild.buildId)").Content
        $build = $result.builds | Where-Object { $_.commitId -eq $Commit } | Select-Object -First 1

        if ($build) {
            return $build
        }

        $lastBuild = $result.builds | Select-Object -Last 1
    }
    
    Write-Host "Unable to locate Appveyor build. Waiting $TimeoutSeconds for it to start"

    $timeoutMilliseconds = $TimeoutSeconds * 1000

    $sw = [system.diagnostics.stopwatch]::StartNew()
    while (($TimeoutSeconds -eq 0) -or ($sw.ElapsedMilliseconds -lt $timeoutMilliseconds)) {
        Start-Sleep -Seconds 2

        $result = ConvertFrom-Json (Invoke-WebRequest "https://ci.appveyor.com/api/projects/$RepositoryName").Content

        if ($result.build -and ($result.build.commitId -eq $Commit)) {
            return $result.build
        }
    }

    throw "Unable to locate Appveyor build for commit $Commit"
}

function Wait-AppveyorBuild {
    param($Build, $TimeoutSeconds)

    $repositoryName = $build.project.repositoryName

    $status = $Build.status

    if ($status -ne "success" -and $status -ne "failed" -and $status -ne "cancelled") {
        while ($status -eq "queued") {
            Start-Sleep -Seconds 2

            $result = ConvertFrom-Json (Invoke-WebRequest "https://ci.appveyor.com/api/projects/$repositoryName/build/$($Build.version)").Content
            $status = $result.build.status
        }

        Write-Host "Waiting for Appveyor build to complete"

        $sw = [system.diagnostics.stopwatch]::StartNew()
        $timeoutMilliseconds = $TimeoutSeconds * 1000

        while ($status -ne "success" -and $status -ne "failed" -and $status -ne "cancelled") {
            Start-Sleep -Seconds 2

            if ($sw.ElapsedMilliseconds -gt $timeoutMilliseconds) {
                throw "Timed out waiting for Appveyor build to complete"
            }

            $result = ConvertFrom-Json (Invoke-WebRequest "https://ci.appveyor.com/api/projects/$repositoryName/build/$($Build.version)").Content
            $status = $result.build.status
        }
    }

    if ($status -ne "success") {
        throw "Appveyor build $status"
    }
}

if ($env:APPVEYOR_REPOSITORY_NAME -and $env:TRAVIS_COMMIT) {
    $appveyorBuild = Find-AppveyorBuild -RepositoryName $env:APPVEYOR_REPOSITORY_NAME -Commit $env:TRAVIS_COMMIT -TimeoutSeconds 20

    Wait-AppveyorBuild -Build $appveyorBuild -TimeoutSeconds 120

    Write-Hose "Found Appveyor build $(appveyorBuild.version) for commit $($env:TRAVIS_COMMIT)"

    $moduleVersion = [string]$appveyorBuild.version
}

Register-PSRepository pshosts -InstallationPolicy Trusted -SourceLocation $env:PS_GALLERY_SOURCE

Install-Module PsHosts -Scope CurrentUser -Repository pshosts -MinimumVersion $moduleVersion -MaximumVersion $moduleVersion
Import-Module PsHosts
$res = Invoke-Pester -PassThru
if ($res.FailedCount -gt 0) { 
    throw "$($res.FailedCount) tests failed."
}