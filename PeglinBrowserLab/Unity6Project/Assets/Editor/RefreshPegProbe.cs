using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections;
using Battle;
using Battle.Pachinko.BallBehaviours;
using DG.Tweening;
using Relics;
using UnityEditor;
using UnityEngine;

public static class RefreshPegProbe
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    private static void Call(object obj, Type type, string method) => type.GetMethod(method, PrivateInstance).Invoke(obj, null);
    private static void Set(object obj, Type type, string field, object value) => type.GetField(field, PrivateInstance).SetValue(obj, value);
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static void ValidateAndBuild()
    {
        Validate();
        WebPortSetup.BuildWebRelease();
    }

    public static void Validate()
    {
        var results = new StringBuilder();
        var relics = ScriptableObject.CreateInstance<RelicManager>();
        var controllerObject = new GameObject("RefreshRegressionController") { tag = "GameController" };
        var controller = controllerObject.AddComponent<BattleController>();
        var manager = new PegManager(relics, null, controller, null);
        Set(controller, typeof(BattleController), "_pegManager", manager);
        foreach (bool useLongPeg in new[] { false, true })
        foreach (var type in new[] { Peg.PegType.RESET, Peg.PegType.CRIT })
        foreach (bool orbCallbackFirst in new[] { false, true })
        {
            var go = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                useLongPeg ? "Assets/Prefabs/LongPeg.prefab" : "Assets/Prefabs/RegularPeg.prefab"));
            var peg = go.GetComponent<Peg>();
            peg.relicManager = relics;
            if (peg is RegularPeg regular)
            {
                Call(regular, typeof(RegularPeg), "CheckComponentRefs");
                Set(regular, typeof(RegularPeg), "_initialVector", Vector3.one);
            }
            else
            {
                var mesh = new Mesh { vertices = new[] { new Vector3(-1,-1), new Vector3(-1,1), new Vector3(1,-1), new Vector3(1,1) }, triangles = new[] { 0,1,2,3,2,1 } };
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
                var longPeg = (LongPeg)peg;
                var generator = new SerializedObject(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/LongPegGenerator.prefab").GetComponent<BezierMeshGenerator>());
                Func<string, Material> mat = name => generator.FindProperty(name).objectReferenceValue as Material;
                longPeg.SetMaterials(mat("activeMaterial"), mat("refreshMaterial"), mat("critMaterial"), mat("destroyedMaterial"), mat("persistentCritMaterial"), mat("persistentRefreshMaterial"), mat("vineMaterial"), null, mat("maxHpMaterial"));
                longPeg.Initialize(0.5f, new LongPeg.LongPegColors { Regular=Color.white, Bonus=Color.white, Hit=Color.red, Cleared=Color.white },
                    generator.FindProperty("resetSprite").objectReferenceValue as Sprite,
                    generator.FindProperty("critSprite").objectReferenceValue as Sprite, Vector3.up, null, null);
                manager.delayedDeathPegs.Add(longPeg);
            }
            // Legacy path: conversion before the peg's contact handler consumes the new R/C.
            if (orbCallbackFirst)
            {
                peg.ConvertPegToType(type);
                peg.SetIsBonusSpecialPeg(true);
                peg.PegActivated(false);
                Require(peg.pegType != type, "Legacy collision-order failure did not reproduce");
                peg.Reset();
                if (peg is LongPeg lp) lp.HardReset();
            }
            var conversion = (IEnumerator)typeof(ConvertHitPegs).GetMethod("ConvertSpecialPegAfterHit", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { peg, type });
            Require(conversion.MoveNext(), "Conversion must wait until contact callbacks finish");
            peg.PegActivated(false);
            if (peg is LongPeg contactPeg) Set(contactPeg, typeof(LongPeg), "_beingHit", true);
            Require(!conversion.MoveNext(), "Conversion did not finish");
            Require(peg.pegType == type && peg.isBonusSpecialPeg && !peg.IsDisabled(), "Special peg was not restored");
            if (peg is RegularPeg rp)
            {
                var renderer = go.GetComponent<SpriteRenderer>();
                Require(renderer.enabled && renderer.sprite != null && renderer.color.a > 0, "R/C sprite is invisible");
                var sequence = (Sequence)typeof(RegularPeg).GetField("_destructionSequence", PrivateInstance).GetValue(rp);
                var specialSprite = renderer.sprite;
                if (sequence != null && sequence.active)
                {
                    sequence.SetUpdate(UpdateType.Manual);
                    DOTween.ManualUpdate(1f, 1f);
                }
                Require(renderer.sprite == specialSprite, "Pop animation can still overwrite the special sprite");
            }
            else
            {
                var lp = (LongPeg)peg;
                Require(!lp.hit && !manager.delayedDeathPegs.Contains(lp), "Long peg retains delayed death");
                Require(!(bool)typeof(LongPeg).GetField("_beingHit", PrivateInstance).GetValue(lp), "Long peg retains old contact timer");
                Require(go.GetComponentsInChildren<SpriteRenderer>().Any(r => r.enabled && r.sprite != null), "Long peg R/C marker missing");
            }
            // A legitimately destroyed target must never be revived by the deferred callback.
            var destroyed = (IEnumerator)typeof(ConvertHitPegs).GetMethod("ConvertSpecialPegAfterHit", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { peg, type });
            destroyed.MoveNext();
            peg.pegType = Peg.PegType.DESTROYED;
            destroyed.MoveNext();
            Require(peg.pegType == Peg.PegType.DESTROYED, "Destroyed target was resurrected");
            results.AppendLine("PASS " + (useLongPeg ? "long" : "round") + " " + type + " callback=" + (orbCallbackFirst ? "orb-first (legacy failure reproduced)" : "peg-first"));
            UnityEngine.Object.DestroyImmediate(go);
        }
        UnityEngine.Object.DestroyImmediate(controllerObject);
        UnityEngine.Object.DestroyImmediate(relics);
        File.WriteAllText("E:/NEWTEST/PeglinBrowserLab/Reports/refresh-peg-regression.txt", results.ToString());
        Debug.Log("REFRESH_REGRESSION_DONE " + results);
    }

    public static void Inspect()
    {
        var report = new StringBuilder();
        foreach (var path in new[] { "Assets/Prefabs/RegularPeg.prefab", "Assets/Prefabs/RegularPegMovable.prefab", "Assets/Resources_moved/Prefabs/FlickerPeg.prefab", "Assets/Prefabs/LongPeg.prefab" })
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var peg = prefab.GetComponent<Peg>();
            report.AppendLine(path);
            var so = new SerializedObject(peg);
            foreach (var field in new[] { "_regularSprite", "_resetSprite", "_persistantResetSprite", "_refreshMaterial", "_persistentRefreshMaterial" })
            {
                var property = so.FindProperty(field);
                if (property == null) continue;
                var obj = property.objectReferenceValue;
                report.AppendLine(field + "=" + (obj == null ? "NULL" : AssetDatabase.GetAssetPath(obj)));
                if (obj is Sprite sprite)
                    report.AppendLine(" texture=" + sprite.texture.name + " bounds=" + sprite.bounds + " uv=" + string.Join(";", sprite.uv.Select(v => v.ToString("F3"))));
                if (obj is Material material)
                    report.AppendLine(" shader=" + material.shader.name + " supported=" + material.shader.isSupported);
            }
        }
        File.WriteAllText("E:/NEWTEST/PeglinBrowserLab/Reports/refresh-peg-inspection.txt", report.ToString());
        Debug.Log("REFRESH_INSPECT_DONE " + report);
    }
}
