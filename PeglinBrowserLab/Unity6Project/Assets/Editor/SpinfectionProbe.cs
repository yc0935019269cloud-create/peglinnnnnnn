using System;
using System.IO;
using System.Reflection;
using System.Text;
using Battle.Attacks.AttackBehaviours;
using Battle.Enemies;
using Battle.StatusEffects;
using I2.Loc;
using Relics;
using UnityEditor;
using UnityEngine;

public static class SpinfectionProbe
{
    private static void SetRelics(Enemy enemy, RelicManager relics)
    {
        typeof(Enemy).GetField("_relicManager", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(enemy, relics);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static void Validate()
    {
        var results = new StringBuilder();
        results.AppendLine("Starting Spinfection & Slime Boss regression validation...");
        var relics = ScriptableObject.CreateInstance<RelicManager>();

        // 1. Test CauldronBubball-Lv3 AddStatusEffectOnHit logic & parameters
        var cauldronPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameObject/CauldronBubball-Lv3.prefab");
        Require(cauldronPrefab != null, "CauldronBubball-Lv3.prefab not found");
        var cauldronHit = cauldronPrefab.GetComponent<AddStatusEffectOnHit>();
        Require(cauldronHit != null, "AddStatusEffectOnHit missing on CauldronBubball-Lv3");
        Require(cauldronHit.type == StatusEffectType.Poison, "CauldronBubball status type should be Poison");
        Require(cauldronHit.intensity == 3, "CauldronBubball-Lv3 base intensity should be 3");

        var cauldronParams = cauldronPrefab.GetComponent<LocalizationParamsManager>();
        Require(cauldronParams != null, "LocalizationParamsManager missing on CauldronBubball-Lv3");
        string critAddVal = cauldronParams.GetParameterValue("STATUS_EFFECT_CRIT_ADD");
        string critAmtVal = cauldronParams.GetParameterValue("STATUS_EFFECT_CRIT_AMOUNT");
        Require(critAddVal == "3", $"CauldronBubball-Lv3 STATUS_EFFECT_CRIT_ADD should be 3, got: {critAddVal}");
        Require(critAmtVal == "6", $"CauldronBubball-Lv3 STATUS_EFFECT_CRIT_AMOUNT should be 6, got: {critAmtVal}");
        results.AppendLine("PASS: CauldronBubball-Lv3 parameter validation (base=3, critAdd=3, critTotal=6)");

        // 2. Test battle application logic (simulated Enemy target)
        var dummyEnemyGo = new GameObject("DummyEnemy");
        var dummyEnemy = dummyEnemyGo.AddComponent<Enemy>();
        SetRelics(dummyEnemy, relics);
        
        // Non-crit hit
        cauldronHit.AffectEnemy(dummyEnemy, relics, 0);
        var poisonEffect = dummyEnemy.GetEffect(StatusEffectType.Poison);
        Require(poisonEffect != null && poisonEffect.Intensity == 3, $"Expected 3 poison on non-crit, got {poisonEffect?.Intensity}");
        results.AppendLine("PASS: CauldronBubball-Lv3 non-crit hit applies 3 Spinfection stacks");

        // Crit hit (double stacks)
        var dummyEnemyCritGo = new GameObject("DummyEnemyCrit");
        var dummyEnemyCrit = dummyEnemyCritGo.AddComponent<Enemy>();
        SetRelics(dummyEnemyCrit, relics);
        cauldronHit.AffectEnemy(dummyEnemyCrit, relics, 1);
        var poisonCritEffect = dummyEnemyCrit.GetEffect(StatusEffectType.Poison);
        Require(poisonCritEffect != null && poisonCritEffect.Intensity == 6, $"Expected 6 poison on crit, got {poisonCritEffect?.Intensity}");
        results.AppendLine("PASS: CauldronBubball-Lv3 crit hit applies 6 Spinfection stacks (2x base)");

        // 3. Test PoisonOrb-Lv3 crit hit (base 5 -> crit 10)
        var poisonOrbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameObject/PoisonOrb-Lvl3.prefab");
        Require(poisonOrbPrefab != null, "PoisonOrb-Lvl3.prefab not found");
        var poisonOrbHit = poisonOrbPrefab.GetComponent<AddStatusEffectOnHit>();
        Require(poisonOrbHit.intensity == 5, "PoisonOrb-Lvl3 intensity should be 5");
        var dummyPoisonOrbGo = new GameObject("DummyPoisonOrbEnemy");
        var dummyPoisonOrbEnemy = dummyPoisonOrbGo.AddComponent<Enemy>();
        SetRelics(dummyPoisonOrbEnemy, relics);
        poisonOrbHit.AffectEnemy(dummyPoisonOrbEnemy, relics, 1);
        var poisonOrbCritEffect = dummyPoisonOrbEnemy.GetEffect(StatusEffectType.Poison);
        Require(poisonOrbCritEffect != null && poisonOrbCritEffect.Intensity == 10, $"Expected 10 poison on crit for PoisonOrb-Lv3, got {poisonOrbCritEffect?.Intensity}");
        results.AppendLine("PASS: PoisonOrb-Lv3 crit hit applies 10 Spinfection stacks (2x base)");

        // 4. Test Slime Boss TriggerStatusEffectsOnExtraTurn
        var slimeBossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Bosses/SlimeBossPrefab.prefab");
        Require(slimeBossPrefab != null, "SlimeBossPrefab not found");
        var extraTrigger = slimeBossPrefab.GetComponent<TriggerStatusEffectsOnExtraTurn>();
        Require(extraTrigger != null, "TriggerStatusEffectsOnExtraTurn missing on SlimeBossPrefab");

        // Set up enemy with 5 poison stacks
        var testBossGo = UnityEngine.Object.Instantiate(slimeBossPrefab);
        var testBoss = testBossGo.GetComponent<Enemy>();
        SetRelics(testBoss, relics);
        testBoss.ApplyStatusEffect(new StatusEffect(StatusEffectType.Poison, 5), StatusEffectSource.PLAYER);
        Require(testBoss.GetEffect(StatusEffectType.Poison).Intensity == 5, "Boss should have 5 poison stacks initially");

        // Call DoExtraTurnBehavior - it should NOT resolve/decrement status effects
        extraTrigger.DoExtraTurnBehavior(testBoss);
        Require(testBoss.GetEffect(StatusEffectType.Poison).Intensity == 5, $"Boss poison should remain 5 after extra turn action, but got {testBoss.GetEffect(StatusEffectType.Poison).Intensity}");
        results.AppendLine("PASS: Slime Boss TriggerStatusEffectsOnExtraTurn does not cause duplicate decay");

        // Cleanup test objects
        UnityEngine.Object.DestroyImmediate(dummyEnemyGo);
        UnityEngine.Object.DestroyImmediate(dummyEnemyCritGo);
        UnityEngine.Object.DestroyImmediate(dummyPoisonOrbGo);
        UnityEngine.Object.DestroyImmediate(testBossGo);

        // Write report
        string reportDir = "E:/NEWTEST/PeglinBrowserLab/Reports";
        Directory.CreateDirectory(reportDir);
        string reportPath = Path.Combine(reportDir, "spinfection-regression.txt");
        File.WriteAllText(reportPath, results.ToString());
        Debug.Log("SpinfectionProbe completed successfully:\n" + results);
    }
}
