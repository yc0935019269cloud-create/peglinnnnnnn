"""Prepare the separate Unity 6 working copy; never mutate the raw recovery."""
import hashlib
import json
import re
from pathlib import Path

RESEARCH = Path(__file__).resolve().parent.parent
PROJECT = Path('E:/NEWTEST/PeglinBrowserLab/Unity6Project')
REPORT = RESEARCH / 'reports' / 'unity6' / 'preparation.json'
if not (PROJECT / 'ProjectSettings/ProjectVersion.txt').is_file():
    raise SystemExit('Copy the raw recovered project into Unity6Project first.')
if REPORT.exists():
    raise SystemExit('Preparation already recorded. Inspect the existing report before making further changes.')

changes = []
def change(path, new_text, reason):
    old = path.read_text(encoding='utf-8-sig')
    if old == new_text:
        return
    changes.append({
        'path': path.relative_to(PROJECT).as_posix(),
        'reason': reason,
        'beforeSHA256': hashlib.sha256(path.read_bytes()).hexdigest(),
        'afterSHA256': hashlib.sha256(new_text.encode('utf-8')).hexdigest(),
    })
    path.write_text(new_text, encoding='utf-8')

for path in (PROJECT / 'Assets/Plugins').glob('*.dll.meta'):
    text = path.read_text(encoding='utf-8-sig')
    fixed = re.sub(r'(Editor: Editor\s+second:\s+enabled:) 0', r'\1 1', text)
    change(path, fixed, 'Enable recovered managed plugin for initial Editor import; no new package DLLs installed yet.')

path = PROJECT / 'ProjectSettings/ProjectSettings.asset'
text = path.read_text(encoding='utf-8-sig')
for field, value in {
    'companyName': 'LocalPeglinResearch',
    'productName': 'PeglinBrowserLab',
    'cloudProjectId': '',
    'organizationId': '',
}.items():
    text = re.sub(rf'(?m)^(  {field}:).*$', lambda m: m[1] + (' ' + value if value else ''), text)
change(path, text, 'Separate test save identity and detach original developer cloud project in working copy.')

# Preserve the original ProjectVersion until Unity performs its upgrade.
# Do not play the game during this import phase.
REPORT.write_text(json.dumps({
    'project': str(PROJECT),
    'targetEditor': '6000.6.5f1',
    'rawRecoveryUnchanged': True,
    'changes': changes,
    'notes': [
        'Initial import uses recovered package DLLs to measure actual upgrade failures.',
        'No game rules, purchase checks, scenes, or placeholder shaders changed here.',
        'No play session or Web build has run yet.',
    ],
}, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps({'project': str(PROJECT), 'changedFiles': len(changes)}, ensure_ascii=False))
