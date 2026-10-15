using System.Collections.Generic;
using System.Linq;
using PeglinUtils;
using Relics;
using UnityEngine;

namespace Seeding
{
	[CreateAssetMenu(fileName = "CustomPoolsSecretSeed", menuName = "Seeding/CustomPoolsSecretSeed")]
	public class CustomPoolsSecretSeed : SecretSeed
	{
		private class CustomPoolsSecretSeedSaveData : SecretSeedSaveData
		{
			[SerializeField]
			private string[] _commonOrbNamesQueue;

			[SerializeField]
			private string[] _uncommonOrbNamesQueue;

			[SerializeField]
			private string[] _rareOrbNamesQueue;

			[SerializeField]
			private RelicManager.RelicQueueSaveObject _commonRelicQueue;

			[SerializeField]
			private RelicManager.RelicQueueSaveObject _rareRelicQueue;

			[SerializeField]
			private RelicManager.RelicQueueSaveObject _bossRelicQueue;

			public string[] CommonOrbNamesQueue => _commonOrbNamesQueue;

			public string[] UncommonOrbNamesQueue => _uncommonOrbNamesQueue;

			public string[] RareOrbNamesQueue => _rareOrbNamesQueue;

			public RelicManager.RelicQueueSaveObject CommonRelicQueue => _commonRelicQueue;

			public RelicManager.RelicQueueSaveObject RareRelicQueue => _rareRelicQueue;

			public RelicManager.RelicQueueSaveObject BossRelicQueue => _bossRelicQueue;

			public CustomPoolsSecretSeedSaveData(string[] commonOrbNamesQueue, string[] uncommonOrbNamesQueue, string[] rareOrbNamesQueue, RelicManager.RelicQueueSaveObject commonRelicQueue, RelicManager.RelicQueueSaveObject rareRelicQueue, RelicManager.RelicQueueSaveObject bossRelicQueue)
			{
				_commonOrbNamesQueue = commonOrbNamesQueue;
				_uncommonOrbNamesQueue = uncommonOrbNamesQueue;
				_rareOrbNamesQueue = rareOrbNamesQueue;
				_commonRelicQueue = commonRelicQueue;
				_rareRelicQueue = rareRelicQueue;
				_bossRelicQueue = bossRelicQueue;
			}
		}

		[Header("Pool Customization")]
		public List<GameObject> customOrbPool = new List<GameObject>();

		[Range(0f, 1f)]
		[Tooltip("The chance that a common orb from this pool is offered")]
		public float commonOrbChance = 0.6f;

		[Range(0f, 1f)]
		[Tooltip("The chance that a uncommon orb from this pool is offered")]
		public float uncommonOrbChance = 0.3f;

		[Range(0f, 1f)]
		[Tooltip("The chance that a rare orb from this pool is offered")]
		public float rareOrbChance = 0.1f;

		[Tooltip("The chance that an orb not in this pool is still offered")]
		[Range(0f, 1f)]
		public float otherOrbOfferChance;

		public List<Relic> customRelicPool = new List<Relic>();

		[Tooltip("When the player owns all the relics in this custom pool, start offering relics that are not in this pool.")]
		public bool relicPoolFallback = true;

		private List<GameObject> _commonOrbPool = new List<GameObject>();

		private List<GameObject> _uncommonOrbPool = new List<GameObject>();

		private List<GameObject> _rareOrbPool = new List<GameObject>();

		private Queue<GameObject> _commonOrbQueue = new Queue<GameObject>();

		private Queue<GameObject> _uncommonOrbQueue = new Queue<GameObject>();

		private Queue<GameObject> _rareOrbQueue = new Queue<GameObject>();

		private List<RelicManager.RelicClassRarityOverrides> _commonRelicPool = new List<RelicManager.RelicClassRarityOverrides>();

		private List<RelicManager.RelicClassRarityOverrides> _rareRelicPool = new List<RelicManager.RelicClassRarityOverrides>();

		private List<RelicManager.RelicClassRarityOverrides> _bossRelicPool = new List<RelicManager.RelicClassRarityOverrides>();

		private Queue<RelicManager.RelicClassRarityOverrides> _commonRelicQueue = new Queue<RelicManager.RelicClassRarityOverrides>();

		private Queue<RelicManager.RelicClassRarityOverrides> _rareRelicQueue = new Queue<RelicManager.RelicClassRarityOverrides>();

		private Queue<RelicManager.RelicClassRarityOverrides> _bossRelicQueue = new Queue<RelicManager.RelicClassRarityOverrides>();

		public List<GameObject> commonOrbPool => _commonOrbPool;

		public List<GameObject> uncommonOrbPool => _uncommonOrbPool;

		public List<GameObject> rareOrbPool => _rareOrbPool;

		public Queue<GameObject> commonOrbQueue => _commonOrbQueue;

		public Queue<GameObject> uncommonOrbQueue => _uncommonOrbQueue;

		public Queue<GameObject> rareOrbQueue => _rareOrbQueue;

		public List<RelicManager.RelicClassRarityOverrides> commonRelicPool => _commonRelicPool;

