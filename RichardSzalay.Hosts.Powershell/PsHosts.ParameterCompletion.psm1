$complete_HostName = {
	param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)

	Get-HostEntry "$wordToComplete*" | %{
		New-Object System.Management.Automation.CompletionResult $_.Name, $_.Name, 'ParameterValue', ('{0} ({1})' -f $_.DisplayName, $_.Status)
	}
}

if (-not $global:options) { $global:options = @{CustomArgumentCompleters = @{};NativeArgumentCompleters = @{}}}
$global:options['CustomArgumentCompleters']['Get-HostEntry:Name'] = $complete_HostName
$global:options['CustomArgumentCompleters']['Set-HostEntry:Name'] = $complete_HostName
$global:options['CustomArgumentCompleters']['Disable-HostEntry:Name'] = $complete_HostName
$global:options['CustomArgumentCompleters']['Enable-HostEntry:Name'] = $complete_HostName
$global:options['CustomArgumentCompleters']['Remove-HostEntry:Name'] = $complete_HostName

# Default expansion uses local $options, so we need to inject in the merging of our global options
# Hacky, but apparently the only way
# http://www.powertheshell.com/dynamicargumentcompletion/
if (-not ([string]$function:TabExpansion2 -like "*`$global:options*"))
{
	$function:tabexpansion2 = $function:tabexpansion2 -replace 'End\r\n{','End { if ($null -ne $options) { $options += $global:options} else {$options = $global:options}'
}
