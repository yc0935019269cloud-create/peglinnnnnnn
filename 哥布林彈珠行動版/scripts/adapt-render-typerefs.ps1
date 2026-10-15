param([switch]$Apply)
$ErrorActionPreference = 'Stop'
$lab = 'E:\NEWTEST\PeglinBrowserLab'
$project = Join-Path $lab 'Unity6Project'
[Reflection.Assembly]::LoadFrom((Join-Path (Split-Path $PSScriptRoot -Parent) 'tools\mono-cecil\lib\net40\Mono.Cecil.dll')) | Out-Null
$resolver = New-Object Mono.Cecil.DefaultAssemblyResolver
foreach ($dir in @("$project\Library\ScriptAssemblies", "$project\Assets\Plugins", 'C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Data\Managed\UnityEngine', 'C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Data\MonoBleedingEdge\lib\mono\unityaot-win32')) { $resolver.AddSearchDirectory($dir) }
$rp = New-Object Mono.Cecil.ReaderParameters
$rp.InMemory = $true
$rp.AssemblyResolver = $resolver
$types = @{}
foreach ($file in Get-ChildItem "$project\Library\ScriptAssemblies\Unity.RenderPipeline*.dll") {
    if ($file.Name -match 'Editor') { continue }
    $m = [Mono.Cecil.ModuleDefinition]::ReadModule($file.FullName,$rp)
    foreach ($t in $m.GetTypes()) { $types[$t.FullName] = $m.Assembly.Name.FullName }
    $m.Dispose()
}
$changes = @()
foreach ($file in Get-ChildItem "$project\Assets\Plugins\*.dll") {
    $m = [Mono.Cecil.ModuleDefinition]::ReadModule($file.FullName,$rp)
    $changed = $false
    # System.Type values in attributes are serialized assembly-qualified strings,
    # and therefore are not returned by GetTypeReferences(). Decode them as well.
    $references = [System.Collections.Generic.List[Mono.Cecil.TypeReference]]::new()
    foreach ($t in $m.GetTypeReferences()) { $references.Add($t) }
    foreach ($definition in $m.GetTypes()) {
        foreach ($attribute in $definition.CustomAttributes) {
            foreach ($argument in $attribute.ConstructorArguments) {
                if ($argument.Value -is [Mono.Cecil.TypeReference]) { $references.Add($argument.Value) }
            }
        }
    }
    foreach ($t in $references) {
        if ($t.Scope.Name -notlike 'Unity.RenderPipeline*' -or -not $types.ContainsKey($t.FullName)) { continue }
        $identity = [Mono.Cecil.AssemblyNameReference]::Parse($types[$t.FullName])
        if ($t.Scope.Name -eq $identity.Name) { continue }
        $changes += "$($file.Name): $($t.FullName): $($t.Scope.Name) -> $($identity.Name)"
        $existing = $m.AssemblyReferences | Where-Object Name -eq $identity.Name | Select-Object -First 1
        if (-not $existing) { $m.AssemblyReferences.Add($identity); $existing = $identity }
        $t.Scope = $existing
        $changed = $true
    }
    if ($Apply -and $changed) {
        $backup = Join-Path "$lab\RecoveredLibraries\RenderMigration\Assets\Plugins" $file.Name
        if (-not (Test-Path $backup)) { Copy-Item -LiteralPath $file.FullName -Destination $backup }
        $tmp = $file.FullName + '.adapting'
        $m.Write($tmp)
        $test = [Mono.Cecil.ModuleDefinition]::ReadModule($tmp,$rp)
        if ($test.Assembly.Name.FullName -ne $m.Assembly.Name.FullName) { throw 'Assembly identity mismatch' }
        $test.Dispose()
        Copy-Item -LiteralPath $tmp -Destination $file.FullName -Force
        Remove-Item -LiteralPath $tmp
    }
    $m.Dispose()
}
$changes | Set-Content "$lab\Reports\render-typeref-adaptation.txt"
$changes
