using System;
using System.Collections.Generic;
using System.Linq;
using Battle;
using Battle.Attacks;
using Battle.Attacks.AttackModifications;
using Battle.Attacks.Interfaces;
using Battle.Pachinko;
using Battle.Pachinko.BallBehaviours;
using Battle.StatusEffects;
using Challenges;
using ClassSystem;
using Cruciball;
using Currency;
using Data.Deck;
using Loading;
using Map;
using MoreMountains.Tools;
using NaughtyAttributes;
using Newtonsoft.Json;
using Peglin.Achievements;
using Peglin.ClassSystem;
using PeglinUI;
using PeglinUI.LoadoutManager;
using PeglinUtils;
using PixelCrushers.DialogueSystem;
using Relics;
using Saving;
using Seeding;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DeckManager : ScriptableObject
{
	public delegate void BallDrawn(GameObject ball);

	public delegate void PersistBallUsed();

	public delegate void BallUpgraded(GameObject previous, GameObject post);

	public delegate void DeckEmpty();

	public delegate void Shuffled(int deckSize);

	private class DeckManagerSaveData : SaveObjectData
	{
		public static readonly string KEY = "DeckManager";

		[JsonProperty]
		[SerializeField]
		private string[] _prefabNames;

		[JsonProperty]
		[SerializeField]
		private string[] _startingDeck;

		[JsonProperty]
		[SerializeField]
		private int _class;

		[JsonProperty]
		[SerializeField]
		private string[] _unlimitedUpgradePrefabNames;

		[JsonProperty]
		[SerializeField]
		private int[] _unlimitedUpgradeUpgradeLevels;

		[JsonProperty]
		[SerializeField]
		private int[] _assemballs;

		[JsonProperty]
		[SerializeField]
		private string[] _commonOrbNamesQueue;

		[JsonProperty]
		[SerializeField]
		private string[] _uncommonOrbNamesQueue;

		[JsonProperty]
		[SerializeField]
		private string[] _rareOrbNamesQueue;

		[JsonProperty]
		[SerializeField]
		private string[] _haglinsSatchelOrbNames;

		[JsonProperty]
		[SerializeField]
		private float[] _haglinsSatchelUpgradeRolls;

		[JsonProperty]
		[SerializeField]
		private UnityEngine.Random.State _assemballRerollRNGState;

		public override string Name => KEY;

		[JsonIgnore]
		public string[] PrefabNames
		{
			get
			{
				return _prefabNames;
			}
			set
			{
				_prefabNames = value;
			}
		}

		[JsonIgnore]
		public string[] StartingOrbs
		{
			get
			{
				return _startingDeck;
			}
			set
			{
				_startingDeck = value;
			}
		}

		[JsonIgnore]
		public Class Class
		{
			get
			{
				return (Class)_class;
			}
			set
			{
				_class = (int)value;
			}
		}

		[JsonIgnore]
		public string[] UnlimitedUpgradePrefabNames
		{
			get
			{
				return _unlimitedUpgradePrefabNames;
			}
			set
			{
				_unlimitedUpgradePrefabNames = value;
			}
		}

		[JsonIgnore]
		public int[] UnlimitedUpgradeUpgradeLevels
		{
			get
			{
				return _unlimitedUpgradeUpgradeLevels;
			}
			set
			{
				_unlimitedUpgradeUpgradeLevels = value;
			}
		}

		[JsonIgnore]
		public int[] Assemballs => _assemballs;

		[JsonIgnore]
		public string[] CommonOrbNamesQueue => _commonOrbNamesQueue;

		[JsonIgnore]
		public string[] UncommonOrbNamesQueue => _uncommonOrbNamesQueue;

		[JsonIgnore]
		public string[] RareOrbNamesQueue => _rareOrbNamesQueue;

		[JsonIgnore]
		public string[] HaglinsSatchelOrbNames => _haglinsSatchelOrbNames;

		[JsonIgnore]
		public float[] HaglinsSatchelUpgradeRolls => _haglinsSatchelUpgradeRolls;

		[JsonIgnore]
		public UnityEngine.Random.State AssemballRerollRNGState => _assemballRerollRNGState;

		public DeckManagerSaveData()
			: base(deleteOnLose: true, DataSerializer.SaveType.RUN)
		{
		}

		public DeckManagerSaveData(string[] prefabNames, string[] startingDeck, Class cl, string[] unlimitedUpgradePrefabNames, int[] unlimitedUpgradeUpgradeLevels, int[] assemballs, string[] commonOrbNamesQueue, string[] uncommonOrbNamesQueue, string[] rareOrbNamesQueue, string[] haglinsSatchelOrbNames, float[] haglinsSatchelUpgradeRolls, UnityEngine.Random.State assemballRerollRNGState)
			: base(deleteOnLose: true, DataSerializer.SaveType.RUN)
		{
			_prefabNames = prefabNames;
			_startingDeck = startingDeck;
			Class = cl;
			_unlimitedUpgradeUpgradeLevels = unlimitedUpgradeUpgradeLevels;
			_unlimitedUpgradePrefabNames = unlimitedUpgradePrefabNames;
			_assemballs = assemballs;
			_commonOrbNamesQueue = commonOrbNamesQueue;
			_uncommonOrbNamesQueue = uncommonOrbNamesQueue;
			_rareOrbNamesQueue = rareOrbNamesQueue;
			_haglinsSatchelOrbNames = haglinsSatchelOrbNames;
			_haglinsSatchelUpgradeRolls = haglinsSatchelUpgradeRolls;
			_assemballRerollRNGState = assemballRerollRNGState;
		}
	}

	public static BallDrawn onBallDrawn = delegate
	{
	};

	public static BallDrawn onBallUsed = (GameObject ball) =>
	{
	};

	public static PersistBallUsed onPersistBallUsed = () =>
	{
	};

	public static BallUpgraded onBallUpgraded = delegate
	{
	};

	public static DeckEmpty onDeckEmpty = () =>
	{
	};

	public static Shuffled onDeckShuffled = delegate
	{
	};

	public static Action onDeckSizeChanged = () =>
	{
	};

	[Header("References")]
	[SerializeField]
	private RelicManager _relicManager;

	[SerializeField]
	private SecretSeedManager _secretSeedManager;

	public CompleteDeckOrbContainer completeDeckOrbContainerPrefab;

	private CompleteDeckOrbContainer completeDeckOrbContainerInstance;

	public BattleDeckOrbContainer battleDeckOrbContainerPrefab;

	private BattleDeckOrbContainer battleDeckOrbContainerInstance;

	public OrbPool OrbPool;

	public GameObject assemballPrefab;

	public List<GameObject> assemballParts;

	private List<GameObject> AllCommonOrbs = new List<GameObject>();

	private List<GameObject> AllUncommonOrbs = new List<GameObject>();

	private List<GameObject> AllRareOrbs = new List<GameObject>();

	private Queue<GameObject> AllCommonOrbsRandomQueue = new Queue<GameObject>();

	private Queue<GameObject> AllUncommonOrbsRandomQueue = new Queue<GameObject>();

	private Queue<GameObject> AllRareOrbsRandomQueue = new Queue<GameObject>();

	private Class _selectedClass;

	[Header("Debug Fields")]
	[SerializeField]
	private DeckData _debugDeck;

	public static List<GameObject> completeDeck = null;

	[Header("Runtime Values")]
	[ReadOnly]
	public List<GameObject> battleDeck;

	[NonSerialized]
	public Stack<GameObject> shuffledDeck = new Stack<GameObject>();

	private SeededOrbAndRarity[] haglinsSatchelSeededOrbs;

	private float[] haglinsSatchelSeededUpgradeRolls;

	private UnityEngine.Random.State _assemballRerollRNGState;

	[HideInInspector]
	public SerializableRandomState deckOrderRandomState;

	private Dictionary<string, GameObject> _dialogueOrbMapping;

	public static int DeckSize
	{
		get
		{
			if (completeDeck != null)
			{
				return completeDeck.Count;
			}
			return 0;
		}
	}

	public Queue<GameObject> CurrentCommonOrbQueue => AllCommonOrbsRandomQueue;

	public Queue<GameObject> CurrentUncommonOrbQueue => AllUncommonOrbsRandomQueue;

	public Queue<GameObject> CurrentRareOrbQueue => AllRareOrbsRandomQueue;

	public List<GameObject> CommonOrbPool { get; protected set; }

	public List<GameObject> UncommonOrbPool { get; protected set; }

	public List<GameObject> RareOrbPool { get; protected set; }

	public List<GameObject> SpecialOrbPool { get; protected set; }

	public List<GameObject> UnavailableOrbPool { get; protected set; }

	public void SetupClassOrbPools(Class cl)
	{
		if (CommonOrbPool == null)
		{
			List<GameObject> list = (CommonOrbPool = new List<GameObject>());
		}
		if (UncommonOrbPool == null)
		{
			List<GameObject> list = (UncommonOrbPool = new List<GameObject>());
		}
		if (RareOrbPool == null)
		{
			List<GameObject> list = (RareOrbPool = new List<GameObject>());
		}
		if (SpecialOrbPool == null)
		{
			List<GameObject> list = (SpecialOrbPool = new List<GameObject>());
		}
		if (UnavailableOrbPool == null)
		{
			List<GameObject> list = (UnavailableOrbPool = new List<GameObject>());
		}
		CommonOrbPool.Clear();
		UncommonOrbPool.Clear();
		RareOrbPool.Clear();
		SpecialOrbPool.Clear();
		UnavailableOrbPool.Clear();
		PopulateOrbPools(cl);
		_selectedClass = cl;
	}

	public void InstantiateDeck(List<GameObject> orbs = null)
	{
		if (completeDeck == null)
		{
			completeDeck = new List<GameObject>();
		}
		else
		{
			completeDeck.Clear();
		}
		if (battleDeck == null)
		{
			battleDeck = new List<GameObject>();
		}
		else
		{
			battleDeck.Clear();
		}
		if (completeDeckOrbContainerInstance == null)
		{
			completeDeckOrbContainerInstance = UnityEngine.Object.Instantiate(completeDeckOrbContainerPrefab);
			UnityEngine.Object.DontDestroyOnLoad(completeDeckOrbContainerInstance);
		}
		else
		{
			completeDeckOrbContainerInstance.transform.MMDestroyAllChildren();
		}
		if (orbs == null)
		{
			return;
		}
		foreach (GameObject orb in orbs)
		{
			AssemballComponent component = orb.GetComponent<AssemballComponent>();
			if (component != null)
			{
				FindAndResolveAssemball(component.type);
			}
			else
			{
				AddOrbToDeckSilent(orb);
			}
		}
	}

	public void SafeDebugInstantiateDeck(Class cl)
	{
		if (completeDeck == null)
		{
			InstantiateDeck(_debugDeck.Balls);
			SetupClassOrbPools(cl);
		}
		if (battleDeck == null)
		{
			battleDeck = new List<GameObject>();
		}
	}

	public void ShuffleCompleteDeck(bool fromComplete = true)
	{
		if ((completeDeck == null || completeDeck.Count == 0) && Debug.isDebugBuild)
		{
			Debug.Log("ShuffleCompleteDeck called without any deck information. Using DebugDeck. " + completeDeck?.ToString() + " len: " + completeDeck?.Count);
			InstantiateDeck(_debugDeck.Balls);
		}
		GameObject[] array = (fromComplete ? completeDeck.ToArray() : battleDeck.ToArray());
		int num = array.Length;
		for (int i = 0; i < num - 1; i++)
		{
			int num2 = i + deckOrderRandomState.GetRandomRangeValue(0, num - i);
			ref GameObject reference = ref array[num2];
			ref GameObject reference2 = ref array[i];
			GameObject gameObject = array[i];
			GameObject gameObject2 = array[num2];
			reference = gameObject;
			reference2 = gameObject2;
		}
		List<GameObject> list = new List<GameObject>();
		shuffledDeck.Clear();
		if (fromComplete)
		{
			if (battleDeck == null)
			{
				battleDeck = new List<GameObject>();
			}
			if (battleDeckOrbContainerInstance == null)
			{
				battleDeckOrbContainerInstance = UnityEngine.Object.Instantiate(battleDeckOrbContainerPrefab);
			}
			Transform transform = battleDeckOrbContainerInstance.transform;
			transform.MMDestroyAllChildren();
			battleDeck.Clear();
			GameObject[] array2 = array;
			foreach (GameObject obj in array2)
			{
				GameObject gameObject3 = UnityEngine.Object.Instantiate(obj, transform);
				UnlimitedUpgrades component = obj.GetComponent<UnlimitedUpgrades>();
				if (component != null)
				{
					gameObject3.GetComponent<UnlimitedUpgrades>().timesUpgraded = component.timesUpgraded;
				}
				battleDeck.Add(gameObject3);
				GameObject gameObject4 = PushOrbToShuffleDeck(gameObject3, fromComplete);
				if (gameObject4 != null)
				{
					list.Add(gameObject4);
				}
			}
		}
		else
		{
			GameObject[] array2 = array;
			foreach (GameObject orb in array2)
			{
				GameObject gameObject5 = PushOrbToShuffleDeck(orb, fromComplete);
				if (gameObject5 != null)
				{
					list.Add(gameObject5);
				}
			}
		}
		BattleController battleController = null;
		if (list.Count > 0)
		{
			battleController = UnityEngine.Object.FindObjectOfType<BattleController>();
		}
		foreach (GameObject item in list)
		{
			if (battleController != null)
			{
				battleController.InitializeOrbAttack(item.GetComponent<Attack>());
			}
		}
		onDeckShuffled(shuffledDeck.Count);
	}

	private GameObject PushOrbToShuffleDeck(GameObject orb, bool useCompleteDeck)
	{
		GameObject result = null;
		GameObject gameObject = orb;
		if (orb.TryGetComponent<CopyOrbOnReload>(out var component))
		{
			GameObject gameObject2 = null;
			int num = 20;
			while (gameObject2 == null && num > 0)
			{
				GameObject gameObject3 = (useCompleteDeck ? completeDeck[UnityEngine.Random.Range(0, completeDeck.Count)] : battleDeck[UnityEngine.Random.Range(0, battleDeck.Count)]);
				if (gameObject3.GetComponent<CopyOrbOnReload>() == null)
				{
					gameObject2 = gameObject3;
				}
				num--;
			}
			if (gameObject2 == null)
			{
				Debug.LogWarning("Unable to copy an orb, maybe the user has only Mirrorbs?");
				return null;
			}
			gameObject = UnityEngine.Object.Instantiate(gameObject2, battleDeckOrbContainerInstance.transform);
			Attack component2 = gameObject.GetComponent<Attack>();
			(component.copiedOrb = gameObject.GetComponent<PachinkoBall>()).SetAsMirrorbCopiedOrb();
			component2.SetInstanceId();
			UnlimitedUpgrades component3 = gameObject.GetComponent<UnlimitedUpgrades>();
			if (component3 != null)
			{
				component3.timesUpgraded = gameObject2.GetComponent<UnlimitedUpgrades>().timesUpgraded;
			}
			result = gameObject;
		}
		PersistentOrb component4 = gameObject.GetComponent<PersistentOrb>();
		if (component4 != null)
		{
			component4.ResetPersistenceLevel();
		}
		shuffledDeck.Push(gameObject);
		return result;
	}

	public void ShuffleBattleDeck()
	{
		ShuffleCompleteDeck(fromComplete: false);
	}

	public void ClearCurrentPersistCharges(PersistentOrb currentPersist)
	{
		if (shuffledDeck.Count > 0 && currentPersist.remainingPersistence >= 0 && currentPersist.remainingPersistence < currentPersist.modifiedPersistLevel)
		{
			shuffledDeck.Pop().GetComponent<PersistentOrb>().ResetPersistenceLevel();
		}
	}

	public GameObject DrawBall(List<AttackModifier> _nextAttackModifiers)
	{
		if (shuffledDeck == null || shuffledDeck.Count == 0)
		{
			return null;
		}
		GameObject gameObject = shuffledDeck.Peek();
		if (_nextAttackModifiers.Count != 0)
		{
			foreach (AttackModifier _nextAttackModifier in _nextAttackModifiers)
			{
				_nextAttackModifier.ApplyStatModificationsToOrb(gameObject);
			}
		}
		PersistentOrb component = gameObject.GetComponent<PersistentOrb>();
		if (component != null && component.remainingPersistence >= 0)
		{
			if (component.remainingPersistence == component.modifiedPersistLevel)
			{
				onBallUsed?.Invoke(gameObject);
			}
			else
			{
				onPersistBallUsed?.Invoke();
			}
			component.DecreasePersistenceLevel();
			if (component.remainingPersistence < 0)
			{
				shuffledDeck.Pop();
			}
		}
		else
		{
			onBallUsed?.Invoke(gameObject);
			shuffledDeck.Pop();
		}
		onBallDrawn?.Invoke(gameObject);
		if (shuffledDeck.Count == 0)
		{
			onDeckEmpty();
		}
		return gameObject;
	}

	public void AddOrbToDeck(GameObject orbPrefab)
	{
		AssemballComponent component = orbPrefab.GetComponent<AssemballComponent>();
		AssemballParent component2 = orbPrefab.GetComponent<AssemballParent>();
		if (component != null)
		{
			PersistentPlayerData.Instance.SetOrbUnlocked(orbPrefab.GetComponent<Attack>().locNameString);
			FindAndResolveAssemball(component.type);
		}
		else if (component2 != null)
		{
			GameObject gameObject = AddOrbToDeckSilent(assemballPrefab);
			foreach (AssemballType value in Enum.GetValues(typeof(AssemballType)))
			{
				if (value != AssemballType.NONE && component2.assemballComponents.HasFlag(value))
				{
					gameObject.GetComponent<AssemballParent>().TryApplyComponent(value);
					if (StaticGameData.CurrentRunStats != null)
					{
						StaticGameData.CurrentRunStats.AddAssemballToDeck(gameObject.GetComponent<Attack>(), value);
					}
				}
			}
		}
		else
		{
			GameObject gameObject2 = AddOrbToDeckSilent(orbPrefab);
			UnlimitedUpgrades component3 = orbPrefab.GetComponent<UnlimitedUpgrades>();
			if (component3 != null)
			{
				gameObject2.GetComponent<UnlimitedUpgrades>().timesUpgraded = component3.timesUpgraded;
			}
			Attack component4 = orbPrefab.GetComponent<Attack>();
			if (component4.HasUpgrade && _relicManager.AttemptUseRelic(RelicEffect.UPGRADE_ADDED_ORBS))
			{
				UpgradeSpecificOrb(gameObject2);
				PlayerHealthController.OnExternalDamageRequested?.Invoke(6f, PlayerHealthController.DamageSource.OTHER_SELF);
			}
			else if (StaticGameData.CurrentRunStats != null)
			{
				if (component3 != null || component4.locNameString.Equals("orbbery"))
				{
					StaticGameData.CurrentRunStats.AddUnlimitedUpgradesOrbToDeck(component4, component3);
				}
				else
				{
					StaticGameData.CurrentRunStats.AddOrbToDeck(component4);
				}
			}
		}
		if (_relicManager.AttemptUseRelic(RelicEffect.MAX_HP_ON_ADD_ORB))
		{
			PlayerHealthController.OnExternalMaxHealthAdjustmentRequested?.Invoke(3f);
		}
		onDeckSizeChanged?.Invoke();
		if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.REMOVE_ORB_AFTER_ADDING))
		{
			InventoryViewController.OnExternalForceRemoveOrbRequest?.Invoke();
		}
	}

	private GameObject AddOrbToDeckSilent(GameObject orbPrefab)
	{
		PersistentPlayerData.Instance.SetOrbUnlocked(orbPrefab.GetComponent<Attack>().locNameString);
		if (completeDeckOrbContainerInstance != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(orbPrefab, completeDeckOrbContainerInstance.transform);
			Attack component = gameObject.GetComponent<Attack>();
			gameObject.name = orbPrefab.name;
			component.SetInstanceId();
			completeDeck.Add(gameObject);
			return gameObject;
		}
		return null;
	}

	public void AddOrbToStarterDeck(GameObject orbPrefab)
	{
		AddOrbToDeckSilent(orbPrefab);
		StaticGameData.CurrentRunStats.AddOrbToDeck(orbPrefab.GetComponent<Attack>());
	}

	private void CheckBattleDeckEmpty()
	{
	}

	private void CheckCompleteDeckEmpty()
	{
	}

	public void RemoveRandomOrbFromDeck()
	{
		List<GameObject> list = new List<GameObject>(completeDeck.ToArray());
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].GetComponent<CannotBeRemoved>() != null)
			{
				list.RemoveAt(num);
			}
		}
		if (list.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			GameObject gameObject = list[index];
			Attack component = gameObject.GetComponent<Attack>();
			DialogueLua.SetVariable("orb0", component.GetNameWithLevel());
			GenerateRoughGoldAmountForRemovedOrbRarity(component.locName);
			RemoveSpecifiedOrbFromDeck(gameObject);
		}
	}

	private void GenerateRoughGoldAmountForRemovedOrbRarity(string attackName)
	{
		int num = -1;
		foreach (GameObject item in UncommonOrbPool)
		{
			if (item.GetComponent<Attack>().locName == attackName)
			{
				num = UnityEngine.Random.Range(8, 13);
				break;
			}
		}
		if (num < 0)
		{
			foreach (GameObject item2 in RareOrbPool)
			{
				if (item2.GetComponent<Attack>().locName == attackName)
				{
					num = UnityEngine.Random.Range(14, 22);
					break;
				}
			}
		}
		if (num < 0)
		{
			num = UnityEngine.Random.Range(4, 8);
		}
		DialogueLua.SetVariable("goldToGrant", num);
	}

	public void SoftRemoveSpecifiedOrbFromDeck(GameObject orb, bool deepRemove = false)
	{
		if (deepRemove)
		{
			RemoveSpecifiedOrbFromDeck(orb);
		}
		else if (orb.GetComponent<CannotBeRemoved>() == null && completeDeck.Remove(orb))
		{
			CheckCompleteDeckEmpty();
			onDeckSizeChanged?.Invoke();
		}
	}

	public void RemoveSpecifiedOrbFromDeck(GameObject orb, bool addToStats = true, bool ignoreCannotBeRemovedOrbs = false)
	{
		if (orb.GetComponent<CannotBeRemoved>() != null && !ignoreCannotBeRemovedOrbs)
		{
			return;
		}
		bool num = completeDeck.Remove(orb);
		CleanUpOrbRemoval(orb, addToStats);
		if (num)
		{
			return;
		}
		Attack component = orb.GetComponent<Attack>();
		foreach (GameObject item in completeDeck)
		{
			if (Attack.IsEquivalent(item.GetComponent<Attack>(), component))
			{
				completeDeck.Remove(item);
				CleanUpOrbRemoval(item, addToStats);
				return;
			}
		}
		Debug.LogWarning("Attempt to remove " + component.GetNameWithLevel() + " from completeDeck failed.");
	}

	private void CleanUpOrbRemoval(GameObject removedOrb, bool addToStats)
	{
		if (addToStats && removedOrb.TryGetComponent<AddGoldWhenOrbRemoved>(out var component))
		{
			CurrencyManager.Instance.AddGold(component.goldToAdd);
		}
		if (addToStats && StaticGameData.CurrentRunStats != null)
		{
			StaticGameData.CurrentRunStats.AddOrbRemoval(removedOrb.GetComponent<Attack>());
		}
		CheckCompleteDeckEmpty();
		onDeckSizeChanged?.Invoke();
	}

	public void RemoveAllOwnedHorriballs()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject item in completeDeck)
		{
			if (item.GetComponent<Attack>().IsHorriball)
			{
				list.Add(item);
			}
		}
		foreach (GameObject item2 in list)
		{
			RemoveSpecifiedOrbFromDeck(item2, addToStats: true, ignoreCannotBeRemovedOrbs: true);
		}
	}

	public bool IsOrbInDeck(string orbLocStringName)
	{
		foreach (GameObject item in completeDeck)
		{
			if (item.GetComponent<Attack>().locNameString == orbLocStringName)
			{
				return true;
			}
		}
		return false;
	}

	public void FindAndResolveAssemball(AssemballType type)
	{
		foreach (GameObject item in completeDeck)
		{
			AssemballParent component = item.GetComponent<AssemballParent>();
			if (component != null && component.TryApplyComponent(type))
			{
				if (StaticGameData.CurrentRunStats != null)
				{
					StaticGameData.CurrentRunStats.AddAssemballToDeck(component.GetComponent<Attack>(), type);
				}
				return;
			}
		}
		GameObject gameObject = AddOrbToDeckSilent(assemballPrefab);
		if (gameObject.GetComponent<AssemballParent>().TryApplyComponent(type) && StaticGameData.CurrentRunStats != null)
		{
			StaticGameData.CurrentRunStats.AddAssemballToDeck(gameObject.GetComponent<Attack>(), type);
		}
	}

	public void RemoveOrbFromBattleDeck(GameObject orb)
	{
		foreach (GameObject item in battleDeck)
		{
			Attack component = item.GetComponent<Attack>();
			Attack component2 = orb.GetComponent<Attack>();
			if (Attack.IsEquivalent(component, component2))
			{
				battleDeck.Remove(item);
				CheckBattleDeckEmpty();
				break;
			}
		}
	}

	public void ModifyStatsOnFire(GameObject ballFired)
	{
		Attack component = ballFired.GetComponent<Attack>();
		GameObject gameObject = null;
		foreach (GameObject item in battleDeck)
		{
			Attack component2 = item.GetComponent<Attack>();
			if (component.instanceID == component2.instanceID)
			{
				gameObject = item;
				break;
			}
		}
		if (gameObject != null)
		{
			ApplyAllFireStatModsToOrb(gameObject);
		}
		foreach (GameObject item2 in completeDeck)
		{
			Attack component3 = item2.GetComponent<Attack>();
			if (component.instanceID == component3.instanceID)
			{
				gameObject = item2;
				break;
			}
		}
		if (!(gameObject != null))
		{
			return;
		}
		IModifiesPermanentStats[] components = gameObject.GetComponents<IModifiesPermanentStats>();
		if (components != null && components.Length != 0)
		{
			IModifiesPermanentStats[] array = components;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ModifyStatsOnFire();
			}
		}
	}

	private void ApplyAllFireStatModsToOrb(GameObject orb)
	{
		IModifiesBattleStats[] componentsInChildren = orb.GetComponentsInChildren<IModifiesBattleStats>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			IModifiesBattleStats[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ModifyStatsOnFire();
			}
		}
	}

	public List<GameObject> GetUpgradeableOrbs()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject item in completeDeck)
		{
			Attack component = item.GetComponent<Attack>();
			if (component != null && component.HasUpgrade)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<GameObject> GetUpgradeableAndRemovableOrbs()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject item in completeDeck)
		{
			Attack component = item.GetComponent<Attack>();
			if (component != null && component.HasUpgrade && item.GetComponent<CannotBeRemoved>() == null)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<GameObject> GetRemovableOrbs()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject item in completeDeck)
		{
			if (item.GetComponent<CannotBeRemoved>() == null)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<GameObject> GetShuffledUpgradeableOrbs()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject item in shuffledDeck)
		{
			Attack component = item.GetComponent<Attack>();
			if (component != null && component.HasUpgrade)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<GameObject> GetOrbsForParticularLevel(int level)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject item in completeDeck)
		{
			if (item.GetComponent<Attack>().Level == level)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<GameObject> GetRemovableOrbsForParticularLevel(int level)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject item in completeDeck)
		{
			if (item.GetComponent<Attack>().Level == level && item.GetComponent<CannotBeRemoved>() == null)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public GameObject UpgradeRandomOrb(float seededRoll = -1f)
	{
		List<GameObject> upgradeableOrbs = GetUpgradeableOrbs();
		if (upgradeableOrbs.Count > 0)
		{
			GameObject toUpgrade = ((seededRoll == -1f) ? upgradeableOrbs[UnityEngine.Random.Range(0, upgradeableOrbs.Count)] : upgradeableOrbs[PeglinUtils.ListExtensions.ConvertPercentageToListIndex(seededRoll, upgradeableOrbs.Count)]);
			return UpgradeSpecificOrb(toUpgrade);
		}
		return null;
	}

	public GameObject UpgradeSpecificOrb(GameObject toUpgrade)
	{
		GameObject gameObject = toUpgrade.GetComponent<Attack>().NextLevelPrefab;
		if (gameObject != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, completeDeckOrbContainerInstance.transform);
			Attack component = gameObject2.GetComponent<Attack>();
			gameObject2.name = gameObject.name;
			component.SetInstanceId();
			completeDeck.Add(gameObject2);
			RemoveSpecifiedOrbFromDeck(toUpgrade, addToStats: false);
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.UpgradeOrbInDeck(component);
			}
		}
		else
		{
			UnlimitedUpgrades component2 = toUpgrade.GetComponent<UnlimitedUpgrades>();
			if (component2 != null)
			{
				component2.timesUpgraded++;
				gameObject = toUpgrade;
				if (StaticGameData.CurrentRunStats != null)
				{
					StaticGameData.CurrentRunStats.UpgradeOrbInDeck(component2.GetComponent<Attack>());
				}
			}
		}
		return gameObject;
	}

	public GameObject UpgradeSpecificOrbForThisBattle(GameObject toUpgrade, Vector2 playerPosition, AttackManager attackManager, RelicManager relicManager, DeckManager deckManager, CruciballManager cruciballManager, PlayerHealthController healthController, PlayerStatusEffectController statusController)
	{
		GameObject gameObject = toUpgrade.GetComponent<Attack>().NextLevelPrefab;
		int num = -1;
		if (gameObject == null && toUpgrade.TryGetComponent<UnlimitedUpgrades>(out var component))
		{
			num = component.timesUpgraded + 1;
			gameObject = toUpgrade;
		}
		if (gameObject != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, battleDeckOrbContainerInstance.transform);
			if (num > 0 && gameObject2.TryGetComponent<UnlimitedUpgrades>(out var component2))
			{
				component2.timesUpgraded = num;
			}
			Attack component3 = gameObject2.GetComponent<Attack>();
			component3.SetInstanceId();
			battleDeck.Add(gameObject2);
			RemoveOrbFromBattleDeck(toUpgrade);
			component3.Initialize(playerPosition, attackManager, relicManager, deckManager, cruciballManager, healthController, statusController);
		}
		return gameObject;
	}

	public void DuplicateAllOrbs()
	{
		ShuffleCompleteDeck();
		GameObject[] array = completeDeck.ToArray();
		foreach (GameObject orbPrefab in array)
		{
			AddOrbToDeck(orbPrefab);
		}
	}

	public void RemoveHalfOrbs()
	{
		int num = Mathf.FloorToInt((float)completeDeck.Count * 0.5f);
		for (int i = 0; i < num; i++)
		{
			RemoveRandomOrbFromDeck();
		}
	}

	public int GetNumberOfStonesInDeck()
	{
		int num = 0;
		foreach (GameObject item in completeDeck)
		{
			Attack component = item.GetComponent<Attack>();
			if (component != null && component.IsStone)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfStonesInDeckAtLevel(int level)
	{
		int num = 0;
		foreach (GameObject item in completeDeck)
		{
			Attack component = item.GetComponent<Attack>();
			if (component != null && component.IsStone && component.Level == level)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfOrebsInDeckAtLevel(int level)
	{
		int num = 0;
		foreach (GameObject item in completeDeck)
		{
			Attack component = item.GetComponent<Attack>();
			if (component != null && component.IsOreb && component.Level == level)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfLargeOrbsInDeck()
	{
		int num = 0;
		foreach (GameObject item in battleDeck)
		{
			if (item.TryGetComponent<PachinkoBall>(out var component) && component.IsOrbLargerThanDefault())
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfUpgradeableStonesInDeck()
	{
		int num = 0;
		foreach (GameObject item in completeDeck)
		{
			Attack component = item.GetComponent<Attack>();
			if (component != null && component.IsStone && component.HasUpgrade)
			{
				num++;
			}
		}
		return num;
	}

	public int GetStartWithBallwarkAmount()
	{
		int num = 0;
		int num2 = 0;
		foreach (GameObject item in shuffledDeck)
		{
			num2++;
			Attack component = item.GetComponent<Attack>();
			component.UpdateBoardDetails();
			int num3 = component.GetModifiedDamagePerPeg() * 2;
			if (num3 > num)
			{
				num = num3;
			}
		}
		return Mathf.Max(num2, num);
	}

	private void ClearDialogueOrbMapping()
	{
		if (_dialogueOrbMapping == null)
		{
			_dialogueOrbMapping = new Dictionary<string, GameObject>();
		}
		else
		{
			_dialogueOrbMapping.Clear();
		}
	}

	public void DialoguePopulatePlayerOwnsEgg()
	{
		bool flag = false;
		foreach (GameObject item in completeDeck)
		{
			if (item.GetComponent<Attack>().IsEgg)
			{
				flag = true;
				break;
			}
		}
		DialogueLua.SetVariable("player_has_egg", flag);
	}

	public void DialogueRemoveFirstEgg()
	{
		foreach (GameObject item in completeDeck)
		{
			Attack component = item.GetComponent<Attack>();
			if (component.IsEgg)
			{
				completeDeck.Remove(item);
				if (StaticGameData.CurrentRunStats != null)
				{
					StaticGameData.CurrentRunStats.AddOrbRemoval(component);
				}
				break;
			}
		}
	}

	public void DialoguePopulateLowestStones(int numChosen)
	{
		ClearDialogueOrbMapping();
		ShuffleCompleteDeck();
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		List<GameObject> list3 = new List<GameObject>();
		foreach (GameObject item in completeDeck)
		{
			Attack component = item.GetComponent<Attack>();
			if (component != null && component.IsStone)
			{
				switch (component.Level)
				{
				case 1:
					list.Add(item);
					break;
				case 2:
					list2.Add(item);
					break;
				case 3:
					list3.Add(item);
					break;
				}
			}
		}
		int num = 0;
		foreach (GameObject item2 in list)
		{
			string text = "randomOrb" + num;
			_dialogueOrbMapping[text] = item2;
			DialogueLua.SetVariable(text, item2.GetComponent<Attack>().GetNameWithLevel());
			num++;
			if (num >= numChosen)
			{
				return;
			}
		}
		foreach (GameObject item3 in list2)
		{
			string text2 = "randomOrb" + num;
			_dialogueOrbMapping[text2] = item3;
			DialogueLua.SetVariable(text2, item3.GetComponent<Attack>().GetNameWithLevel());
			num++;
			if (num >= numChosen)
			{
				return;
			}
		}
		foreach (GameObject item4 in list3)
		{
			string text3 = "randomOrb" + num;
			_dialogueOrbMapping[text3] = item4;
			DialogueLua.SetVariable(text3, item4.GetComponent<Attack>().GetNameWithLevel());
			num++;
			if (num >= numChosen)
			{
				break;
			}
		}
	}

	public void DialogueUpgradeAllStones()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject item in completeDeck)
		{
			Attack component = item.GetComponent<Attack>();
			if (component.IsStone && component.HasUpgrade)
			{
				list.Add(item);
			}
		}
		foreach (GameObject item2 in list)
		{
			UpgradeSpecificOrb(item2);
		}
	}

	public void DialogueConvertStonesToAlternateOrb(OrbPool alternateOrbPool)
	{
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		foreach (GameObject item in completeDeck)
		{
			Attack component = item.GetComponent<Attack>();
			if (!component.IsStone)
			{
				continue;
			}
			list.Add(item);
			GameObject[] availableOrbs = alternateOrbPool.AvailableOrbs;
			foreach (GameObject gameObject in availableOrbs)
			{
				if (component.Level == gameObject.GetComponent<Attack>().Level)
				{
					list2.Add(gameObject);
				}
			}
		}
		foreach (GameObject item2 in list)
		{
			RemoveSpecifiedOrbFromDeck(item2);
		}
		foreach (GameObject item3 in list2)
		{
			AddOrbToDeckSilent(item3);
		}
	}

	public void DialogueDuplicateRandomOrb()
	{
		ClearDialogueOrbMapping();
		ShuffleCompleteDeck();
		GameObject gameObject = shuffledDeck.Peek();
		DialogueLua.SetVariable("orb0", gameObject.GetComponent<Attack>().GetNameWithLevel());
		AddOrbToDeck(shuffledDeck.Peek());
	}

	public void DialogueRemoveAllOrbsAndAddNew(OrbPool orbPool)
	{
		ClearDialogueOrbMapping();
		for (int num = completeDeck.Count - 1; num >= 0; num--)
		{
			if (completeDeck[num].GetComponent<CannotBeRemoved>() == null)
			{
				if (StaticGameData.CurrentRunStats != null)
				{
					StaticGameData.CurrentRunStats.AddOrbRemoval(completeDeck[num].GetComponent<Attack>());
				}
				completeDeck.RemoveAt(num);
			}
		}
		int num2 = 0;
		GameObject[] availableOrbs = orbPool.AvailableOrbs;
		foreach (GameObject gameObject in availableOrbs)
		{
			AddOrbToDeck(gameObject);
			DialogueLua.SetVariable("orb" + num2, gameObject.GetComponent<Attack>().GetNameWithLevel());
			num2++;
		}
	}

	public void DialogueAddOrb(GameObject orbToAdd)
	{
		AddOrbToDeck(orbToAdd);
		DialogueLua.SetVariable("addedOrb", orbToAdd.GetComponent<Attack>().GetNameWithLevel());
	}

	public void DialogueRemoveOrb(GameObject orbToRemove)
	{
		RemoveSpecifiedOrbFromDeck(orbToRemove);
		DialogueLua.SetVariable("lostOrb", orbToRemove.GetComponent<Attack>().GetNameWithLevel());
	}

	public void DialogueAddMultipleOrbs(OrbPool orbPool)
	{
		ClearDialogueOrbMapping();
		int num = 0;
		GameObject[] availableOrbs = orbPool.AvailableOrbs;
		foreach (GameObject gameObject in availableOrbs)
		{
			AddOrbToDeck(gameObject);
			DialogueLua.SetVariable("orb" + num, gameObject.GetComponent<Attack>().GetNameWithLevel());
			num++;
		}
	}

	public void DialogueUpgradeAllOrbs()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject item in completeDeck)
		{
			if (item.GetComponent<Attack>().HasUpgrade)
			{
				list.Add(item);
			}
		}
		foreach (GameObject item2 in list)
		{
			UpgradeSpecificOrb(item2);
		}
	}

	public void AddRandomOrbFromPool(OrbPool orbPool)
	{
		ClearDialogueOrbMapping();
		GameObject gameObject = orbPool.AvailableOrbs[UnityEngine.Random.Range(0, orbPool.AvailableOrbs.Length)];
		AddOrbToDeck(gameObject);
		DialogueLua.SetVariable("orb0", gameObject.GetComponent<Attack>().GetNameWithLevel());
	}

	public void AddThreeRandomOrbsFromPool(OrbPool orbPool)
	{
		ClearDialogueOrbMapping();
		for (int i = 0; i < 3; i++)
		{
			GameObject gameObject = orbPool.AvailableOrbs[UnityEngine.Random.Range(0, orbPool.AvailableOrbs.Length)];
			AddOrbToDeck(gameObject);
			DialogueLua.SetVariable("orb" + i, gameObject.GetComponent<Attack>().GetNameWithLevel());
		}
	}

	public void DialoguePopulateUpgradableOrbs(int numChosen)
	{
		ClearDialogueOrbMapping();
		ShuffleCompleteDeck();
		List<GameObject> upgradeableOrbs = GetUpgradeableOrbs();
		upgradeableOrbs.MMShuffle();
		numChosen = Mathf.Min(numChosen, upgradeableOrbs.Count);
		for (int i = 0; i < numChosen; i++)
		{
			string text = "randomOrb" + i;
			GameObject gameObject = upgradeableOrbs[i];
			_dialogueOrbMapping[text] = gameObject;
			DialogueLua.SetVariable(text, gameObject.GetComponent<Attack>().GetNameWithLevel());
		}
	}

	public void DialoguePopulateUpgradableAndRemovableOrbs(int numChosen)
	{
		ClearDialogueOrbMapping();
		ShuffleCompleteDeck();
		List<GameObject> upgradeableAndRemovableOrbs = GetUpgradeableAndRemovableOrbs();
		upgradeableAndRemovableOrbs.MMShuffle();
		numChosen = Mathf.Min(numChosen, upgradeableAndRemovableOrbs.Count);
		for (int i = 0; i < numChosen; i++)
		{
			string text = "randomOrb" + i;
			GameObject gameObject = upgradeableAndRemovableOrbs[i];
			_dialogueOrbMapping[text] = gameObject;
			DialogueLua.SetVariable(text, gameObject.GetComponent<Attack>().GetNameWithLevel());
		}
	}

	public void DialoguePopulateRandomOrbs(int numChosen)
	{
		ClearDialogueOrbMapping();
		ShuffleCompleteDeck();
		numChosen = Mathf.Min(numChosen, shuffledDeck.Count);
		for (int i = 0; i < numChosen; i++)
		{
			string text = "randomOrb" + i;
			GameObject gameObject = shuffledDeck.Pop();
			_dialogueOrbMapping[text] = gameObject;
			DialogueLua.SetVariable(text, gameObject.GetComponent<Attack>().GetNameWithLevel());
		}
	}

	public void DialoguePopulateRandomRemovableOrbs(int numChosen)
	{
		ClearDialogueOrbMapping();
		List<GameObject> removableOrbs = GetRemovableOrbs();
		removableOrbs.Shuffle();
		numChosen = Mathf.Min(numChosen, removableOrbs.Count);
		for (int i = 0; i < numChosen; i++)
		{
			string text = "randomOrb" + i;
			GameObject gameObject = removableOrbs[i];
			_dialogueOrbMapping[text] = gameObject;
			DialogueLua.SetVariable(text, gameObject.GetComponent<Attack>().GetNameWithLevel());
		}
	}

	public void DialogueUpgradeOrb(string orbKey)
	{
		if (_dialogueOrbMapping.ContainsKey(orbKey))
		{
			GameObject toUpgrade = _dialogueOrbMapping[orbKey];
			toUpgrade = UpgradeSpecificOrb(toUpgrade);
			Attack component = toUpgrade.GetComponent<Attack>();
			DialogueLua.SetVariable("upgradedOrb", component.Name);
			DialogueLua.SetVariable("upgradedOrbVerbose", component.GetNameWithLevel());
			DialogueLua.SetVariable("upgradedLevel", component.FullLevel);
		}
	}

	public void DialogueUpgradeRandomOrb()
	{
		ClearDialogueOrbMapping();
		List<GameObject> upgradeableOrbs = GetUpgradeableOrbs();
		upgradeableOrbs.MMShuffle();
		GameObject gameObject = upgradeableOrbs[0];
		DialogueLua.SetVariable("orb0", gameObject.GetComponent<Attack>().GetNameWithLevel());
		DialogueLua.SetVariable("orb1", UpgradeSpecificOrb(gameObject).GetComponent<Attack>().GetNameWithLevel());
	}

	public void DialogueRemoveOrb(string orbKey)
	{
		if (_dialogueOrbMapping.ContainsKey(orbKey))
		{
			GameObject gameObject = _dialogueOrbMapping[orbKey];
			RemoveSpecifiedOrbFromDeck(gameObject);
			DialogueLua.SetVariable("lostOrb", gameObject.GetComponent<Attack>().GetNameWithLevel());
		}
	}

	public void DialogueDuplicateOrb(string orbKey)
	{
		if (_dialogueOrbMapping.ContainsKey(orbKey))
		{
			GameObject gameObject = _dialogueOrbMapping[orbKey];
			AddOrbToDeck(gameObject);
			DialogueLua.SetVariable("dupeOrb", gameObject.GetComponent<Attack>().GetNameWithLevel());
		}
	}

	public void DialoguePopulateNumberOfUpgradeableOrbs()
	{
		DialogueLua.SetVariable("num_upgrades", GetUpgradeableOrbs().Count);
	}

	public void DialoguePopulateHelpfulSpiritsEvent()
	{
		ClearDialogueOrbMapping();
		ShuffleCompleteDeck();
		List<GameObject> list = new List<GameObject>(completeDeck);
		list.MMShuffle();
		Attack attack = null;
		Attack attack2 = null;
		Attack attack3 = null;
		foreach (GameObject item in list)
		{
			Attack component = item.GetComponent<Attack>();
			if (component.Level == 1 && attack == null && item.GetComponent<CannotBeRemoved>() == null)
			{
				attack = component;
			}
			else if (component.Level == 2 && attack2 == null)
			{
				attack2 = component;
			}
			else if (component.Level == 3 && attack3 == null)
			{
				attack3 = component;
			}
		}
		if (attack == null || attack2 == null || attack3 == null)
		{
			Debug.LogError("Error! HelpfulSpirits event has populated incorrectly!(Mirrorb was incorrectly allowing the Pre Reqs before v0.8.5).");
			return;
		}
		DialogueLua.SetVariable("orb1Name", attack.GetNameWithLevel());
		_dialogueOrbMapping["orb1"] = attack.gameObject;
		DialogueLua.SetVariable("orb2Name", attack2.GetNameWithLevel());
		_dialogueOrbMapping["orb2"] = attack2.gameObject;
		DialogueLua.SetVariable("orb3Name", attack3.GetNameWithLevel());
		_dialogueOrbMapping["orb3"] = attack3.gameObject;
	}

	public string[] GetOrbQueuePrefabNamesSaveData(Queue<GameObject> orbQueue)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < orbQueue.Count; i++)
		{
			GameObject gameObject = orbQueue.Dequeue();
			orbQueue.Enqueue(gameObject);
			string text = gameObject.name;
			if (text.EndsWith("(Clone)"))
			{
				text = text.Replace("(Clone)", "");
			}
			list.Add(text);
		}
		return list.ToArray();
	}

	public void SaveDeckData()
	{
		if (completeDeck == null)
		{
			Debug.LogError("DeckManager::SaveDeckData(): Attempted to save a deck, but completeDeck was null. Bailing.");
			return;
		}
		string[] array = new string[completeDeck.Count];
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		string[] array2 = ((StaticGameData.StartingOrbs == null) ? Array.Empty<string>() : new string[StaticGameData.StartingOrbs.Length]);
		List<int> list3 = new List<int>();
		for (int i = 0; i < completeDeck.Count; i++)
		{
			string text = completeDeck[i].name;
			if (text.EndsWith("(Clone)"))
			{
				text = text.Replace("(Clone)", "");
			}
			array[i] = text;
			UnlimitedUpgrades component = completeDeck[i].GetComponent<UnlimitedUpgrades>();
			if (component != null)
			{
				list.Add(text);
				list2.Add(component.timesUpgraded);
			}
			AssemballParent component2 = completeDeck[i].GetComponent<AssemballParent>();
			if (component2 != null)
			{
				list3.Add((int)component2.assemballComponents);
			}
		}
		if (StaticGameData.StartingOrbs != null && StaticGameData.StartingOrbs.Length != 0)
		{
			for (int j = 0; j < StaticGameData.StartingOrbs.Length; j++)
			{
				string text2 = StaticGameData.StartingOrbs[j].name;
				if (text2.EndsWith("(Clone)"))
				{
					text2 = text2.Replace("(Clone)", "");
				}
				array2[j] = text2;
			}
		}
		string[] orbQueuePrefabNamesSaveData = GetOrbQueuePrefabNamesSaveData(AllCommonOrbsRandomQueue);
		string[] orbQueuePrefabNamesSaveData2 = GetOrbQueuePrefabNamesSaveData(AllUncommonOrbsRandomQueue);
		string[] orbQueuePrefabNamesSaveData3 = GetOrbQueuePrefabNamesSaveData(AllRareOrbsRandomQueue);
		string[] array3 = null;
		if (haglinsSatchelSeededOrbs != null)
		{
			array3 = new string[haglinsSatchelSeededOrbs.Length];
			for (int k = 0; k < haglinsSatchelSeededOrbs.Length; k++)
			{
				if (haglinsSatchelSeededOrbs[k].orb != null)
				{
					string text3 = haglinsSatchelSeededOrbs[k].orb.name;
					if (text3.EndsWith("(Clone)"))
					{
						text3 = text3.Replace("(Clone)", "");
					}
					array3[k] = text3;
				}
			}
		}
		new DeckManagerSaveData(array, array2, _selectedClass, list.ToArray(), list2.ToArray(), list3.ToArray(), orbQueuePrefabNamesSaveData, orbQueuePrefabNamesSaveData2, orbQueuePrefabNamesSaveData3, array3, haglinsSatchelSeededUpgradeRolls, _assemballRerollRNGState).Save();
	}

	public void LoadDeckData()
	{
		if (completeDeck == null)
		{
			Debug.LogError("DeckManager::LoadDeckData(): Attempted to load a deck, but completeDeck was null. Bailing.");
			return;
		}
		DeckManagerSaveData deckManagerSaveData = (DeckManagerSaveData)DataSerializer.Load<SaveObjectData>(DeckManagerSaveData.KEY, DataSerializer.SaveType.RUN);
		List<GameObject> list = new List<GameObject>();
		string[] prefabNames = deckManagerSaveData.PrefabNames;
		foreach (string orbName in prefabNames)
		{
			GameObject orbPrefabFromName = GetOrbPrefabFromName(orbName);
			if (!orbPrefabFromName.GetComponent<AssemballParent>())
			{
				list.Add(orbPrefabFromName);
			}
		}
		List<GameObject> list2 = new List<GameObject>();
		prefabNames = deckManagerSaveData.StartingOrbs;
		foreach (string text in prefabNames)
		{
			if (AssetLoading.Instance != null && AssetLoading.Instance.OrbPrefabs.TryGetValue(text, out var value))
			{
				list2.Add(value);
				continue;
			}
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>("Orbs/" + text + ".prefab");
			if (!asyncOperationHandle.IsValid())
			{
				Debug.LogError(text + " not found when loading deck!");
			}
			GameObject item = asyncOperationHandle.WaitForCompletion();
			list2.Add(item);
		}
		StaticGameData.StartingOrbs = list2.ToArray();
		if (StaticGameData.classLoadouts != null)
		{
			ClassLoadoutPairs[] classLoadouts = StaticGameData.classLoadouts;
			for (int i = 0; i < classLoadouts.Length; i++)
			{
				if (classLoadouts[i].Class == deckManagerSaveData.Class)
				{
					SetupClassOrbPools(deckManagerSaveData.Class);
				}
			}
		}
		else
		{
			SetupClassOrbPools(Class.Peglin);
		}
		InstantiateDeck(list);
		int[] assemballs = deckManagerSaveData.Assemballs;
		foreach (int type in assemballs)
		{
			AddOrbToDeckSilent(assemballPrefab).GetComponent<AssemballParent>().TryApplyComponent((AssemballType)type);
		}
		SearchAndMatchUpgradeLevels(deckManagerSaveData);
		PopulateAllOrbsListsForQueues();
		AllCommonOrbsRandomQueue.Clear();
		AllUncommonOrbsRandomQueue.Clear();
		AllRareOrbsRandomQueue.Clear();
		int j;
		for (j = 0; j < deckManagerSaveData.CommonOrbNamesQueue.Length; j++)
		{
			GameObject gameObject = AllCommonOrbs.Find((GameObject x) => x.name == deckManagerSaveData.CommonOrbNamesQueue[j]);
			if (gameObject == null)
			{
				OrbChangedRarityFailsafe(deckManagerSaveData.CommonOrbNamesQueue[j]);
			}
			else
			{
				AllCommonOrbsRandomQueue.Enqueue(gameObject);
			}
		}
		int i2;
		for (i2 = 0; i2 < deckManagerSaveData.UncommonOrbNamesQueue.Length; i2++)
		{
			GameObject gameObject2 = AllUncommonOrbs.Find((GameObject x) => x.name == deckManagerSaveData.UncommonOrbNamesQueue[i2]);
			if (gameObject2 == null)
			{
				OrbChangedRarityFailsafe(deckManagerSaveData.UncommonOrbNamesQueue[i2]);
			}
			else
			{
				AllUncommonOrbsRandomQueue.Enqueue(gameObject2);
			}
		}
		int i3;
		for (i3 = 0; i3 < deckManagerSaveData.RareOrbNamesQueue.Length; i3++)
		{
			GameObject gameObject3 = AllRareOrbs.Find((GameObject x) => x.name == deckManagerSaveData.RareOrbNamesQueue[i3]);
			if (gameObject3 == null)
			{
				OrbChangedRarityFailsafe(deckManagerSaveData.RareOrbNamesQueue[i3]);
			}
			else
			{
				AllRareOrbsRandomQueue.Enqueue(gameObject3);
			}
		}
		if (deckManagerSaveData.HaglinsSatchelOrbNames != null)
		{
			haglinsSatchelSeededOrbs = new SeededOrbAndRarity[deckManagerSaveData.HaglinsSatchelOrbNames.Length];
			int i4;
			for (i4 = 0; i4 < deckManagerSaveData.HaglinsSatchelOrbNames.Length; i4++)
			{
				string text2 = deckManagerSaveData.HaglinsSatchelOrbNames[i4];
				if (text2 != null && text2 != "null")
				{
					haglinsSatchelSeededOrbs[i4] = new SeededOrbAndRarity(AllRareOrbs.Find((GameObject x) => x.name == deckManagerSaveData.HaglinsSatchelOrbNames[i4]), PachinkoBall.OrbRarity.RARE);
				}
			}
		}
		if (deckManagerSaveData.HaglinsSatchelUpgradeRolls != null)
		{
			haglinsSatchelSeededUpgradeRolls = deckManagerSaveData.HaglinsSatchelUpgradeRolls;
		}
		_assemballRerollRNGState = deckManagerSaveData.AssemballRerollRNGState;
	}

	private void OrbChangedRarityFailsafe(string orbName)
	{
		GameObject gameObject = AllCommonOrbs.Find((GameObject x) => x.name == orbName);
		if (gameObject != null)
		{
			AllCommonOrbsRandomQueue.Enqueue(gameObject);
			return;
		}
		GameObject gameObject2 = AllUncommonOrbs.Find((GameObject x) => x.name == orbName);
		if (gameObject2 != null)
		{
			AllUncommonOrbsRandomQueue.Enqueue(gameObject2);
			return;
		}
		GameObject gameObject3 = AllRareOrbs.Find((GameObject x) => x.name == orbName);
		if (gameObject3 != null)
		{
			AllRareOrbsRandomQueue.Enqueue(gameObject3);
		}
	}

	public GameObject GetOrbPrefabFromName(string orbName)
	{
		if (AssetLoading.Instance == null || !AssetLoading.Instance.OrbPrefabs.TryGetValue(orbName, out var value))
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>("Orbs/" + orbName + ".prefab");
			if (!asyncOperationHandle.IsValid())
			{
				Debug.LogError(orbName + " not found when loading deck!");
			}
			return asyncOperationHandle.WaitForCompletion();
		}
		return value;
	}

	private static void SearchAndMatchUpgradeLevels(DeckManagerSaveData deckManagerSaveData)
	{
		if (deckManagerSaveData.UnlimitedUpgradeUpgradeLevels == null || deckManagerSaveData.UnlimitedUpgradePrefabNames == null)
		{
			Debug.LogWarning("We've found null UnlimtedUpgrade lists in our save data");
			return;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < deckManagerSaveData.UnlimitedUpgradePrefabNames.Length; i++)
		{
			string text = deckManagerSaveData.UnlimitedUpgradePrefabNames[i];
			UnlimitedUpgrades unlimitedUpgrades = null;
			for (int j = 0; j < completeDeck.Count; j++)
			{
				if (!list.Contains(j))
				{
					GameObject gameObject = completeDeck[j];
					if (gameObject.name.Replace("(Clone)", "").Equals(text))
					{
						unlimitedUpgrades = gameObject.GetComponent<UnlimitedUpgrades>();
						list.Add(j);
						break;
					}
				}
			}
			if (unlimitedUpgrades != null)
			{
				unlimitedUpgrades.timesUpgraded = deckManagerSaveData.UnlimitedUpgradeUpgradeLevels[i];
			}
			else
			{
				Debug.LogWarning("Did not find an UnlimitedUpgrades component when looking for the prefab " + text);
			}
		}
	}

	public void RelicModifyDeck(Relic relic)
	{
		if (completeDeck == null || relic.effect != RelicEffect.ADD_ORBS_AND_UPGRADE)
		{
			return;
		}
		if (haglinsSatchelSeededOrbs != null)
		{
			GameObject[] array = CheckAndUpdateSeededOrbOffers(haglinsSatchelSeededOrbs, allowDuplicates: true);
			for (int i = 0; i < array.Length; i++)
			{
				AddOrbToDeck(array[i]);
			}
		}
		else
		{
			for (int j = 0; j < 2; j++)
			{
				List<GameObject> randomOrbPool = GetRandomOrbPool();
				GameObject orbPrefab = randomOrbPool[UnityEngine.Random.Range(0, randomOrbPool.Count)];
				AddOrbToDeck(orbPrefab);
			}
		}
		if (haglinsSatchelSeededUpgradeRolls != null)
		{
			for (int k = 0; k < 3; k++)
			{
				UpgradeRandomOrb(haglinsSatchelSeededUpgradeRolls[k]);
			}
		}
		else
		{
			for (int l = 0; l < 3; l++)
			{
				UpgradeRandomOrb();
			}
		}
	}

	public void SeedHaglinsSatchel()
	{
		haglinsSatchelSeededOrbs = new SeededOrbAndRarity[2]
		{
			new SeededOrbAndRarity(AllRareOrbs[UnityEngine.Random.Range(0, AllRareOrbs.Count)], PachinkoBall.OrbRarity.RARE),
			new SeededOrbAndRarity(AllRareOrbs[UnityEngine.Random.Range(0, AllRareOrbs.Count)], PachinkoBall.OrbRarity.RARE)
		};
		haglinsSatchelSeededUpgradeRolls = MapController.GetMultipleSeededRolls(5);
	}

	public void SeedAssemballRerolls()
	{
		_assemballRerollRNGState = UnityEngine.Random.state;
		for (int i = 0; i < 10; i++)
		{
			_ = UnityEngine.Random.value;
		}
	}

	public List<GameObject> GetOrbPrefabs(List<string> attackNames)
	{
		List<GameObject> list = new List<GameObject>();
		if (attackNames == null || attackNames.Count == 0)
		{
			return list;
		}
		Dictionary<string, GameObject> dictionary = new Dictionary<string, GameObject>();
		foreach (GameObject item in CommonOrbPool)
		{
			dictionary.TryAdd(item.GetComponent<Attack>().locNameString, item);
		}
		foreach (GameObject item2 in UncommonOrbPool)
		{
			dictionary.TryAdd(item2.GetComponent<Attack>().locNameString, item2);
		}
		foreach (GameObject item3 in RareOrbPool)
		{
			dictionary.TryAdd(item3.GetComponent<Attack>().locNameString, item3);
		}
		foreach (GameObject item4 in SpecialOrbPool)
		{
			dictionary.TryAdd(item4.GetComponent<Attack>().locNameString, item4);
		}
		foreach (string attackName in attackNames)
		{
			if (dictionary.TryGetValue(attackName, out var value))
			{
				list.Add(value);
			}
		}
		return list;
	}

	public List<GameObject> GetRandomOrbPool()
	{
		float value = UnityEngine.Random.value;
		if (value <= 0.6f)
		{
			return CommonOrbPool;
		}
		if (value <= 0.9f)
		{
			return UncommonOrbPool;
		}
		return RareOrbPool;
	}

	public SeededOrbAndRarity[] GetMultipleSeededRandomOrbs(int number)
	{
		List<SeededOrbAndRarity> list = new List<SeededOrbAndRarity>();
		for (int i = 0; i < number; i++)
		{
			list.Add(GetSeededRandomOrb());
		}
		return list.ToArray();
	}

	public SeededOrbAndRarity GetSeededRandomOrb()
	{
		List<GameObject> list = AllCommonOrbs;
		List<GameObject> list2 = AllUncommonOrbs;
		List<GameObject> list3 = AllRareOrbs;
		float num = 0.6f;
		float num2 = 0.3f;
		CustomPoolsSecretSeed customPoolsSecretSeed = _secretSeedManager.currentSecretSeed as CustomPoolsSecretSeed;
		if (customPoolsSecretSeed != null)
		{
			if (customPoolsSecretSeed.otherOrbOfferChance > 0f && UnityEngine.Random.value > customPoolsSecretSeed.otherOrbOfferChance)
			{
				if (customPoolsSecretSeed.commonOrbPool.Count != 0)
				{
					list = customPoolsSecretSeed.commonOrbPool;
				}
				if (customPoolsSecretSeed.uncommonOrbPool.Count != 0)
				{
					list2 = customPoolsSecretSeed.uncommonOrbPool;
				}
				if (customPoolsSecretSeed.rareOrbPool.Count != 0)
				{
					list3 = customPoolsSecretSeed.rareOrbPool;
				}
				num = customPoolsSecretSeed.commonOrbChance;
				num2 = customPoolsSecretSeed.uncommonOrbChance;
			}
			else
			{
				list = new List<GameObject>(AllCommonOrbs.ToArray());
				list.RemoveAll((GameObject x) => customPoolsSecretSeed.commonOrbPool.Contains(x));
				if (list.Count == 0)
				{
					list = new List<GameObject>(AllCommonOrbs.ToArray());
				}
				list2 = new List<GameObject>(AllUncommonOrbs.ToArray());
				list2.RemoveAll((GameObject x) => customPoolsSecretSeed.uncommonOrbPool.Contains(x));
				if (list2.Count == 0)
				{
					list2 = new List<GameObject>(AllUncommonOrbs.ToArray());
				}
				list3 = new List<GameObject>(AllRareOrbs.ToArray());
				list3.RemoveAll((GameObject x) => customPoolsSecretSeed.rareOrbPool.Contains(x));
				if (list3.Count == 0)
				{
					list3 = new List<GameObject>(AllRareOrbs.ToArray());
				}
			}
		}
		float value = UnityEngine.Random.value;
		if (value <= num)
		{
			return new SeededOrbAndRarity(list[UnityEngine.Random.Range(0, list.Count)], PachinkoBall.OrbRarity.COMMON);
		}
		if (value <= num + num2)
		{
			return new SeededOrbAndRarity(list2[UnityEngine.Random.Range(0, list2.Count)], PachinkoBall.OrbRarity.UNCOMMON);
		}
		return new SeededOrbAndRarity(list3[UnityEngine.Random.Range(0, list3.Count)], PachinkoBall.OrbRarity.RARE);
	}

	public GameObject[] CheckAndUpdateSeededOrbOffers(SeededOrbAndRarity[] orbsAndRarites, bool allowDuplicates = false)
	{
		List<GameObject> list = new List<GameObject>();
		List<PachinkoBall.OrbRarity> list2 = new List<PachinkoBall.OrbRarity>();
		for (int i = 0; i < orbsAndRarites.Length; i++)
		{
			list.Add(CheckAndUpdateSeededOrb(orbsAndRarites[i]));
			list2.Add(orbsAndRarites[i].orbRarity);
		}
		if (!allowDuplicates)
		{
			bool flag = _secretSeedManager.currentSecretSeed as CustomPoolsSecretSeed != null;
			int num = 0;
			List<GameObject> list3 = list.Distinct().ToList();
			while (list3.Count != list.Count)
			{
				num++;
				if (num > 10000)
				{
					Debug.LogError("DeckManager tryied to remove duplicates over 10000 times.");
					break;
				}
				List<GameObject> list4 = new List<GameObject>();
				List<int> list5 = new List<int>();
				for (int j = 0; j < list.Count; j++)
				{
					if (list4.Contains(list[j]))
					{
						list5.Add(j);
					}
					list4.Add(list[j]);
				}
				foreach (int item in list5)
				{
					PachinkoBall.OrbRarity rarity = list2[item];
					Queue<GameObject> allPosiblOrbsOfRarityQueue = GetAllPosiblOrbsOfRarityQueue(rarity, flag && num > 5);
					GameObject gameObject = list[item];
					for (int k = 0; k < allPosiblOrbsOfRarityQueue.Count; k++)
					{
						gameObject = allPosiblOrbsOfRarityQueue.Dequeue();
						allPosiblOrbsOfRarityQueue.Enqueue(gameObject);
						if (CheckIfOrbValid(gameObject, rarity))
						{
							break;
						}
					}
					list[item] = gameObject;
				}
				list3 = list.Distinct().ToList();
			}
		}
		List<AssemballComponent> list6 = new List<AssemballComponent>();
		List<int> list7 = new List<int>();
		List<AssemballType> list8 = new List<AssemballType>();
		for (int l = 0; l < list.Count; l++)
		{
			if (list[l].TryGetComponent<AssemballComponent>(out var component))
			{
				list6.Add(component);
				list7.Add(l);
				list8.Add(component.type);
			}
		}
		if (list6.Count > 0)
		{
			List<AssemballParent> list9 = new List<AssemballParent>();
			foreach (GameObject item2 in completeDeck)
			{
				if (item2.TryGetComponent<AssemballParent>(out var component2))
				{
					list9.Add(component2);
				}
			}
			for (int m = 0; m < list6.Count; m++)
			{
				AssemballComponent assemballComponent = list6[m];
				foreach (AssemballParent item3 in list9)
				{
					if (!item3.assemballComponents.HasFlag(assemballComponent.type))
					{
						continue;
					}
					UnityEngine.Random.State state = UnityEngine.Random.state;
					UnityEngine.Random.state = _assemballRerollRNGState;
					int num2 = PeglinUtils.ListExtensions.ConvertPercentageToListIndex(UnityEngine.Random.value, assemballParts.Count);
					for (int n = 0; n < assemballParts.Count; n++)
					{
						GameObject gameObject2 = assemballParts[(num2 + n) % assemballParts.Count];
						AssemballComponent component3 = gameObject2.GetComponent<AssemballComponent>();
						if (allowDuplicates || component3.type == assemballComponent.type || !list8.Contains(component3.type))
						{
							list[list7[m]] = gameObject2;
							break;
						}
					}
					_assemballRerollRNGState = UnityEngine.Random.state;
					UnityEngine.Random.state = state;
					break;
				}
			}
		}
		return list.ToArray();
	}

	private GameObject CheckAndUpdateSeededOrb(SeededOrbAndRarity orbAndRarity)
	{
		GameObject orb = orbAndRarity.orb;
		PachinkoBall.OrbRarity orbRarity = orbAndRarity.orbRarity;
		Queue<GameObject> allPosiblOrbsOfRarityQueue = GetAllPosiblOrbsOfRarityQueue(orbRarity);
		GameObject gameObject = orb;
		for (int i = 0; i < allPosiblOrbsOfRarityQueue.Count; i++)
		{
			if (CheckIfOrbValid(gameObject, orbRarity))
			{
				break;
			}
			gameObject = allPosiblOrbsOfRarityQueue.Dequeue();
			allPosiblOrbsOfRarityQueue.Enqueue(gameObject);
		}
		return gameObject;
	}

	private bool CheckIfOrbValid(GameObject orb, PachinkoBall.OrbRarity rarity)
	{
		if (orb == null)
		{
			return false;
		}
		if (GetFinalRarityForOrb(orb, _selectedClass) != rarity)
		{
			return false;
		}
		return true;
	}

	private Queue<GameObject> GetAllPosiblOrbsOfRarityQueue(PachinkoBall.OrbRarity rarity, bool forceNonSecret = false)
	{
		CustomPoolsSecretSeed customPoolsSecretSeed = _secretSeedManager.currentSecretSeed as CustomPoolsSecretSeed;
		if (forceNonSecret || customPoolsSecretSeed == null)
		{
			switch (rarity)
			{
			case PachinkoBall.OrbRarity.COMMON:
				return AllCommonOrbsRandomQueue;
			case PachinkoBall.OrbRarity.UNCOMMON:
				return AllUncommonOrbsRandomQueue;
			case PachinkoBall.OrbRarity.RARE:
				return AllRareOrbsRandomQueue;
			default:
				Debug.LogError("Seeded orbs cannot be of type: " + rarity);
				return new Queue<GameObject>();
			}
		}
		switch (rarity)
		{
		case PachinkoBall.OrbRarity.COMMON:
			return customPoolsSecretSeed.commonOrbQueue;
		case PachinkoBall.OrbRarity.UNCOMMON:
			return customPoolsSecretSeed.uncommonOrbQueue;
		case PachinkoBall.OrbRarity.RARE:
			return customPoolsSecretSeed.rareOrbQueue;
		default:
			Debug.LogError("Seeded orbs in secret seed pools cannot be of type: " + rarity);
			return new Queue<GameObject>();
		}
	}

	public bool IsAttackUnique(Attack attack, string idToIgnore = "", bool forUI = true)
	{
		PachinkoBall component = attack.GetComponent<PachinkoBall>();
		List<GameObject> list = battleDeck;
		bool flag = true;
		foreach (GameObject item in battleDeck)
		{
			if (item != null)
			{
				flag = false;
				break;
			}
		}
		if ((list == null || (list.Count <= 0 && completeDeck.Count > 0)) | flag)
		{
			list = completeDeck;
		}
		if (component.isMirrorb)
		{
			int num = 0;
			foreach (GameObject item2 in list)
			{
				if (item2.GetComponent<CopyOrbOnReload>() != null)
				{
					num++;
				}
				if (num > 1)
				{
					return false;
				}
			}
			return true;
		}
		bool flag2 = false;
		foreach (GameObject item3 in list)
		{
			Attack component2 = item3.GetComponent<Attack>();
			if (attack.instanceID == component2.instanceID)
			{
				flag2 = true;
			}
			if (!(idToIgnore == component2.instanceID) && attack.locNameString == component2.locNameString && attack.instanceID != component2.instanceID)
			{
				return false;
			}
		}
		if (!flag2 && !forUI)
		{
			return false;
		}
		return true;
	}

	private void PopulateOrbPools(Class cl)
	{
		GameObject[] availableOrbs = OrbPool.AvailableOrbs;
		foreach (GameObject gameObject in availableOrbs)
		{
			switch (GetFinalRarityForOrb(gameObject, cl))
			{
			case PachinkoBall.OrbRarity.COMMON:
				CommonOrbPool.Add(gameObject);
				break;
			case PachinkoBall.OrbRarity.UNCOMMON:
				UncommonOrbPool.Add(gameObject);
				break;
			case PachinkoBall.OrbRarity.RARE:
				RareOrbPool.Add(gameObject);
				break;
			case PachinkoBall.OrbRarity.SPECIAL:
				SpecialOrbPool.Add(gameObject);
				break;
			case PachinkoBall.OrbRarity.NOT_PRESENT:
				UnavailableOrbPool.Add(gameObject);
				break;
			}
		}
		PopulateAllOrbsListsForQueues();
	}

	private void PopulateAllOrbsListsForQueues()
	{
		AllCommonOrbs.Clear();
		AllUncommonOrbs.Clear();
		AllRareOrbs.Clear();
		GameObject[] availableOrbs = OrbPool.AvailableOrbs;
		foreach (GameObject gameObject in availableOrbs)
		{
			PachinkoBall.OrbRarity[] allRaritiesForOrb = GetAllRaritiesForOrb(gameObject);
			for (int j = 0; j < allRaritiesForOrb.Length; j++)
			{
				switch (allRaritiesForOrb[j])
				{
				case PachinkoBall.OrbRarity.COMMON:
					AllCommonOrbs.Add(gameObject);
					break;
				case PachinkoBall.OrbRarity.UNCOMMON:
					AllUncommonOrbs.Add(gameObject);
					break;
				case PachinkoBall.OrbRarity.RARE:
					AllRareOrbs.Add(gameObject);
					break;
				}
			}
		}
	}

	private PachinkoBall.OrbRarity GetFinalRarityForOrb(GameObject orbPrefab, Class cl)
	{
		ClassOrbRarityOverride[] components = orbPrefab.GetComponents<ClassOrbRarityOverride>();
		if (components == null || components.Length == 0)
		{
			return orbPrefab.GetComponent<PachinkoBall>().orbRarity;
		}
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i]._class == cl)
			{
				return components[i].orbRarityOverride;
			}
		}
		return orbPrefab.GetComponent<PachinkoBall>().orbRarity;
	}

	public PachinkoBall.OrbRarity[] GetAllRaritiesForOrb(GameObject orbPrefab)
	{
		ClassOrbRarityOverride[] components = orbPrefab.GetComponents<ClassOrbRarityOverride>();
		if (components == null || components.Length == 0)
		{
			return new PachinkoBall.OrbRarity[1] { orbPrefab.GetComponent<PachinkoBall>().orbRarity };
		}
		List<PachinkoBall.OrbRarity> list = new List<PachinkoBall.OrbRarity>();
		list.Add(orbPrefab.GetComponent<PachinkoBall>().orbRarity);
		for (int i = 0; i < components.Length; i++)
		{
			list.Add(components[i].orbRarityOverride);
		}
		return list.ToArray();
	}

	public void SeedOrbPools()
	{
		AllCommonOrbs.Shuffle();
		AllUncommonOrbs.Shuffle();
		AllRareOrbs.Shuffle();
		AllCommonOrbsRandomQueue = new Queue<GameObject>(AllCommonOrbs);
		AllUncommonOrbsRandomQueue = new Queue<GameObject>(AllUncommonOrbs);
		AllRareOrbsRandomQueue = new Queue<GameObject>(AllRareOrbs);
	}

	public void CheckForTwoEggsAchievement()
	{
		if (AchievementData.Achievements[AchievementData.AchievementId.FINISH_RUN_WITH_TWO_EGGS].Unlocked)
		{
			return;
		}
		int num = 0;
		foreach (GameObject item in completeDeck)
		{
			if (item.GetComponent<EggPachinko>() != null)
			{
				num++;
				if (num >= 2)
				{
					AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.FINISH_RUN_WITH_TWO_EGGS);
					break;
				}
			}
		}
	}

	public bool CheckForLevelOneOrbsAchievement()
	{
		if (AchievementData.Achievements[AchievementData.AchievementId.WIN_RUN_WITH_ONLY_LEVEL_ONE_ORBS].Unlocked)
		{
			return false;
		}
		foreach (GameObject item in completeDeck)
		{
			Attack component = item.GetComponent<Attack>();
			if (component != null && component.Level > 1)
			{
				return false;
			}
		}
		return true;
	}
}
