using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

// Development-build evidence only. No gameplay or saved state changes.
public class WebRenderDiagnostics : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        if (!Debug.isDebugBuild) return;
        var go = new GameObject("WebRenderDiagnostics");
        DontDestroyOnLoad(go);
        go.AddComponent<WebRenderDiagnostics>();
    }
    void Awake() { SceneManager.sceneLoaded += Loaded; }
    void OnDestroy() { SceneManager.sceneLoaded -= Loaded; }
    void Loaded(Scene scene, LoadSceneMode mode) { StartCoroutine(Report(scene.name)); }
    IEnumerator Report(string name)
    {
        yield return new WaitForSecondsRealtime(3);
        Debug.Log("[WebRender] scene=" + name + " active=" + SceneManager.GetActiveScene().name + " pipeline=" + GraphicsSettings.currentRenderPipeline?.name);
        foreach (var cam in Camera.allCameras)
            Debug.Log("[WebRender] camera=" + cam.name + " enabled=" + cam.enabled + " mask=" + cam.cullingMask + " rect=" + cam.rect + " target=" + (cam.targetTexture == null ? "screen" : cam.targetTexture.name));
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        var sprites = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        Debug.Log("[WebRender] activeCanvases=" + canvases.Length + " activeSprites=" + sprites.Length);
    }
}
