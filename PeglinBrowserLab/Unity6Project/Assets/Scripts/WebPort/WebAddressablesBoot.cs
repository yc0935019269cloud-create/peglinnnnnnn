using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Web port: the original game calls AsyncOperationHandle.WaitForCompletion() on Addressables at many
// call sites. That cannot block in a browser, but it returns immediately for operations that are
// already complete. This boot scene loads every non-scene Addressable asset once (using the same
// (location, type) pair the game will ask for, which is the ResourceManager's operation-cache key)
// and keeps the handles, so the unmodified call sites find finished operations.
public class WebAddressablesBoot : MonoBehaviour
{
    public string nextScene = "PreMainMenu";
    public int concurrent = 48;

    static readonly List<object> Held = new List<object>();   // keeps every preloaded handle alive
    Text label;
    int shownPercent = -1;

    // A single uGUI label (IMGUI OnGUI made every frame take >1 s in the browser).
    void CreateLabel()
    {
        var canvas = new GameObject("WebBootCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        label = new GameObject("Progress").AddComponent<Text>();
        label.transform.SetParent(canvas.transform, false);
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 32;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        var rt = label.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        label.text = "Loading...";
    }

    void ShowProgress(int done, int total)
    {
        int percent = total > 0 ? done * 100 / total : 0;
        if (percent == shownPercent) return;
        shownPercent = percent;
        label.text = "Loading " + percent + "%";
    }

    IEnumerator Start()
    {
        Application.targetFrameRate = 60;
        CreateLabel();
        var init = Addressables.InitializeAsync();
        yield return init;

        var wanted = new List<(IResourceLocation loc, System.Type type)>();
        var seen = new HashSet<string>();
        foreach (var locator in Addressables.ResourceLocators)
        {
            foreach (var key in locator.Keys)
            {
                if (!locator.Locate(key, null, out var locs)) continue;
                foreach (var loc in locs)
                {
                    if (loc.ProviderId == null || !loc.ProviderId.Contains("BundledAssetProvider")) continue;
                    if (loc.ResourceType == typeof(SceneInstance)) continue;
                    Add(wanted, seen, loc, loc.ResourceType);
                    // PegLayoutLoader requests pegboard data and peg prefabs (RegularPeg, DullPeg, bombs...)
                    // as LoadAssetAsync<object>, which is a separate cache entry from the typed load.
                    if (loc.ResourceType == typeof(TextAsset) || loc.ResourceType == typeof(GameObject)) Add(wanted, seen, loc, typeof(object));
                }
            }
        }

        int done = 0, failures = 0, next = 0;
        float lastLog = Time.realtimeSinceStartup;
        var inFlight = new List<(Pending p, IResourceLocation loc, System.Type type)>();
        while (next < wanted.Count || inFlight.Count > 0)
        {
            while (next < wanted.Count && inFlight.Count < concurrent)
            {
                var (loc, type) = wanted[next++];
                var h = Provide(loc, type);
                Held.Add(h.Handle);
                inFlight.Add((h, loc, type));
            }
            for (int i = inFlight.Count - 1; i >= 0; i--)
            {
                if (!inFlight[i].p.IsDone) continue;
                done++;
                if (inFlight[i].p.Failed)
                {
                    failures++;
                    Debug.LogWarning($"[WebBoot] failed: {inFlight[i].loc.PrimaryKey} ({inFlight[i].type.Name}) {inFlight[i].p.Error}");
                }
                inFlight.RemoveAt(i);
            }
            ShowProgress(done, wanted.Count);
            if (Time.realtimeSinceStartup - lastLog > 2f)
            {
                lastLog = Time.realtimeSinceStartup;
                string first = inFlight.Count > 0 ? inFlight[0].loc.PrimaryKey + " (" + inFlight[0].type.Name + ", " + inFlight[0].loc.InternalId + ")" : "-";
                Debug.Log($"[WebBoot] progress done={done} inFlight={inFlight.Count} failures={failures} firstPending={first}");
            }
            yield return null;
        }

        Debug.Log($"[WebBoot] preloaded {done} (location,type) operations, failures={failures}, " +
                  $"managedMemory={System.GC.GetTotalMemory(false) / (1024 * 1024)} MB");
        label.text = "Starting...";
        yield return null;
        SceneManager.LoadScene(nextScene);
    }

    // ResourceManager.ProvideResource(location, type) is internal; the generic overload is public.
    // Handles are generic structs, so they are held and polled as objects.
    static readonly System.Reflection.MethodInfo ProvideGeneric = typeof(UnityEngine.ResourceManagement.ResourceManager)
        .GetMethod("ProvideResource", new[] { typeof(IResourceLocation) });

    sealed class Pending
    {
        public object Handle;
        System.Reflection.PropertyInfo isDone, status;
        public Pending(object handle)
        {
            Handle = handle;
            var t = handle.GetType();
            isDone = t.GetProperty("IsDone");
            status = t.GetProperty("Status");
        }
        public bool IsDone => (bool)isDone.GetValue(Handle);
        public bool Failed => (AsyncOperationStatus)status.GetValue(Handle) == AsyncOperationStatus.Failed;
        public string Error { get { var p = Handle.GetType().GetProperty("OperationException"); return p?.GetValue(Handle)?.ToString(); } }
    }

    static Pending Provide(IResourceLocation loc, System.Type type)
        => new Pending(ProvideGeneric.MakeGenericMethod(type).Invoke(Addressables.ResourceManager, new object[] { loc }));

    static void Add(List<(IResourceLocation, System.Type)> list, HashSet<string> seen, IResourceLocation loc, System.Type type)
    {
        if (seen.Add(loc.InternalId + "|" + loc.ProviderId + "|" + type.FullName)) list.Add((loc, type));
    }

}
