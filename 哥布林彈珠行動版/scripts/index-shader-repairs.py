import collections
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PROJECT = Path('E:/NEWTEST/PeglinBrowserLab/Unity6Project')
ASSETS = PROJECT / 'Assets'
shaders = {}
for path in ASSETS.rglob('*.shader'):
    metadata = Path(str(path) + '.meta').read_text(encoding='utf-8-sig')
    guid = re.search(r'(?m)^guid: ([0-9a-f]{32})', metadata)[1]
    text = path.read_text(encoding='utf-8-sig')
    name = re.search(r'Shader\s+"([^"]+)"', text)[1]
    shaders[guid] = {'name': name, 'file': path.relative_to(PROJECT).as_posix(),
                     'placeholder': 'DummyShaderTextExporter' in text, 'materials': []}
builtins = collections.defaultdict(list)
for path in ASSETS.rglob('*.mat'):
    text = path.read_text(encoding='utf-8-sig')
    pointer = re.search(r'm_Shader: \{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: \d+\}', text)
    if pointer:
        if pointer[2] in shaders:
            shaders[pointer[2]]['materials'].append(path.relative_to(PROJECT).as_posix())
        else:
            builtins[pointer[2] + ':' + pointer[1]].append(path.relative_to(PROJECT).as_posix())
rows = sorted(shaders.values(), key=lambda row: len(row['materials']), reverse=True)
output = ROOT / 'reports/unity6/shader-repair-index.json'
output.write_text(json.dumps({'shaders': rows, 'otherShaderReferences': dict(builtins)}, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps([{'name': row['name'], 'materials': len(row['materials']), 'placeholder': row['placeholder']} for row in rows[:8]], ensure_ascii=False, indent=2))
