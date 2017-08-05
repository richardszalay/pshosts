$psversionTable | out-string | write-host
Register-PSRepository pshosts -InstallationPolicy Trusted -SourceLocation $env:PS_GALLERY_SOURCE
Install-Module PsHosts -Scope CurrentUser -Repository pshosts
Import-Module PsHosts
Invoke-Pester
