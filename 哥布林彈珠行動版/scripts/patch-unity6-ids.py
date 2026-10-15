import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PROJECT = Path('E:/NEWTEST/PeglinBrowserLab/Unity6Project')
files = [p for p in (PROJECT / 'Assets').rglob('*.cs') if 'GetInstanceID()' in p.read_text(encoding='utf-8-sig')]
changes = []
for path in files:
    before = path.read_bytes()
    text = before.decode('utf-8-sig')
    fixed = text.replace('(int)Mathf.Sign(x.GetInstanceID() - y.GetInstanceID())', 'x.GetEntityId().CompareTo(y.GetEntityId())')
    fixed = fixed.replace('.GetInstanceID()', '.GetEntityId()')
    if path.name == 'DamageCountDisplay.cs':
        fixed = fixed.replace('Dictionary<int, TextDisplay>', 'Dictionary<EntityId, TextDisplay>')
        fixed = fixed.replace('Dictionary<int, int>', 'Dictionary<EntityId, int>')
        fixed = fixed.replace('int num = -1;', 'EntityId num = EntityId.None;')
        fixed = fixed.replace('foreach (int key in _displaysQueued.Keys)', 'foreach (EntityId key in _displaysQueued.Keys)')
        fixed = fixed.replace('if (num != -1)', 'if (num != EntityId.None)')
    path.write_text(fixed, encoding='utf-8')
    changes.append({
        'file': path.relative_to(PROJECT).as_posix(),
        'beforeSHA256': hashlib.sha256(before).hexdigest(),
        'afterSHA256': hashlib.sha256(path.read_bytes()).hexdigest(),
        'reason': 'Unity 6000.6 disallows Object.GetInstanceID. Preserve object identity using EntityId; comparisons use CompareTo and queued display keys use EntityId.None sentinel.',
    })
(ROOT / 'reports/unity6/entity-id-migration.json').write_text(json.dumps(changes, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps({'changedFiles': len(changes)}, ensure_ascii=False))
