$psversionTable | out-string | write-host
Register-PSRepository pshosts -InstallationPolicy Trusted -SourceLocation $env:PS_GALLERY_SOURCE
Install-Module PsHosts -Scope CurrentUser -Repository pshosts
Import-Module PsHosts
$res = Invoke-Pester -PassThru
if ($res.FailedCount -gt 0) { 
    throw "$($res.FailedCount) tests failed."
}