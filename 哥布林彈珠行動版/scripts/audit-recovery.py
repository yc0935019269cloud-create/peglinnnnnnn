"""Inventory the recovered Unity project without launching the game or touching saves."""
import collections
import hashlib
import json
import re
from functools import lru_cache
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PROJECT = ROOT / 'RecoveredProject' / 'ExportedProject'
ASSETS = PROJECT / 'Assets'
REPORTS = ROOT / 'reports'
GUID = re.compile(r'\bguid:\s*([0-9a-fA-F]{32})')
SCRIPT = re.compile(r'm_Script:\s*\{fileID:\s*(-?\d+),\s*guid:\s*([0-9a-fA-F]{32})')
YAML_EXTENSIONS = {'.unity', '.prefab', '.asset', '.mat', '.controller', '.anim', '.overridecontroller', '.playable'}
BUILTIN_GUIDS = {'0' * 32, '0000000000000000f000000000000000', '0000000000000000e000000000000000', '0000000000000000d000000000000000'}

def relative(path):
    return path.relative_to(PROJECT).as_posix()

@lru_cache(maxsize=None)
def read(path):
    return path.read_text(encoding='utf-8-sig', errors='replace')

def write(name, data):
    (REPORTS / name).write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding='utf-8')

all_files = list(ASSETS.rglob('*'))
files = [p for p in all_files if p.is_file()]
guids = {}
duplicates = {}
for path in files:
    if path.suffix != '.meta':
        continue
    match = GUID.search(read(path))
    if match:
        guid = match[1].lower()
        if guid in guids:
            duplicates.setdefault(guid, [relative(guids[guid])]).append(relative(path))
        guids[guid] = path

missing = collections.defaultdict(set)
script_missing = []
script_zero = []
scene_info = []
local_pointer_gaps = []
yaml_count = 0
for path in files:
    if path.suffix not in YAML_EXTENSIONS:
        continue
    text = read(path)
    if not text.startswith('%YAML'):
        continue
    yaml_count += 1
    for guid in GUID.findall(text):
        guid = guid.lower()
        if guid not in guids and guid not in BUILTIN_GUIDS:
            missing[guid].add(relative(path))
    for fileid, guid in SCRIPT.findall(text):
        if guid.lower() not in guids and guid.lower() not in BUILTIN_GUIDS:
            script_missing.append({'file': relative(path), 'guid': guid, 'fileID': fileid})
    if re.search(r'm_Script:\s*\{fileID:\s*0\}', text):
        script_zero.append(relative(path))
    if path.suffix == '.unity':
        scene_info.append({
            'file': relative(path),
            'bytes': path.stat().st_size,
            'objects': len(re.findall(r'^--- !u!', text, re.M)),
            'gameObjects': len(re.findall(r'^--- !u!1\s', text, re.M)),
            'monoBehaviours': len(re.findall(r'^--- !u!114\s', text, re.M)),
            'externalScriptReferences': len(SCRIPT.findall(text)),
        })
    if path.suffix in {'.unity', '.prefab'}:
        object_ids = set(re.findall(r'^--- !u!\d+ &(-?\d+)', text, re.M))
        local_refs = set(re.findall(r'\{fileID:\s*(-?\d+)\}', text)) - {'0'}
        gaps = sorted(local_refs - object_ids, key=int)
        if gaps:
            local_pointer_gaps.append({'file': relative(path), 'missingLocalIDs': gaps})

source_risks = {}
patterns = {
    'steam_calls': r'\b(?:SteamAPI|SteamApps|SteamUser|SteamManager|SteamRemoteStorage|SteamFriends)\b',
    'gog_calls': r'\b(?:Galaxy|GalaxyManager)\b',
    'addressables': r'\bAddressables\.',
    'disk_io': r'\b(?:File|Directory|FileStream)\.',
    'threads_tasks': r'\b(?:Thread|Task|ThreadPool)\b',
    'purchase_services': r'\b(?:UnityPurchasing|UnityServices|IsFullGameLocked)\b',
    'touch_input': r'\b(?:Input\.touchCount|Input\.GetTouch|TouchScreen|InputType\.Touch)\b',
    'native_imports': r'\bDllImport\b',
}
for name, expression in patterns.items():
    rows = []
    pattern = re.compile(expression)
    for path in files:
        if path.suffix != '.cs':
            continue
        matches = [{'line': number, 'code': line.strip()} for number, line in enumerate(read(path).splitlines(), 1) if pattern.search(line)]
        if matches:
            rows.append({'file': relative(path), 'matches': matches})
    source_risks[name] = rows

