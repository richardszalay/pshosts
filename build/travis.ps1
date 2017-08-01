Register-PSRepository pshosts -SourceLocation $env:PS_GALLERY_SOURCE
Install-Module PsHosts -Scope CurrentUser -Repository pshosts
Import-Module PsHosts
Invoke-Pester
