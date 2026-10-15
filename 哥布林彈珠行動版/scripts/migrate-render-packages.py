"""Migrate only the mutable research project; keep byte-for-byte backups and a change ledger."""
from pathlib import Path
import json, re, shutil

LAB = Path('E:/NEWTEST/PeglinBrowserLab')
PROJECT = LAB / 'Unity6Project'
PACKAGES = Path('C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Data/Resources/PackageManager/BuiltInPackages')
BACKUP = LAB / 'RecoveredLibraries/RenderMigration'
REPORT = LAB / 'Reports/render-package-migration.json'
changes = []
if json.loads((PROJECT/'Packages/manifest.json').read_text())['dependencies'].get('com.unity.render-pipelines.universal') == '17.6.0' and REPORT.exists():
    print('Render package migration already applied; preserving the original report and backups.')
    raise SystemExit(0)

def write(path, data, reason):
    old = path.read_bytes()
    if old == data: return
    dst = BACKUP / path.relative_to(PROJECT)
    dst.parent.mkdir(parents=True, exist_ok=True)
    if not dst.exists(): dst.write_bytes(old)
    path.write_bytes(data)
    changes.append({'file':path.relative_to(PROJECT).as_posix(), 'reason':reason})

scripts = json.loads((LAB / 'Reports/render-old-scripts.json').read_text())['scripts']
sources = list((PACKAGES/'com.unity.render-pipelines.core').rglob('*.cs')) + list((PACKAGES/'com.unity.render-pipelines.universal').rglob('*.cs'))
deprecated = list((PROJECT/'Assets').rglob('DebugUI*.prefab'))
deprecated_guids = {re.search(r'^guid: (\w+)', Path(str(p)+'.meta').read_text(), re.M)[1] for p in deprecated}
mapping = {}
for s in scripts:
    name = s['type'].split('.')[-1]
    candidates = [p for p in sources if p.stem == name and Path(str(p)+'.meta').exists() and 'Editor' not in p.relative_to(PACKAGES).parts and 'Tests' not in p.relative_to(PACKAGES).parts]
    if len(candidates) == 1:
        guid = re.search(r'^guid: (\w+)', Path(str(candidates[0])+'.meta').read_text(), re.M)[1]
        mapping[(s['guid'], str(s['fileID']))] = (guid, '11500000', s['type'])

used, unresolved = {}, {}
ref = re.compile(r'\{fileID: (-?\d+), guid: (\w+), type: 3\}')
old_guids = {s['guid'] for s in scripts}
for p in (PROJECT/'Assets').rglob('*'):
    if p.suffix not in {'.asset','.unity','.prefab','.mat'}: continue
    b = p.read_bytes()
    if not b.startswith(b'%YAML'): continue
    if p in deprecated: continue
    if set(re.findall(r'guid: (\w+)', b.decode('utf-8'))) & deprecated_guids:
        raise RuntimeError('Game asset references obsolete debug UI: '+str(p))
    def replace(m):
        k = (m[2],m[1])
        if k in mapping:
            g,f,t = mapping[k]
            used[t] = used.get(t,0)+1
            return '{fileID: '+f+', guid: '+g+', type: 3}'
        if m[2] in old_guids: unresolved[str(k)] = unresolved.get(str(k),0)+1
        return m[0]
    text = ref.sub(replace, b.decode('utf-8'))
    write(p,text.encode('utf-8'),'DLL script reference to package source script')

if unresolved:
    raise RuntimeError('Unresolved old rendering script references: '+str(unresolved))

# Old SRP runtime-debug UI was removed upstream; no game asset references this closed prefab set.
for p in [q for p in deprecated for q in (p,Path(str(p)+'.meta'))]:
    dst = BACKUP / p.relative_to(PROJECT)
    assert p.resolve().is_relative_to((PROJECT/'Assets').resolve()) and dst.resolve().is_relative_to(BACKUP.resolve())
    dst.parent.mkdir(parents=True,exist_ok=True)
    if dst.exists(): raise RuntimeError('Backup already exists: '+str(dst))
    shutil.move(str(p),str(dst))
    changes.append({'file':p.relative_to(PROJECT).as_posix(),'reason':'Quarantine obsolete unreferenced SRP debug prefab'})

for p in sorted((PROJECT/'Assets/Plugins').glob('Unity.RenderPipeline*.dll*')):
    dst = BACKUP / p.relative_to(PROJECT)
    assert p.resolve().is_relative_to((PROJECT/'Assets/Plugins').resolve()) and dst.resolve().is_relative_to(BACKUP.resolve())
    dst.parent.mkdir(parents=True,exist_ok=True)
    if dst.exists(): raise RuntimeError('Backup already exists: '+str(dst))
    shutil.move(str(p),str(dst))
    changes.append({'file':p.relative_to(PROJECT).as_posix(),'reason':'Remove old render DLL in favor of official package'})

p = PROJECT/'Packages/manifest.json'
manifest = json.loads(p.read_text())
manifest['dependencies']['com.unity.render-pipelines.universal'] = '17.6.0'
write(p,(json.dumps(manifest,indent=2)+'\n').encode(),'Use editor bundled URP 17.6.0')
p = PROJECT/'Assets/Scripts/Assembly-CSharp/PeglinUI/OrbDisplay/OrbInfoWidget.cs'
write(p,p.read_bytes().replace(b'UnityEngine.Experimental.Rendering.Universal',b'UnityEngine.Rendering.Universal'),'PixelPerfectCamera namespace moved in URP')
REPORT.write_text(json.dumps({'mappedTypes':used,'unresolved':unresolved,'changes':changes},indent=2))
print(json.dumps({'changedFiles':len(changes),'mappedTypes':used,'unresolved':unresolved}))
