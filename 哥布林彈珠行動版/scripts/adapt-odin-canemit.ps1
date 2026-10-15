# Peglin.OdinSerializer.dll was compiled for the Windows Mono player, where Odin's
# EmitUtilities.CanEmit is the constant `true`. In the WebGL (IL2CPP/AOT) player
# System.Reflection.Emit is unavailable, so formatter emission fails and Odin never takes its
# reflection / weak-formatter fallback paths (they are guarded by !CanEmit). The visible symptom is
# "Was not in array when exiting array" while saving. Odin's own IL2CPP build returns false here,
# so this patch makes the getter return false. Always rebuilt from the pristine backup.
param(
    [string]$Plugin = 'E:\NEWTEST\PeglinBrowserLab\Unity6Project\Assets\Plugins\Peglin.OdinSerializer.dll',
    [string]$Backup = 'E:\NEWTEST\PeglinBrowserLab\RecoveredLibraries\Originals\Peglin.OdinSerializer.dll'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
[Reflection.Assembly]::LoadFrom((Join-Path $root 'tools\mono-cecil\lib\net40\Mono.Cecil.dll')) | Out-Null
if (-not (Test-Path $Backup)) { Copy-Item $Plugin $Backup }
$rp = New-Object Mono.Cecil.ReaderParameters
$rp.InMemory = $true
$mod = [Mono.Cecil.ModuleDefinition]::ReadModule($Backup, $rp)
$type = $mod.GetType('Peglin.OdinSerializer.Utilities.EmitUtilities')
$getter = $type.Methods | Where-Object { $_.Name -eq 'get_CanEmit' }
if (-not $getter) { throw 'EmitUtilities.get_CanEmit not found' }
$il = $getter.Body.GetILProcessor()
$getter.Body.Instructions.Clear()
$getter.Body.Variables.Clear()
$getter.Body.ExceptionHandlers.Clear()
$il.Append($il.Create([Mono.Cecil.Cil.OpCodes]::Ldc_I4_0))
$il.Append($il.Create([Mono.Cecil.Cil.OpCodes]::Ret))
$tmp = "$Plugin.adapting"
$mod.Write($tmp)
$mod.Dispose()
$check = [Mono.Cecil.ModuleDefinition]::ReadModule($tmp)
$g = ($check.GetType('Peglin.OdinSerializer.Utilities.EmitUtilities').Methods | Where-Object { $_.Name -eq 'get_CanEmit' }).Body.Instructions
"get_CanEmit now: " + (($g | ForEach-Object { $_.OpCode.Name }) -join ' ')
$check.Dispose()
Copy-Item $tmp $Plugin -Force
Remove-Item -LiteralPath $tmp -Force
Add-Content -LiteralPath (Join-Path $root 'reports\unity6\engine-module-adaptation.txt') 'Peglin.OdinSerializer.dll: EmitUtilities.CanEmit -> false (IL2CPP has no Reflection.Emit), scripts/adapt-odin-canemit.ps1'
