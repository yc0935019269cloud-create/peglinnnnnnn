"""Rewrite serialized Addressables AssetReference GUIDs in the Unity 6 working copy.

The recovered project's asset GUIDs differ from the GUIDs recorded in the shipped Addressables
catalog; scenes/prefabs still hold the original ones (`m_AssetGUID: <old>`). Using the map written by
RebuildAddressablesFromCatalog.Apply (old catalog guid -> recovered asset guid) this rewrites them.
Reversible: reports/unity6/addressable-guid-patch.json holds every change; run with --revert.
"""
import json, re, sys
from pathlib import Path

PROJ = Path(r"E:\NEWTEST\PeglinBrowserLab\Unity6Project")
MAP = Path(r"E:\NEWTEST\PeglinBrowserLab\Reports\addressables-guid-map.tsv")
REPORT = Path(__file__).resolve().parent.parent / "reports" / "unity6" / "addressable-guid-patch.json"
PAT = re.compile(r"(m_AssetGUID: )([0-9a-f]{32})")

def load_map():
    m = {}
    for line in MAP.read_text(encoding="utf-8").splitlines():
        old, new, _path = line.split("\t")
        m[old] = new
    return m

def run(revert=False):
    if revert:
        rep = json.loads(REPORT.read_text(encoding="utf-8"))
        back = {v: k for k, v in rep["mapUsed"].items()}
        mapping = back
    else:
        mapping = load_map()
    changed, counts, used = {}, 0, {}
    for ext in ("*.unity", "*.prefab", "*.asset"):
        for f in (PROJ / "Assets").rglob(ext):
            raw = f.read_bytes()
            try:
                text = raw.decode("utf-8")
            except UnicodeDecodeError:
                continue
            if "m_AssetGUID:" not in text:
                continue
            n = 0
            def sub(m):
                nonlocal n
                new = mapping.get(m.group(2))
                if new is None:
                    return m.group(0)
                n += 1
                used[m.group(2)] = new
                return m.group(1) + new
            out = PAT.sub(sub, text)
            if n:
                f.write_bytes(out.encode("utf-8"))
                changed[str(f.relative_to(PROJ))] = n
                counts += n
    if not revert:
        prev = json.loads(REPORT.read_text(encoding="utf-8")) if REPORT.exists() else {"mapUsed": {}, "files": {}, "referencesChanged": 0}
        prev["mapUsed"].update(used)
        for k, v in changed.items():
            prev["files"][k] = prev["files"].get(k, 0) + v
        prev["referencesChanged"] += counts
        prev["filesChanged"] = len(prev["files"])
        REPORT.write_text(json.dumps(prev, indent=1), encoding="utf-8")
    print(("reverted" if revert else "patched"), len(changed), "files,", counts, "references,", len(used), "distinct guids")

if __name__ == "__main__":
    run(revert="--revert" in sys.argv)
