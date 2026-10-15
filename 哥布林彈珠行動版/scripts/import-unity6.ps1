param(
    [string]$Editor = 'C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe',
    [string]$Project = 'E:\NEWTEST\PeglinBrowserLab\Unity6Project',
    [switch]$Inspect,
    [switch]$BuildSmoke
)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$reportDir = Join-Path $taskRoot 'reports\unity6'
if (-not (Test-Path -LiteralPath $Editor)) { throw 'Unity Editor is not installed yet.' }
if (-not (Test-Path -LiteralPath (Join-Path $Project 'ProjectSettings\ProjectVersion.txt'))) { throw 'Working project is missing.' }
if ([IO.Path]::GetFullPath($Project) -ne 'E:\NEWTEST\PeglinBrowserLab\Unity6Project') { throw 'Inspect the working project path before running a different project.' }
New-Item -ItemType Directory -Path $reportDir -Force | Out-Null
$log = Join-Path $reportDir ('editor-import-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.log')
Write-Output "Unity import log: $log"
$unityArguments = @('-batchmode', '-nographics', '-quit', '-accept-apiupdate', '-projectPath', ('"' + $Project + '"'), '-logFile', ('"' + $log + '"'))
if ($Inspect) { $unityArguments += @('-executeMethod', 'BrowserResearchProbe.Inspect') }
if ($BuildSmoke) { $unityArguments += @('-buildTarget', 'WebGL', '-executeMethod', 'BrowserResearchProbe.BuildPlatformSmoke') }
$process = Start-Process -FilePath $Editor -ArgumentList $unityArguments -WindowStyle Hidden -PassThru
$abortedForRepeatedWarnings = $false
while (-not $process.WaitForExit(1000)) {
    if ((Test-Path -LiteralPath $log) -and (Get-Item -LiteralPath $log).Length -gt 100MB) {
        $stream = [IO.File]::Open($log, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite)
        try {
            $null = $stream.Seek([Math]::Max(0, $stream.Length - 32768), [IO.SeekOrigin]::Begin)
            $reader = [IO.StreamReader]::new($stream)
            $tailText = $reader.ReadToEnd()
            if ([regex]::Matches($tailText, 'Do not use ReadObjectThreaded on scene objects!').Count -gt 300) {
                $abortedForRepeatedWarnings = $true
                Stop-Process -Id $process.Id
                $process.WaitForExit()
            }
        }
        finally { $stream.Dispose() }
    }
}
$importExit = $process.ExitCode
$errors = @()
if (Test-Path -LiteralPath $log) {
    $null = & python (Join-Path $PSScriptRoot 'summarize-unity-log.py') $log
    $parsedLog = Get-Content -LiteralPath ([IO.Path]::ChangeExtension($log, '.summary.json')) -Raw | ConvertFrom-Json
    $errors = @($parsedLog.compilerErrors)
}
$summary = [ordered]@{
    editor = $Editor
    project = $Project
    log = $log
    exitCode = $importExit
    compilerOrLicenseErrorLines = $errors.Count
    abortedForRepeatedWarnings = $abortedForRepeatedWarnings
    importOnly = (-not [bool]$BuildSmoke)
    platformBuildTest = [bool]$BuildSmoke
    gamePlayed = $false
    webBuildProduced = (@(Get-ChildItem -LiteralPath 'E:\NEWTEST\PeglinBrowserLab\Builds\PlatformSmoke\Build' -Filter '*.wasm' -ErrorAction SilentlyContinue).Count -gt 0)
}
$summary | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $reportDir 'latest-import.json') -Encoding utf8
$summary | ConvertTo-Json
if ($errors.Count) { $errors | Select-Object -First 15 }
exit $importExit
