using Loading;
using Loading.Pegboards;
using Map;
using Peglin.ClassSystem;
using PeglinUI.LoadoutManager;
using Relics;
using Stats;
using UnityEngine;
using Worldmap;

public static class StaticGameData
{
	public static MapNode currentNode = null;

	public static int chosenNextNodeIndex = 0;

	public static MapNode specificNodeOverride = null;

	public static MapData dataToLoad = null;

	public static PeglinSceneLoader.Scene activeMapScene = PeglinSceneLoader.Scene.NONE;

	public static bool loadNextScene = false;

	public static PeglinSceneLoader.Scene battleVictorySceneOverride = PeglinSceneLoader.Scene.NONE;

	public static RelicRarity relicRarityOverride = RelicRarity.NONE;

	public static int totalFloorCount = 0;

	public static int mimicsBattled = 0;

	public static bool hasReachedBoss = false;

	public static PegLayoutLoader.PreInstancedPegboardData preInstancedPegboardData = null;

	public static bool seedSet = false;

	public static string currentSeed = string.Empty;

	public static Random.State nextMapSeedState;

	public static SeededMapNodeData seededNodeData;

	public static Relic[] StartingRelics = null;

	public static GameObject[] StartingOrbs = null;

	public static int StoreNumberOfOrbsRemoved = 0;

	public static Vector2 battleXBounds = new Vector2(-8f, 8f);

	public static Vector2 battleYBounds = new Vector2(-9.5f, 5f);

	public static Vector2 act4BossPhase2TallerYBounds = new Vector2(-9.5f, 11.25f);

	public static ClassLoadoutPairs[] classLoadouts;

	public static Class chosenClass = Class.Peglin;

	public static RunStats CurrentRunStats;
}
