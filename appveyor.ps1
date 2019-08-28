$ErrorActionPreference = "Stop"

$pwshRelease = $env:PWSH_RELEASE

if ($pwshRelease -eq "native")
{
	$ps = "powershell"
}
else
{
	if ($IsLinux)
	{
		$previewArg = if ($pwshRelease -eq "preview") { "-preview" } else { "" }
		(curl -L -s https://aka.ms/install-powershell.sh) | sudo bash -s -- $previewArg
		$ps = if ($pwshRelease -eq "preview") { "pwsh-preview" } else { "pwsh" }
	}
	else
	{
		$previewArg = if ($pwshRelease -eq "preview") { "-Preview" } else { "" }
		iex "& { $(irm https://aka.ms/install-powershell.ps1) } -AddToPath $previewArg"
		$ps = "pwsh"
	}
}

& $ps ".\build.ps1"
