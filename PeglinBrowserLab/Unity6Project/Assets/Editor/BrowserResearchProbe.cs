using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BrowserResearchProbe
{
    [Serializable]
    private class ProbeReport
    {
        public string editorVersion;
        public string project;
        public bool editorImportCompleted;
        public bool gamePlayed;
        public bool browserTested;
        public string[] scenes;
        public int prefabs;
        public int textures;
        public int materials;
        public int shaders;
        public string[] shadersWithErrors;
        public string[] assemblyFiles;
        public string[] windowsBundles;
        public bool webBuildSupport;
        public string saveDirectory;
    }

    public static void Inspect()
    {
        string project = Path.GetDirectoryName(Application.dataPath);
        string output = "E:/NEWTEST/PeglinBrowserLab/Reports";
        Directory.CreateDirectory(output);
        string[] shaderPaths = AssetDatabase.FindAssets("t:Shader")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.StartsWith("Assets/", StringComparison.Ordinal)).ToArray();
        ProbeReport report = new ProbeReport
        {
            editorVersion = Application.unityVersion,
            project = project,
            editorImportCompleted = true,
            gamePlayed = false,
            browserTested = false,
            scenes = AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath).ToArray(),
            prefabs = AssetDatabase.FindAssets("t:Prefab").Length,
            textures = AssetDatabase.FindAssets("t:Texture2D").Length,
            materials = AssetDatabase.FindAssets("t:Material").Length,
            shaders = shaderPaths.Length,
            shadersWithErrors = shaderPaths.Where(p =>
            {
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(p);
                return shader == null || ShaderUtil.ShaderHasError(shader);
            }).ToArray(),
            assemblyFiles = Directory.GetFiles(Path.Combine(project, "Library/ScriptAssemblies"), "Assembly-CSharp*.dll")
                .Select(Path.GetFileName).ToArray(),
            windowsBundles = Directory.Exists(Path.Combine(Application.streamingAssetsPath, "aa/StandaloneWindows64"))
                ? Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "aa/StandaloneWindows64"), "*.bundle")
                    .Select(Path.GetFileName).ToArray() : new string[0],
            webBuildSupport = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL),
            saveDirectory = Application.persistentDataPath
        };
        File.WriteAllText(Path.Combine(output, "editor-probe.json"), JsonUtility.ToJson(report, true));
        Debug.Log("BROWSER_RESEARCH_PROBE_COMPLETED " + output);
    }

    // This is a platform compilation experiment, not a playable release.
    // Original Windows Addressables and placeholder shaders still need repair.
    public static void BuildPlatformSmoke()
    {
        string output = "E:/NEWTEST/PeglinBrowserLab/Builds/PlatformSmoke";
        Directory.CreateDirectory(output);
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.template = "APPLICATION:Default";
        PlayerSettings.WebGL.dataCaching = true;
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/PreMainMenu.unity" },
            locationPathName = output,
            target = BuildTarget.WebGL,
            options = BuildOptions.Development
        });
        Directory.CreateDirectory("E:/NEWTEST/PeglinBrowserLab/Reports");
        File.WriteAllText("E:/NEWTEST/PeglinBrowserLab/Reports/web-build-summary.txt",
            "result=" + report.summary.result + "\nerrors=" + report.summary.totalErrors +
            "\nwarnings=" + report.summary.totalWarnings + "\nbytes=" + report.summary.totalSize +
            "\nscope=Platform compilation test only. Not a playable Peglin release.\n");
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("Platform smoke build failed: " + report.summary.result);
        Debug.Log("BROWSER_PLATFORM_SMOKE_BUILD_COMPLETED " + output);
    }
}
