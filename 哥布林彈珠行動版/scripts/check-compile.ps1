param(
    [string]$Project = (Join-Path $PSScriptRoot '..\RecoveredProject\ExportedProject'),
    [string]$Managed = 'E:\SteamLibrary\steamapps\common\Peglin\Peglin_Data\Managed',
    [switch]$ExportedReferences,
    [switch]$Firstpass
)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $taskRoot 'tools\roslyn\tasks\net472\csc.exe'
$reportDir = Join-Path $taskRoot 'reports'
$sourceDir = Join-Path $Project 'Assets\Scripts\Assembly-CSharp'
if ($Firstpass) { $sourceDir = Join-Path $Project 'Assets\Plugins\Assembly-CSharp-firstpass' }
$compileSubdir = if ($Firstpass) { 'compile-firstpass' } elseif ($ExportedReferences) { 'compile-exported' } else { 'compile' }
$assemblyFileName = if ($Firstpass) { 'Assembly-CSharp-firstpass.dll' } else { 'Assembly-CSharp.dll' }
$output = Join-Path $reportDir "$compileSubdir\$assemblyFileName"
New-Item -ItemType Directory -Force -Path (Split-Path $output -Parent) | Out-Null
$sourceFiles = @(Get-ChildItem -LiteralPath $sourceDir -Recurse -File -Filter '*.cs')
$referenceFiles = @(Get-ChildItem -LiteralPath $Managed -File -Filter '*.dll' | Where-Object Name -NE 'Assembly-CSharp.dll')
if ($Firstpass) { $referenceFiles = @($referenceFiles | Where-Object Name -NE 'Assembly-CSharp-firstpass.dll') }
if ($ExportedReferences) {
    $referenceFiles = @(Get-ChildItem -LiteralPath $Managed -File -Filter '*.dll' | Where-Object {
        $_.Name -in @('mscorlib.dll', 'netstandard.dll', 'Mono.Security.dll', 'System.dll') -or
        $_.Name -like 'System.*' -or
        ($_.Name -like 'UnityEngine*' -and $_.Name -notlike 'UnityEngine.Purchasing*' -and $_.Name -ne 'UnityEngine.UI.dll')
    })
    $referenceFiles += Get-ChildItem -LiteralPath (Join-Path $Project 'Assets\Plugins') -File -Filter '*.dll'
    if (-not $Firstpass) {
        $firstpassOutput = Join-Path $reportDir 'compile-firstpass\Assembly-CSharp-firstpass.dll'
        if (-not (Test-Path -LiteralPath $firstpassOutput)) { throw 'Compile the recovered firstpass sources first: check-compile.ps1 -Firstpass -ExportedReferences' }
        $referenceFiles += Get-Item -LiteralPath $firstpassOutput
    }
    $referenceFiles = @($referenceFiles | Group-Object Name | ForEach-Object { $_.Group[-1] })
}
$arguments = @('/nologo', '/target:library', '/nostdlib+', '/unsafe+', '/langversion:9', '/optimize+', ('/out:"' + $output + '"'))
$arguments += $referenceFiles | ForEach-Object { '/reference:"' + $_.FullName + '"' }
$arguments += $sourceFiles | ForEach-Object { '"' + $_.FullName + '"' }
$responseFile = Join-Path $reportDir "$compileSubdir\compile.rsp"
$arguments | Set-Content -LiteralPath $responseFile -Encoding utf8
& $compiler ('@' + $responseFile) *> (Join-Path $reportDir "$compileSubdir\compiler.log")
$compileExit = $LASTEXITCODE
$log = Get-Content -LiteralPath (Join-Path $reportDir "$compileSubdir\compiler.log")
$errors = @($log | Where-Object { $_ -match 'error CS\d+' })
$warnings = @($log | Where-Object { $_ -match 'warning CS\d+' })
$summary = [ordered]@{
    exitCode = $compileExit
    sourceCount = $sourceFiles.Count
    referenceCount = $referenceFiles.Count
    errorCount = $errors.Count
    warningCount = $warnings.Count
    outputExists = (Test-Path -LiteralPath $output)
    exportedReferences = [bool]$ExportedReferences
    firstpass = [bool]$Firstpass
    scope = 'Static C# compilation. Unity engine/framework references are from the Windows player. ExportedReferences mode uses recovered plugin DLLs and a firstpass DLL rebuilt from recovered sources. Not a Unity Editor import, WebGL/AOT build, or runtime test.'
}
$summary | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $reportDir "$compileSubdir\summary.json") -Encoding utf8
$summary | ConvertTo-Json
if ($errors.Count) { $errors | Select-Object -First 12 }
exit $compileExit
