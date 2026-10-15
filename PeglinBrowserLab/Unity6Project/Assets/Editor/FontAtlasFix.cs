using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

// Web port: the game's CJK/KR text uses *dynamic* TMP font assets; glyphs that are not yet in the
// recovered atlas are rasterized into the atlas texture at runtime. AssetRipper exported those atlases
// as ordinary PNG textures with automatic (compressed) platform formats, so on WebGL nothing can be
// written into them and those characters render as blanks. Make every atlas of a dynamic font asset
// an uncompressed, readable Alpha8 texture, like the original font sub-asset.
public static class FontAtlasFix
{
    const string Backup = "E:/NEWTEST/PeglinBrowserLab/RecoveredLibraries/FontAtlasMetas";
    const string ReportPath = "E:/NEWTEST/PeglinBrowserLab/Reports/font-atlas-fix.txt";

    public static void Apply()
    {
        var log = new List<string>();
        var done = new HashSet<string>();
        foreach (var guid in AssetDatabase.FindAssets("t:TMP_FontAsset"))
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
            if (font == null || font.atlasPopulationMode != AtlasPopulationMode.Dynamic || font.atlasTextures == null) continue;
            foreach (var tex in font.atlasTextures)
            {
                if (tex == null) continue;
                string path = AssetDatabase.GetAssetPath(tex);
                if (!done.Add(path)) continue;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) { log.Add("SKIP (embedded) " + font.name + " " + path); continue; }
                var platform = importer.GetDefaultPlatformTextureSettings();
                string before = "format=" + platform.format + " compression=" + importer.textureCompression + " readable=" + importer.isReadable + " actual=" + tex.format;
                Directory.CreateDirectory(Backup);
                string metaBackup = Path.Combine(Backup, Path.GetFileName(path) + ".meta");
                if (!File.Exists(metaBackup)) File.Copy(path + ".meta", metaBackup);
                importer.isReadable = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                platform.format = TextureImporterFormat.Alpha8;
                platform.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(platform);
                importer.SaveAndReimport();
                var after = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                log.Add(font.name + " | " + path + " | before " + before + " | after actual=" + after.format + " " + after.width + "x" + after.height);
            }
        }
        File.WriteAllLines(ReportPath, log);
        Debug.Log("FONT_ATLAS_FIX_DONE " + log.Count);
    }

    public static void ApplyAndBuild()
    {
        Apply();
        RebuildAddressablesFromCatalog.BuildContent();
        WebPortSetup.BuildWeb();
    }
}
