"""Rebind I2 localization targets at runtime instead of illegal asset-to-scene links."""
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PROJECT = Path('E:/NEWTEST/PeglinBrowserLab/Unity6Project')
ASSETS = PROJECT / 'Assets'
guid_pattern = re.compile(r'^guid:\s*([0-9a-f]{32})', re.M)
scene_guids = set()
for path in (ASSETS / 'Scenes').glob('*.unity.meta'):
    scene_guids.add(guid_pattern.search(path.read_text(encoding='utf-8-sig'))[1])
target_scripts = {}
for path in (ASSETS / 'Scripts/Assembly-CSharp/I2/Loc').glob('LocalizeTarget_*.cs.meta'):
    target_scripts[guid_pattern.search(path.read_text(encoding='utf-8-sig'))[1]] = path.name
changes = []
for path in (ASSETS / 'MonoBehaviour').glob('*.asset'):
    before = path.read_bytes()
    text = before.decode('utf-8-sig')
    script = re.search(r'm_Script:.*guid: ([0-9a-f]{32})', text)
    target = re.search(r'(?m)^  mTarget: \{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: \d+\}', text)
    if not script or script[1] not in target_scripts or not target or target[2] not in scene_guids:
        continue
    fixed = text[:target.start()] + '  mTarget: {fileID: 0}' + text[target.end():]
    path.write_text(fixed, encoding='utf-8')
    changes.append({
        'file': path.relative_to(PROJECT).as_posix(),
        'script': target_scripts[script[1]],
        'originalTargetSceneGuid': target[2],
        'originalTargetFileID': target[1],
        'beforeSHA256': hashlib.sha256(before).hexdigest(),
        'afterSHA256': hashlib.sha256(path.read_bytes()).hexdigest(),
        'reason': 'A standalone ScriptableObject asset cannot persist a scene component reference. I2 LocalizeTarget<T>.IsValid already rebinds a null mTarget via cmp.GetComponent<T>(); preserve the remaining localization settings.',
    })
(ROOT / 'reports/unity6/localization-scene-links.json').write_text(json.dumps(changes, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps({'fixedLocalizationTargets': len(changes)}, ensure_ascii=False))
