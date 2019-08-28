$ErrorActionPreference = "Stop"

if (-not (Get-Module Pester -ListAvailable))
{
	Install-Module Pester -Scope CurrentUser -SkipPublisherCheck
}

Import-Module ./build/psake/psake.psm1

Invoke-psake ./build/default.ps1