shader_files = [p for p in files if p.suffix == '.shader']
shader_samples = [{'file': relative(p), 'head': read(p)[:400]} for p in shader_files[:3]]
dummy_shaders = [relative(p) for p in shader_files if 'DummyShaderTextExporter' in read(p)]
native_files = [p for p in files if p.suffix.lower() in {'.dll', '.so', '.dylib'}]
editor_disabled_plugins = []
for path in native_files:
    metadata = Path(str(path) + '.meta')
    if metadata.exists() and re.search(r'Editor: Editor\s+second:\s+enabled: 0', read(metadata)):
        editor_disabled_plugins.append(relative(path))
meta_without_asset = [relative(p) for p in files if p.suffix == '.meta' and not Path(str(p)[:-5]).exists()]
counts = collections.Counter(p.suffix.lower() for p in files if p.suffix != '.meta')
source_inventory = json.loads(read(REPORTS / 'source-inventory.json'))
changed_source = []
for row in source_inventory['files']:
    path = Path(source_inventory['source']) / row['path']
    if not path.exists() or hashlib.sha256(path.read_bytes()).hexdigest().upper() != row['sha256']:
        changed_source.append(row['path'])

summary = {
    'project': str(PROJECT),
    'unityVersion': read(PROJECT / 'ProjectSettings' / 'ProjectVersion.txt').strip(),
    'assetFileCount': len(files),
    'assetBytes': sum(p.stat().st_size for p in files),
    'extensionCounts': dict(sorted(counts.items())),
    'scenes': scene_info,
    'guidCheckScope': 'External GUID existence only. Does not validate referenced local fileIDs, field deserialization, shader rendering, or runtime scene loading.',
    'yamlFilesScanned': yaml_count,
    'missingGuidCount': len(missing),
    'missingScriptReferenceCount': len(script_missing),
    'zeroScriptFileCount': len(script_zero),
    'duplicateGuidCount': len(duplicates),
    'localPointerGapFileCount': len(local_pointer_gaps),
    'editorDisabledPluginCount': len(editor_disabled_plugins),
    'dummyShaderCount': len(dummy_shaders),
    'metaWithoutAssetCount': len(meta_without_asset),
    'sourceUnchanged': not changed_source,
    'changedSourceFiles': changed_source,
    'sourceRiskFileCounts': {name: len(rows) for name, rows in source_risks.items()},
    'exportWarnings': [line for line in read(REPORTS / 'assetripper.log').splitlines() if '[Warning]' in line or '[Error]' in line],
    'unityEditorImportTested': False,
    'webglBuilt': False,
    'browserRuntimeTested': False,
}
write('recovery-audit.json', summary)
write('unresolved-references.json', {
    'guids': {guid: sorted(paths) for guid, paths in sorted(missing.items())},
    'missingScriptReferences': script_missing,
    'zeroScriptFiles': script_zero,
    'duplicateGuids': duplicates,
    'metaWithoutAssets': meta_without_asset,
    'localPointerGaps': local_pointer_gaps,
})
write('platform-audit.json', source_risks)
write('shader-audit.json', {'exportMode': 'Dummy', 'shaderCount': len(shader_files), 'dummyShaderCount': len(dummy_shaders), 'dummyShaders': dummy_shaders, 'samples': shader_samples})
write('plugin-inventory.json', {'plugins': [{'file': relative(p), 'bytes': p.stat().st_size} for p in native_files], 'editorDisabled': editor_disabled_plugins})
print(json.dumps({key: summary[key] for key in ('extensionCounts', 'missingGuidCount', 'missingScriptReferenceCount', 'zeroScriptFileCount', 'duplicateGuidCount', 'localPointerGapFileCount', 'editorDisabledPluginCount', 'dummyShaderCount', 'metaWithoutAssetCount', 'sourceUnchanged', 'sourceRiskFileCounts')}, ensure_ascii=False, indent=2))
