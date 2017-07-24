$complete_HostName = {
	param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)

	Get-HostEntry "$wordToComplete*" | %{
		New-Object System.Management.Automation.CompletionResult $_.Name, $_.Name, 'ParameterValue', ('{0} ({1})' -f $_.DisplayName, $_.Status)
	}
}

$cmdletsToRegister = @(
	"Get-HostEntry", "Set-HostEntry", "Disable-HostEntry", 
	"Enable-HostEntry", "Remove-HostEntry", "Test-HostEntry"
)

$parameterToComplete = "Name"

$registerCmdlet = Get-Command "Register-ArgumentCompleter" -ErrorAction SilentlyContinue

# Prefer PS5.1 / PSCore cmdlet
if ($registerCmdlet)
{
	$cmdletsToRegister | ForEach-Object {
		Register-ArgumentCompleter -CommandName $_ -ParameterName $parameterToComplete -ScriptBlock $complete_HostName
	}
}
else
{
	# Hacky fallback, but compatible down to PS3
	# http://www.powertheshell.com/dynamicargumentcompletion/

	if (-not $global:options) { $global:options = @{CustomArgumentCompleters = @{};NativeArgumentCompleters = @{}}}

	$cmdletsToRegister | ForEach-Object {
		$key = "$($_):$parameterToComplete"
		$global:options['CustomArgumentCompleters'][$key] = $complete_HostName
	}

	if (-not ([string]$function:TabExpansion2 -like "*`$global:options*"))
	{
		$function:tabexpansion2 = $function:tabexpansion2 -replace 'End\r\n{','End { if ($null -ne $options) { $options += $global:options} else {$options = $global:options}'
	}	
}
