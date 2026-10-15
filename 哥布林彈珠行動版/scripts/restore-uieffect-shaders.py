from pathlib import Path
import urllib.request,json,re
LAB=Path('E:/NEWTEST/PeglinBrowserLab')
PROJECT=LAB/'Unity6Project'
COMMIT='332340e7bc2f8257816dd28784607a1edf6b6c2a'
BASE='https://raw.githubusercontent.com/mob-sakai/UIEffect/'+COMMIT+'/'
DEST=LAB/'RecoveredLibraries/UIEffect-upm-source'
DEST.mkdir(parents=True,exist_ok=True)
names=['UIDissolve.shader','UIEffect.cginc','UIEffect.shader','UIEffectSprite.cginc','UIHsvModifier.shader','UIShiny.shader','UITtransition.shader']
for name in names+['LICENSE.md','package.json']:
    remote=('Resources/' if name in names else '')+name
    out=DEST/name
    if not out.exists(): out.write_bytes(urllib.request.urlopen(BASE+remote).read())
sources={}
for p in DEST.glob('*.shader'):
    text=p.read_text(encoding='utf-8-sig')
    sources[re.search(r'Shader\s+"([^"]+)"',text)[1]]=p
changes=[]
for p in (PROJECT/'Assets').rglob('*.shader'):
    data=p.read_bytes(); text=data.decode('utf-8-sig'); m=re.search(r'Shader\s+"([^"]+)"',text)
    if not m or m[1] not in sources: continue
    source=sources[m[1]]
    oldprops=set(re.findall(r'^\s*(?:\[[^\]]*\]\s*)*(\w+)\s*\(',text,re.M))
    newprops=set(re.findall(r'^\s*(?:\[[^\]]*\]\s*)*(\w+)\s*\(',source.read_text(encoding='utf-8-sig'),re.M))
    backup=LAB/'RecoveredLibraries/RenderMigration'/p.relative_to(PROJECT)
    backup.parent.mkdir(parents=True,exist_ok=True)
    if not backup.exists(): backup.write_bytes(data)
    p.write_bytes(source.read_bytes())
    for inc in DEST.glob('*.cginc'): (p.parent/inc.name).write_bytes(inc.read_bytes())
    changes.append({'file':str(p.relative_to(PROJECT)),'source':BASE+'Resources/'+source.name,'missingOldProperties':sorted(oldprops-newprops)})
(LAB/'Reports/uieffect-shader-restoration.json').write_text(json.dumps({'commit':COMMIT,'package':json.loads((DEST/'package.json').read_text()),'changes':changes},indent=2))
print(json.dumps(changes))
