"""Bounded streaming summary for Unity logs, including repeated multi-GB logs."""
import collections
import json
import re
import sys
from pathlib import Path

path = Path(sys.argv[1])
result = {'file': str(path), 'bytes': path.stat().st_size, 'lines': 0, 'repeatedThreadWarnings': 0,
          'firstThreadWarningContext': [], 'lastDistinctLines': [], 'compilerErrors': [], 'lastImportedAssets': []}
context = collections.deque(maxlen=18)
last = collections.deque(maxlen=14)
assets = collections.deque(maxlen=12)
errors = set()
build_errors = set()
with path.open(encoding='utf-8-sig', errors='replace') as stream:
    for line in stream:
        result['lines'] += 1
        line = line.rstrip()[:2400]
        if 'Do not use ReadObjectThreaded on scene objects!' in line:
            result['repeatedThreadWarnings'] += 1
            if not result['firstThreadWarningContext']:
                result['firstThreadWarningContext'] = list(context)
            continue
        if 'error CS' in line:
            errors.add(line)
        if re.search(r'error:|BuildFailedException|Error building|IL2CPP.*[Ee]rror|Exception:', line) and len(build_errors) < 100:
            build_errors.add(line)
        if " - Path: Assets/" in line and 'with importer' in line:
            assets.append(line)
        if not last or last[-1] != line:
            last.append(line)
        context.append(line)
result['compilerErrors'] = sorted(errors)
result['buildErrors'] = sorted(build_errors)
result['lastDistinctLines'] = list(last)
result['lastImportedAssets'] = list(assets)
output = path.with_suffix('.summary.json')
output.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps({key: result[key] for key in ('bytes', 'lines', 'repeatedThreadWarnings')}, ensure_ascii=False))
print('First warning context:')
print('\n'.join(result['firstThreadWarningContext'][-8:]))
print('Last imported assets:')
print('\n'.join(result['lastImportedAssets'][-4:]))
