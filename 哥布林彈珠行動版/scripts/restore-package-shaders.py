from pathlib import Path
import json, re, tarfile

LAB=Path('E:/NEWTEST/PeglinBrowserLab')
PROJECT=LAB/'Unity6Project'
PKG=Path('C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Data/Resources/PackageManager/BuiltInPackages')
BACKUP=LAB/'RecoveredLibraries/RenderMigration'
tmp_files={}
with tarfile.open(PKG/'com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage','r:gz') as tar:
    members={m.name:m for m in tar.getmembers()}
    for name,m in members.items():
        if not name.endswith('/pathname'): continue
        path=tar.extractfile(m).read().decode().strip()
        if Path(path).suffix not in {'.shader','.cginc','.hlsl'}: continue
        asset=name.rsplit('/',1)[0]+'/asset'
        if asset in members: tmp_files[Path(path).name]=tar.extractfile(members[asset]).read()

shader_sources={}
name_rx=re.compile(r'Shader\s+"([^"]+)"')
for pkgname in ['com.unity.render-pipelines.universal','com.unity.render-pipelines.core','com.unity.shadergraph']:
    for p in (PKG/pkgname).rglob('*.shader'):
        if 'Samples~' in p.parts or 'Tests' in p.parts: continue
        text=p.read_text(encoding='utf-8-sig')
        match=name_rx.search(text)
        if not match: continue
        def inc(m):
            target=p.parent/m[2]
            if target.exists():
                return m[1]+'"Packages/'+target.resolve().relative_to(PKG).as_posix()+'"'
            return m[0]
        text=re.sub(r'(#include(?:_with_pragmas)?\s+)"([^"]+)"',inc,text)
        shader_sources[match[1]]=(text.encode(), 'Packages/'+p.relative_to(PKG).as_posix())
for name,data in tmp_files.items():
    m=name_rx.search(data.decode('utf-8-sig'))
    if m: shader_sources[m[1]]=(data,'TMP Essential Resources/'+name)

changed=[]; remaining=[]
for p in (PROJECT/'Assets').rglob('*.shader'):
    old=p.read_bytes(); m=name_rx.search(old.decode('utf-8-sig'))
    if not m: continue
    if m[1] not in shader_sources:
        remaining.append({'file':str(p.relative_to(PROJECT)), 'name':m[1]}); continue
    data,source=shader_sources[m[1]]
    if old!=data:
        backup=BACKUP/p.relative_to(PROJECT); backup.parent.mkdir(parents=True,exist_ok=True)
        if not backup.exists(): backup.write_bytes(old)
        p.write_bytes(data)
    if m[1].startswith('TextMeshPro/'):
        for filename,incdata in tmp_files.items():
            if Path(filename).suffix in {'.cginc','.hlsl'}:
                out=p.parent/filename
                if out.exists() and out.read_bytes()!=incdata: raise RuntimeError('Conflicting include '+str(out))
                out.write_bytes(incdata)
    changed.append({'file':str(p.relative_to(PROJECT)), 'name':m[1], 'source':source})
(LAB/'Reports/shader-restoration.json').write_text(json.dumps({'restored':changed,'remaining':remaining},indent=2))
print(json.dumps({'restored':len(changed),'remaining':len(remaining),'remainingNames':sorted({s['name'] for s in remaining})}))