		public List<RelicManager.RelicClassRarityOverrides> rareRelicPool => _rareRelicPool;

		public List<RelicManager.RelicClassRarityOverrides> bossRelicPool => _bossRelicPool;

		public Queue<RelicManager.RelicClassRarityOverrides> commonRelicQueue => _commonRelicQueue;

		public Queue<RelicManager.RelicClassRarityOverrides> rareRelicQueue => _rareRelicQueue;

		public Queue<RelicManager.RelicClassRarityOverrides> bossRelicQueue => _bossRelicQueue;

		protected override void Awake()
		{
			base.Awake();
			if (Mathf.Abs(commonOrbChance + uncommonOrbChance + rareOrbChance - 1f) > 0.001f)
			{
				Debug.LogError($"Secret seed {base.name} orb chances do not add up to 1.0 . They add up to {commonOrbChance + uncommonOrbChance + rareOrbChance}");
			}
		}

		public override void SetUpSecretSeed(DeckManager deckManager, RelicManager relicManager)
		{
			base.SetUpSecretSeed(deckManager, relicManager);
			CustomizeRandomPools();
		}

		protected void CustomizeRandomPools()
		{
			_commonOrbQueue = CreateCustomOrbQueue(customOrbPool, PachinkoBall.OrbRarity.COMMON, _deckManager.CurrentCommonOrbQueue.ToArray());
			_uncommonOrbQueue = CreateCustomOrbQueue(customOrbPool, PachinkoBall.OrbRarity.UNCOMMON, _deckManager.CurrentUncommonOrbQueue.ToArray());
			_rareOrbQueue = CreateCustomOrbQueue(customOrbPool, PachinkoBall.OrbRarity.RARE, _deckManager.CurrentRareOrbQueue.ToArray());
			_commonOrbPool = new List<GameObject>(_commonOrbQueue.ToArray());
			_uncommonOrbPool = new List<GameObject>(_uncommonOrbQueue.ToArray());
			_rareOrbPool = new List<GameObject>(_rareOrbQueue.ToArray());
			_commonRelicQueue = CreateCustomRelicQueue(customRelicPool, RelicRarity.COMMON, _relicManager.CurrentCommonRelicsQueue.ToArray());
			_rareRelicQueue = CreateCustomRelicQueue(customRelicPool, RelicRarity.RARE, _relicManager.CurrentRareRelicsQueue.ToArray());
			_bossRelicQueue = CreateCustomRelicQueue(customRelicPool, RelicRarity.BOSS, _relicManager.CurrentBossRelicsQueue.ToArray());
			_commonRelicPool = new List<RelicManager.RelicClassRarityOverrides>(_commonRelicQueue.ToArray());
			_rareRelicPool = new List<RelicManager.RelicClassRarityOverrides>(_rareRelicQueue.ToArray());
			_bossRelicPool = new List<RelicManager.RelicClassRarityOverrides>(_bossRelicQueue.ToArray());
		}

		private Queue<GameObject> CreateCustomOrbQueue(List<GameObject> customOrbsPool, PachinkoBall.OrbRarity orbRarity, GameObject[] allOrbsArray)
		{
			List<GameObject> list = new List<GameObject>(customOrbsPool.ToArray());
			list.Shuffle();
			Queue<GameObject> queue = new Queue<GameObject>();
			new List<GameObject>(allOrbsArray);
			foreach (GameObject item in list)
			{
				if (_deckManager.GetAllRaritiesForOrb(item).Contains(orbRarity))
				{
					queue.Enqueue(item);
				}
			}
			if (queue.Count == 0)
			{
				queue = new Queue<GameObject>(allOrbsArray);
			}
			return queue;
		}

		private Queue<RelicManager.RelicClassRarityOverrides> CreateCustomRelicQueue(List<Relic> customRelicsPool, RelicRarity rarity, RelicManager.RelicClassRarityOverrides[] allRelicsArray)
		{
			List<Relic> list = new List<Relic>(customRelicsPool.ToArray());
			list.Shuffle();
			Queue<RelicManager.RelicClassRarityOverrides> queue = new Queue<RelicManager.RelicClassRarityOverrides>();
			List<RelicManager.RelicClassRarityOverrides> list2 = new List<RelicManager.RelicClassRarityOverrides>(allRelicsArray);
			foreach (Relic r in list)
			{
				RelicManager.RelicClassRarityOverrides relicClassRarityOverrides = list2.Find((RelicManager.RelicClassRarityOverrides x) => x.relic.effect == r.effect);
				if (!(relicClassRarityOverrides.relic == null) && relicClassRarityOverrides.classRarities.Values.Contains(rarity))
				{
					queue.Enqueue(list2.Find((RelicManager.RelicClassRarityOverrides x) => x.relic.effect == r.effect));
				}
			}
			if (queue.Count == 0)
			{
				queue = new Queue<RelicManager.RelicClassRarityOverrides>(allRelicsArray);
			}
			return queue;
		}

