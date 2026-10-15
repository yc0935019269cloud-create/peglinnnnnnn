# Runs a static Editor method in the Unity 6 working copy (batchmode) and prints a bounded summary.
# Usage: run-unity-method.ps1 -Method RebuildAddressablesFromCatalog.Dump [-WebGL]
param(
    [Parameter(Mandatory = $true)][string]$Method,
    [switch]$WebGL,
    [string]$Editor = 'C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe',
    [string]$Project = 'E:\NEWTEST\PeglinBrowserLab\Unity6Project'
)
$ErrorActionPreference = 'Stop'
$reportDir = Join-Path (Split-Path $PSScriptRoot -Parent) 'reports\unity6'
$log = Join-Path $reportDir ('method-' + ($Method -replace '[^A-Za-z0-9]', '_') + '-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.log')
$args2 = @('-batchmode', '-nographics', '-quit', '-accept-apiupdate', '-projectPath', ('"' + $Project + '"'), '-logFile', ('"' + $log + '"'), '-executeMethod', $Method)
if ($WebGL) { $args2 += @('-buildTarget', 'WebGL') }
$p = Start-Process -FilePath $Editor -ArgumentList $args2 -WindowStyle Hidden -PassThru
$p.WaitForExit()
"exit=$($p.ExitCode) log=$log"
Select-String -Path $log -CaseSensitive -Pattern 'error CS|Exception:|_DONE\b|Error:' | Select-Object -Last 15 | ForEach-Object { $_.Line.Trim() }
exit $p.ExitCode
