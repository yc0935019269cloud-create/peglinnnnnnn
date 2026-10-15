param(
    [string]$Source = 'E:\SteamLibrary\steamapps\common\Peglin',
    [string]$Destination = (Join-Path (Split-Path $PSScriptRoot -Parent) ('RecoveredProject-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [int]$Port = 18771
)
$ErrorActionPreference = 'Stop'
$taskRoot = [IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$targetPath = [IO.Path]::GetFullPath($Destination)
if (-not $targetPath.StartsWith($taskRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'The export destination must be a new child directory of this project.'
}
if (Test-Path -LiteralPath $targetPath) { throw 'The destination already exists. Use a fresh directory; exports must not overwrite an earlier recovery.' }
if (-not (Test-Path -LiteralPath (Join-Path $Source 'Peglin_Data\Managed\Assembly-CSharp.dll'))) { throw 'Peglin Mono game files not found at Source.' }
$tool = Join-Path $taskRoot 'tools\AssetRipper\AssetRipper.GUI.Free.exe'
$base = "http://127.0.0.1:$Port"
$createdProcess = $null
try {
    try { $null = Invoke-WebRequest "$base/" -TimeoutSec 2 }
    catch {
        $createdProcess = Start-Process -FilePath $tool -ArgumentList '--headless','--port',"$Port",'--log-path',('"' + (Join-Path $taskRoot 'reports\assetripper-rerun.log') + '"') -WorkingDirectory (Split-Path $tool -Parent) -WindowStyle Hidden -PassThru
        $ready = $false
        for ($attempt = 0; $attempt -lt 20; $attempt++) {
            try { $null = Invoke-WebRequest "$base/" -TimeoutSec 1; $ready = $true; break }
            catch { Start-Sleep -Milliseconds 500 }
        }
        if (-not $ready) { throw 'AssetRipper did not start.' }
    }
    # These are the documented routes in AssetRipper 2.0.0's WebApplicationLauncher.
    $null = Invoke-WebRequest "$base/LoadFolder" -Method Post -Body @{Path=$Source} -TimeoutSec 900
    $null = Invoke-WebRequest "$base/Export/UnityProject" -Method Post -Body @{Path=$targetPath;CreateSubfolder='false'} -TimeoutSec 1800
    if (-not (Test-Path -LiteralPath (Join-Path $targetPath 'ExportedProject\ProjectSettings\ProjectVersion.txt'))) {
        throw 'No exported Unity project was produced. Check AssetRipper logs.'
    }
    Write-Output "Recovered project: $targetPath\ExportedProject"
}
finally {
    if ($createdProcess -and -not $createdProcess.HasExited) { Stop-Process -Id $createdProcess.Id }
}
