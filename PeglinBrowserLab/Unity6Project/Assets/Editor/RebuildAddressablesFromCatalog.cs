using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

// Reconstructs the Addressables editor configuration (lost in the recovered project) from the
// shipped Windows catalog.json: one group per original bundle, one entry per catalog asset,
// the same addresses and labels. Run with -executeMethod RebuildAddressablesFromCatalog.Dump
// (report only) or .Apply (creates AddressableAssetsData).
public static class RebuildAddressablesFromCatalog
{
    const string CatalogPath = "Assets/StreamingAssets/aa/catalog.json";
    const string ReportPath = "E:/NEWTEST/PeglinBrowserLab/Reports/addressables-catalog-dump.json";

    [Serializable]
    class Row
    {
        public string primaryKey;
        public string internalId;
        public string resourceType;
        public string provider;
        public string bundle;
        public string[] otherKeys;
    }

    [Serializable]
    class Report
    {
        public int locations;
        public int assetEntries;
        public string[] bundles;
        public Row[] rows;
        public string[] missingAssets;
        public string[] labelCandidates;
    }

    static string GroupNameFromBundle(string bundleId)
    {
        string file = Path.GetFileNameWithoutExtension(bundleId.Replace('\\', '/'));
        // enemies_assets_all_<hash>  ->  enemies ; defaultlocalgroup_scenes_all_<hash> -> defaultlocalgroup_scenes
        int cut = file.IndexOf("_assets_all_", StringComparison.Ordinal);
        if (cut < 0) cut = file.IndexOf("_scenes_all_", StringComparison.Ordinal);
        if (cut >= 0)
            return file.Substring(0, cut) + (file.Contains("_scenes_all_") ? "_scenes" : "");
        return file; // unitybuiltinshaders / monoscripts bundles
    }

    static (Dictionary<IResourceLocation, List<string>> keysByLocation, List<IResourceLocation> locations) Load()
    {
        var catalog = JsonUtility.FromJson<JsonContentCatalogData>(File.ReadAllText(CatalogPath));
        var locator = catalog.CreateLocator();
        var keysByLocation = new Dictionary<IResourceLocation, List<string>>();
        foreach (var key in locator.Keys)
        {
            if (!locator.Locate(key, null, out var locs)) continue;
            foreach (var l in locs)
            {
                if (!keysByLocation.TryGetValue(l, out var list)) keysByLocation[l] = list = new List<string>();
                list.Add(key.ToString());
            }
        }
        return (keysByLocation, keysByLocation.Keys.ToList());
    }

    public static void Dump()
    {
        var (keysByLocation, locations) = Load();
        var rows = new List<Row>();
        var missing = new List<string>();
        var bundles = new HashSet<string>();
        foreach (var l in locations)
        {
            string bundle = l.Dependencies != null && l.Dependencies.Count > 0 ? l.Dependencies[0].InternalId : "";
            rows.Add(new Row
            {
                primaryKey = l.PrimaryKey,
                internalId = l.InternalId,
                resourceType = l.ResourceType != null ? l.ResourceType.FullName : "",
                provider = l.ProviderId,
                bundle = bundle,
                otherKeys = keysByLocation[l].Where(k => k != l.PrimaryKey).ToArray()
            });
            if (!string.IsNullOrEmpty(bundle)) bundles.Add(bundle);
            if (l.InternalId.StartsWith("Assets/", StringComparison.Ordinal) && AssetDatabase.AssetPathToGUID(l.InternalId).Length == 0)
                missing.Add(l.InternalId);
        }
        var report = new Report
        {
            locations = locations.Count,
            assetEntries = rows.Count(r => r.internalId.StartsWith("Assets/", StringComparison.Ordinal)),
            bundles = bundles.OrderBy(b => b).ToArray(),
            rows = rows.ToArray(),
            missingAssets = missing.ToArray(),
            labelCandidates = new string[0]
        };
        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
        File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
        Debug.Log("ADDRESSABLES_DUMP_DONE " + ReportPath);
    }
    static readonly Dictionary<string, string> GroupDisplay = new Dictionary<string, string>
    {
        { "defaultlocalgroup_scenes", "Default Local Group" },
        { "duplicateassetisolation", "Duplicate Asset Isolation" },
    };

    public static void Apply()
    {
        var (keysByLocation, locations) = Load();
        var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        var groups = new Dictionary<string, AddressableAssetGroup>();
        var done = new HashSet<string>();
        var missing = new List<string>();
        var map = new List<string>();      // oldGuid	newGuid	path  (used to patch serialized AssetReferences)
        int created = 0;
        foreach (var l in locations)
        {
            if (!l.InternalId.StartsWith("Assets/", StringComparison.Ordinal)) continue;
            if (l.Dependencies == null || l.Dependencies.Count == 0) continue;
            string groupKey = GroupNameFromBundle(l.Dependencies[0].InternalId);
            string guid = AssetDatabase.AssetPathToGUID(l.InternalId);
            if (guid.Length == 0)
            {
                // AssetRipper re-exported some assets with another extension (wav/mp3 -> ogg, ttf -> otf)
                foreach (var ext in new[] { ".ogg", ".otf", ".wav", ".mp3", ".ttf" })
                {
                    string alt = Path.ChangeExtension(l.InternalId, ext);
                    guid = AssetDatabase.AssetPathToGUID(alt);
                    if (guid.Length != 0) break;
                }
            }
            if (guid.Length == 0) { missing.Add(l.InternalId); continue; }
            if (!groups.TryGetValue(groupKey, out var group))
            {
                string display = GroupDisplay.TryGetValue(groupKey, out var d) ? d : groupKey;
                group = settings.FindGroup(display) ?? settings.CreateGroup(display, false, false, false, null, typeof(BundledAssetGroupSchema));
                groups[groupKey] = group;
            }
            var entry = settings.CreateOrMoveEntry(guid, group, false, false);
            if (entry == null) { missing.Add(l.InternalId + " (entry null)"); continue; }
            entry.address = l.PrimaryKey;
            foreach (var k in keysByLocation[l])
            {
                if (k == l.PrimaryKey) continue;
                if (k.Length == 32 && k.All(Uri.IsHexDigit)) { map.Add(k + "	" + guid + "	" + l.InternalId); continue; }
                settings.AddLabel(k, false);
                entry.SetLabel(k, true, false, false);
            }
            if (done.Add(l.InternalId)) created++;
        }
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
        AssetDatabase.SaveAssets();
        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
        File.WriteAllLines("E:/NEWTEST/PeglinBrowserLab/Reports/addressables-guid-map.tsv", map.Distinct());
        File.WriteAllLines("E:/NEWTEST/PeglinBrowserLab/Reports/addressables-missing-assets.txt", missing.Distinct());
        Debug.Log("ADDRESSABLES_APPLY_DONE entries=" + created + " groups=" + groups.Count + " missing=" + missing.Distinct().Count() + " guidAliases=" + map.Distinct().Count());
    }

    public static void BuildContent()
    {
        var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
        AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent(out var result);
        Debug.Log("ADDRESSABLES_BUILD_DONE error=" + (string.IsNullOrEmpty(result.Error) ? "none" : result.Error) + " outputPath=" + result.OutputPath + " duration=" + result.Duration);
        if (!string.IsNullOrEmpty(result.Error)) throw new Exception(result.Error);
    }
}
