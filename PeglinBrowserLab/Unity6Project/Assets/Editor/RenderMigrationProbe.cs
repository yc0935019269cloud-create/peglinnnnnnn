using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class RenderMigrationProbe
{
    public static void ValidateAndBuild()
    {
        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/MonoBehaviour/UniversalRenderPipeline(2D).asset");
        var renderer = AssetDatabase.LoadAssetAtPath<Renderer2DData>("Assets/MonoBehaviour/UniversalRenderPipeline(2D)_Renderer.asset");
        if (pipeline == null || renderer == null) throw new Exception("Migrated pipeline or 2D renderer did not deserialize");
        // The recovered 2022 post-processing resource list lacks newer mandatory shaders.
        // Use the official engine resource list; camera/volume effect settings stay in the game assets.
        var rendererSettings = new SerializedObject(renderer);
        rendererSettings.FindProperty("m_PostProcessData").objectReferenceValue = AssetDatabase.LoadAssetAtPath<PostProcessData>("Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
        rendererSettings.ApplyModifiedPropertiesWithoutUndo();
        UnityEngine.Rendering.ResourceReloader.ReloadAllNullIn(renderer, "Packages/com.unity.render-pipelines.universal");
        EditorUtility.SetDirty(renderer);
        GraphicsSettings.defaultRenderPipeline = pipeline;
        var log = new List<string> { "pipeline=" + pipeline.name, "renderer=" + renderer.name };
        foreach (var scenePath in new[] { "Assets/Scenes/PreMainMenu.unity", "Assets/Scenes/MainMenu.unity", "Assets/Scenes/ForestMap.unity", "Assets/Scenes/Battle.unity" })
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int missing = 0, lights = 0, cameras = 0, renderers = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true)) missing += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                lights += root.GetComponentsInChildren<Light2D>(true).Length;
                cameras += root.GetComponentsInChildren<UniversalAdditionalCameraData>(true).Length;
                renderers += root.GetComponentsInChildren<SpriteRenderer>(true).Length;
            }
            log.Add(scene.name + ": missingScripts=" + missing + " lights=" + lights + " urpCameras=" + cameras + " spriteRenderers=" + renderers);
            if (missing != 0) throw new Exception("Missing scripts in " + scene.name + ": " + missing);
        }
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AssetDatabase.SaveAssets();
        File.WriteAllLines("E:/NEWTEST/PeglinBrowserLab/Reports/render-scene-validation.txt",log);
        Debug.Log("RENDER_SCENES_DONE " + string.Join("; ",log));
        RebuildAddressablesFromCatalog.BuildContent();
        WebPortSetup.BuildWeb();
    }
    [Serializable] public class Entry { public string type, path, guid; public long fileID; }
    [Serializable] public class Report { public Entry[] scripts; }
    public static void DumpOldScripts()
    {
        var entries = new List<Entry>();
        foreach (var path in AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/Plugins/Unity.RenderPipeline") && p.EndsWith(".dll")))
        foreach (var script in AssetDatabase.LoadAllAssetsAtPath(path).OfType<MonoScript>())
        {
            if (script.GetClass() == null) continue;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(script, out string guid, out long id);
            entries.Add(new Entry { type = script.GetClass().FullName, path = path, guid = guid, fileID = id });
        }
        File.WriteAllText("E:/NEWTEST/PeglinBrowserLab/Reports/render-old-scripts.json", JsonUtility.ToJson(new Report { scripts = entries.ToArray() }, true));
        Debug.Log("RENDER_OLD_SCRIPTS_DONE " + entries.Count);
    }
}
