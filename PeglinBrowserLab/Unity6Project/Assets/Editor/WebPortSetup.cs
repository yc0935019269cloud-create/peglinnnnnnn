using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WebPortSetup
{
    const string BootScene = "Assets/Scenes/WebBoot.unity";

    public static void CreateBootScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var go = new GameObject("WebAddressablesBoot");
        go.AddComponent<WebAddressablesBoot>();
        var cam = new GameObject("Camera").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        EditorSceneManager.SaveScene(scene, BootScene);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(BootScene, true),
            new EditorBuildSettingsScene("Assets/Scenes/PreMainMenu.unity", true),
        };
        Debug.Log("WEBBOOT_SCENE_DONE");
    }

    public static void BuildWeb()
    {
        string output = "E:/NEWTEST/PeglinBrowserLab/Builds/WebTest";
        Directory.CreateDirectory(output);
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.template = "APPLICATION:Default";
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { BootScene, "Assets/Scenes/PreMainMenu.unity" },
            locationPathName = output,
            target = BuildTarget.WebGL,
            options = BuildOptions.Development
        });
        File.WriteAllText("E:/NEWTEST/PeglinBrowserLab/Reports/web-test-summary.txt",
            "result=" + report.summary.result + "\nerrors=" + report.summary.totalErrors + "\nbytes=" + report.summary.totalSize +
            "\nscope=Work-in-progress web port test build. Not a playable release.\n");
        if (report.summary.result != BuildResult.Succeeded) throw new System.Exception("Web test build failed: " + report.summary.result);
        Debug.Log("WEBTEST_BUILD_DONE " + output);
    }

    // Non-development build for actually playing (phone/tablet). Gzip with decompression fallback
    // so it works from any plain static web server, which cannot be relied on to send
    // Content-Encoding headers.
    public static void BuildWebRelease()
    {
        string output = "E:/NEWTEST/PeglinBrowserLab/Builds/WebRelease";
        Directory.CreateDirectory(output);
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.template = "APPLICATION:Default";
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { BootScene, "Assets/Scenes/PreMainMenu.unity" },
            locationPathName = output,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });
        File.WriteAllText("E:/NEWTEST/PeglinBrowserLab/Reports/web-release-summary.txt",
            "result=" + report.summary.result + "\nerrors=" + report.summary.totalErrors + "\nbytes=" + report.summary.totalSize +
            "\nscope=Release (non-development) web build of the port for local play testing.\n");
        if (report.summary.result != BuildResult.Succeeded) throw new System.Exception("Web release build failed: " + report.summary.result);
        Debug.Log("WEBRELEASE_BUILD_DONE " + output);
    }
}
