# Rewired (obfuscated, Mono-era) trips Unity 6 IL2CPP's VTable builder:
#   "The base method Base::M() is implemented both by T::M() and T::Base.get_M()"
# Obfuscated type T (derived from Base) has an explicit MethodImpl B (T::Base.get_M -> Base::M) AND
# a same-named virtual A (T::M, not newslot) that overrides Base::M implicitly by name. Mono lets the
# explicit MethodImpl win and A never occupies the slot. IL2CPP counts both and fails.
# Fix: keep B untouched (Mono semantics) and give A its own slot (newslot) so it no longer overrides
# Base::M by name. Only done when nothing in the three Rewired DLLs calls A, so behaviour is unchanged.
# Conflicts with call sites are reported and left alone.
#
# Usage: adapt-rewired-vtable.ps1            (dry run, prints findings)
#        adapt-rewired-vtable.ps1 -Apply     (writes adapted DLLs, always from the pristine backup)
param(
    [string]$PluginDir = 'E:\NEWTEST\PeglinBrowserLab\Unity6Project\Assets\Plugins',
    [string]$BackupDir = 'E:\NEWTEST\PeglinBrowserLab\RecoveredLibraries\Originals\Rewired',
    [string[]]$Dlls = @('Rewired_Core.dll', 'Rewired_Windows.dll', 'Rewired_Windows_Functions.dll'),
    [string]$Report = 'E:\NEWTEST\哥布林彈珠行動版\reports\unity6\rewired-vtable-adaptation.txt',
    [string]$Cecil = 'E:\NEWTEST\哥布林彈珠行動版\tools\mono-cecil\lib\net40\Mono.Cecil.dll',
    [switch]$Apply
)
$ErrorActionPreference = 'Stop'
[Reflection.Assembly]::LoadFrom($Cecil) | Out-Null
$resolver = New-Object Mono.Cecil.DefaultAssemblyResolver
$resolver.AddSearchDirectory($BackupDir)
$resolver.AddSearchDirectory($PluginDir)
$resolver.AddSearchDirectory('C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Data\MonoBleedingEdge\lib\mono\unityaot-linux')
function SigKey($m) {
    $ps = ($m.Parameters | ForEach-Object { $_.ParameterType.FullName }) -join ','
    "$($m.Name)($ps)->$($m.ReturnType.FullName)"
}
if ($Apply) { New-Item -ItemType Directory -Force $BackupDir | Out-Null }
$mods = @{}
foreach ($dll in $Dlls) {
    $path = Join-Path $PluginDir $dll
    $backup = Join-Path $BackupDir $dll
    if (-not (Test-Path $backup)) { Copy-Item $path $backup }   # pristine copy, never overwritten
    $rp = New-Object Mono.Cecil.ReaderParameters
    $rp.AssemblyResolver = $resolver
    $rp.InMemory = $true
    $mods[$dll] = [Mono.Cecil.ModuleDefinition]::ReadModule($backup, $rp)
}
# call-site index: "DeclaringType::Name" keys referenced by any instruction in any module
$called = New-Object 'System.Collections.Generic.HashSet[string]'
$callOps = @{}   # key -> hashtable of opcode names
foreach ($mod in $mods.Values) {
    foreach ($t in $mod.GetTypes()) {
        foreach ($m in $t.Methods) {
            if (-not $m.HasBody) { continue }
            foreach ($ins in $m.Body.Instructions) {
                $op = $ins.Operand
                if ($op -is [Mono.Cecil.MethodReference]) {
                    $k = "$($op.DeclaringType.FullName)::$($op.Name)"
                    [void]$called.Add($k)
                    if (-not $callOps.ContainsKey($k)) { $callOps[$k] = @{} }
                    $callOps[$k][$ins.OpCode.Name] = 1 + [int]$callOps[$k][$ins.OpCode.Name]
                }
            }
        }
    }
}
$lines = New-Object System.Collections.Generic.List[string]
$total = @{}
$redirects = New-Object System.Collections.Generic.List[object]   # A -> B call-site retargeting
foreach ($dll in $Dlls) {
    $mod = $mods[$dll]
    $fixed = 0; $kept = 0
    foreach ($t in $mod.GetTypes()) {
        foreach ($b in @($t.Methods | Where-Object { $_.HasOverrides })) {
            foreach ($ov in $b.Overrides) {
                try { $decl = $ov.DeclaringType.Resolve() } catch { $decl = $null }   # e.g. legacy UnityEngine.dll
                if ($null -eq $decl -or $decl.IsInterface) { continue }      # base-class MethodImpl only
                $key = SigKey $ov
                foreach ($a in @($t.Methods | Where-Object { $_ -ne $b -and $_.IsVirtual -and -not $_.IsNewSlot -and -not $_.HasOverrides -and (SigKey $_) -eq $key })) {
                    $k = "$($t.FullName)::$($a.Name)"
                    $ops = @()
                    if ($callOps.ContainsKey($k)) { $ops = @($callOps[$k].Keys) }
                    if ($ops | Where-Object { $_ -ne 'callvirt' }) {
                        $kept++; $lines.Add("KEEP (non-callvirt call sites $($ops -join ',')): $dll $k"); continue
                    }
                    # Mono dispatches callvirt A through the slot, where the explicit MethodImpl B wins.
                    $redirects.Add([pscustomobject]@{ A = $a; B = $b; Type = $t })
                    $a.IsNewSlot = $true
                    $fixed++; $lines.Add("newslot + callvirt redirected to explicit impl: $dll $k ($($ops.Count) site kinds)")
                }
            }
        }
    }
    $lines.Add("$dll newslot=$fixed kept=$kept")
    $total[$dll] = $fixed
}
foreach ($mod in $mods.Values) {
    foreach ($t in $mod.GetTypes()) {
        foreach ($m in $t.Methods) {
            if (-not $m.HasBody) { continue }
            foreach ($ins in $m.Body.Instructions) {
                $op = $ins.Operand
                if ($op -isnot [Mono.Cecil.MethodReference]) { continue }
                foreach ($r in $redirects) {
                    if ($op.Name -eq $r.A.Name -and $op.DeclaringType.FullName -eq $r.Type.FullName -and $op.Parameters.Count -eq $r.A.Parameters.Count) {
                        $ins.Operand = $mod.ImportReference($r.B)
                        break
                    }
                }
            }
        }
    }
}
if ($Apply) {
    foreach ($dll in $Dlls) {
        if ($total[$dll] -eq 0) { Copy-Item (Join-Path $BackupDir $dll) (Join-Path $PluginDir $dll) -Force; continue }
        $tmp = (Join-Path $PluginDir $dll) + '.adapting'
        $mods[$dll].Write($tmp)
        $check = [Mono.Cecil.ModuleDefinition]::ReadModule($tmp)
        if ($check.Assembly.Name.FullName -ne $mods[$dll].Assembly.Name.FullName) { throw "identity changed for $dll" }
        $check.Dispose()
        Copy-Item $tmp (Join-Path $PluginDir $dll) -Force
        Remove-Item -LiteralPath $tmp -Force
    }
    $lines | Set-Content -LiteralPath $Report -Encoding utf8
}
foreach ($mod in $mods.Values) { $mod.Dispose() }
$lines | ForEach-Object { $_ }