		public override SecretSeedSaveData GetSecretSeedSaveData()
		{
			string[] orbQueuePrefabNamesSaveData = _deckManager.GetOrbQueuePrefabNamesSaveData(_commonOrbQueue);
			string[] orbQueuePrefabNamesSaveData2 = _deckManager.GetOrbQueuePrefabNamesSaveData(_uncommonOrbQueue);
			string[] orbQueuePrefabNamesSaveData3 = _deckManager.GetOrbQueuePrefabNamesSaveData(_rareOrbQueue);
			RelicManager.RelicQueueSaveObject relicQueueSaveObject = _relicManager.GetRelicQueueSaveObject(_commonRelicQueue);
			RelicManager.RelicQueueSaveObject relicQueueSaveObject2 = _relicManager.GetRelicQueueSaveObject(_rareRelicQueue);
			RelicManager.RelicQueueSaveObject relicQueueSaveObject3 = _relicManager.GetRelicQueueSaveObject(_bossRelicQueue);
			return new CustomPoolsSecretSeedSaveData(orbQueuePrefabNamesSaveData, orbQueuePrefabNamesSaveData2, orbQueuePrefabNamesSaveData3, relicQueueSaveObject, relicQueueSaveObject2, relicQueueSaveObject3);
		}

		public override void LoadSecretSeedData(SecretSeedSaveData saveData)
		{
			CustomPoolsSecretSeedSaveData customPoolsSaveData = saveData as CustomPoolsSecretSeedSaveData;
			_commonOrbQueue.Clear();
			_uncommonOrbQueue.Clear();
			_rareOrbQueue.Clear();
			List<GameObject> list = new List<GameObject>(_deckManager.CurrentCommonOrbQueue);
			List<GameObject> list2 = new List<GameObject>(_deckManager.CurrentUncommonOrbQueue);
			List<GameObject> list3 = new List<GameObject>(_deckManager.CurrentRareOrbQueue);
			int i;
			for (i = 0; i < customPoolsSaveData.CommonOrbNamesQueue.Length; i++)
			{
				_commonOrbQueue.Enqueue(list.Find((GameObject x) => x.name == customPoolsSaveData.CommonOrbNamesQueue[i]));
			}
			int i2;
			for (i2 = 0; i2 < customPoolsSaveData.UncommonOrbNamesQueue.Length; i2++)
			{
				_uncommonOrbQueue.Enqueue(list2.Find((GameObject x) => x.name == customPoolsSaveData.UncommonOrbNamesQueue[i2]));
			}
			int i3;
			for (i3 = 0; i3 < customPoolsSaveData.RareOrbNamesQueue.Length; i3++)
			{
				_rareOrbQueue.Enqueue(list3.Find((GameObject x) => x.name == customPoolsSaveData.RareOrbNamesQueue[i3]));
			}
			_commonOrbPool = new List<GameObject>(_commonOrbQueue.ToArray());
			_uncommonOrbPool = new List<GameObject>(_uncommonOrbQueue.ToArray());
			_rareOrbPool = new List<GameObject>(_rareOrbQueue.ToArray());
			_commonRelicQueue.Clear();
			_rareRelicQueue.Clear();
			_bossRelicQueue.Clear();
			List<RelicManager.RelicClassRarityOverrides> list4 = new List<RelicManager.RelicClassRarityOverrides>(_relicManager.CurrentCommonRelicsQueue);
			List<RelicManager.RelicClassRarityOverrides> list5 = new List<RelicManager.RelicClassRarityOverrides>(_relicManager.CurrentRareRelicsQueue);
			List<RelicManager.RelicClassRarityOverrides> list6 = new List<RelicManager.RelicClassRarityOverrides>(_relicManager.CurrentBossRelicsQueue);
			int i4;
			for (i4 = 0; i4 < customPoolsSaveData.CommonRelicQueue.RelicEffects.Length; i4++)
			{
				_commonRelicQueue.Enqueue(list4.Find((RelicManager.RelicClassRarityOverrides x) => x.relic.effect == (RelicEffect)customPoolsSaveData.CommonRelicQueue.RelicEffects[i4]));
			}
			int i5;
			for (i5 = 0; i5 < customPoolsSaveData.RareRelicQueue.RelicEffects.Length; i5++)
			{
				_rareRelicQueue.Enqueue(list5.Find((RelicManager.RelicClassRarityOverrides x) => x.relic.effect == (RelicEffect)customPoolsSaveData.RareRelicQueue.RelicEffects[i5]));
			}
			int i6;
			for (i6 = 0; i6 < customPoolsSaveData.BossRelicQueue.RelicEffects.Length; i6++)
			{
				_bossRelicQueue.Enqueue(list6.Find((RelicManager.RelicClassRarityOverrides x) => x.relic.effect == (RelicEffect)customPoolsSaveData.BossRelicQueue.RelicEffects[i6]));
			}
			_commonRelicPool = new List<RelicManager.RelicClassRarityOverrides>(_commonRelicQueue.ToArray());
			_rareRelicPool = new List<RelicManager.RelicClassRarityOverrides>(_rareRelicQueue.ToArray());
			_bossRelicPool = new List<RelicManager.RelicClassRarityOverrides>(_bossRelicQueue.ToArray());
		}
	}
}
