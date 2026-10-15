using System;
using System.Collections.Generic;
using Battle.Enemies.Polish;
using I2.Loc;
using ToolBox.Serialization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Battle.Enemies
{
	[CreateAssetMenu(menuName = "Encirclopedia/Enemy Data")]
	public class EnemyData : ScriptableObject
	{
		[Flags]
		public enum LocationTypes
		{
			NONE = 0,
			FOREST = 1,
			CASTLE = 2,
			MINES = 4,
			CORE = 8
		}

		public enum ResearchType
		{
			KILL = 0,
			LOOT = 1,
			CARD_DROP = 2
		}

		public enum EnemyType
		{
			NORMAL = 0,
			MINIBOSS = 1,
			BOSS = 2
		}

		[Serializable]
		public struct EnemySprite
		{
			public Sprite sprite;

			public Color color;

			public Vector2 position;

			public EnemySprite(Sprite sprite, Color color, Vector2 position)
			{
				this.sprite = sprite;
				this.color = color;
				this.position = position;
			}
		}

		public class BestiarySaveData : SaveObjectData
		{
			public static readonly string KEY = "BestiarySaveData";

			public EnemySaveData[] EnemyData;

			public override string Name => KEY;

			public BestiarySaveData(EnemySaveData[] enemyData)
				: base(deleteOnLose: false, DataSerializer.SaveType.BASE)
			{
				EnemyData = enemyData;
			}
		}

		[Serializable]
		public class EnemySaveData
		{
			public string LocKey;

			public bool Seen;

			public int Killed;

			public int KilledBy;

			public int CardDrop;

			public int LootDrop;

			public int ResearchProgress;

			public bool SeenInBestiary;

			public string TableKey => LocKey + NAME_SUFFIX;

			public EnemySaveData()
			{
			}

			public EnemySaveData(EnemyData data)
			{
				Update(data);
			}

			public EnemySaveData(string locKey)
			{
				LocKey = locKey;
				Seen = false;
				Killed = 0;
				KilledBy = 0;
				CardDrop = 0;
				LootDrop = 0;
				ResearchProgress = 0;
			}

			public void Reset()
			{
				Seen = false;
				SeenInBestiary = false;
				Killed = 0;
				KilledBy = 0;
				CardDrop = 0;
				LootDrop = 0;
				ResearchProgress = 0;
			}

			public void Update(EnemyData data)
			{
				LocKey = data.LocKey;
				Seen = data.Seen;
				Killed = data.Killed;
				KilledBy = data.KilledBy;
				CardDrop = data.CardDrop;
				LootDrop = data.LootDrop;
				ResearchProgress = data.ResearchProgress;
				SeenInBestiary = data.SeenInBestiary;
			}

			public void Update(EnemySaveData data)
			{
				LocKey = data.LocKey;
				Seen = data.Seen;
				Killed = data.Killed;
				KilledBy = data.KilledBy;
				CardDrop = data.CardDrop;
				LootDrop = data.LootDrop;
				ResearchProgress = data.ResearchProgress;
				SeenInBestiary = data.SeenInBestiary;
			}
		}

		public static readonly string NAME_SUFFIX = "_name";

		private static readonly string DESC_SUFFIX = "_desc";

		private static readonly string LORE_SUFFIX = "_lore";

		private static readonly string DEV_NOTES_SUFFIX = "_dev";

		private static readonly string PREFIX = "Enemies/";

		private static readonly string LOOT_SPRITE_LAYER = "SidescrollerOverlay";

		public AssetReferenceGameObject enemyPrefabAssetReference;

		public Sprite encirclepediaSprite;

		public Sprite lootDropSprite;

		public string LocKey;

		public LocationTypes location;

		public List<EnemySprite> Sprites;

		public float MaxHealth;

		public float MaxHealthCruciball;

		public float MeleeAttackDamage;

		public float RangedAttackDamage;

		public int ResearchPointTotal = 100;

		[FormerlySerializedAs("PointAmount")]
		public int PointsOnKill = 10;

		[FormerlySerializedAs("LootDropPointAmount")]
		public int PointsOnLoot = 50;

		public int ChallengeReward1 = 100;

		public int ChallengeReward2 = 100;

		public int ChallengeReward3 = 100;

		[Range(0f, 1f)]
		public float LootDropRate = 0.1f;

		[Range(0f, 1f)]
		public float CardDropRate = 0.05f;

		[NonSerialized]
		public bool Seen;

		[NonSerialized]
		public int Killed;

		[NonSerialized]
		public int KilledBy;

		[NonSerialized]
		public int CardDrop;

		[NonSerialized]
		public int LootDrop;

		[NonSerialized]
		public bool SeenInBestiary;

		[NonSerialized]
		private EnemySaveData _saveData;

		public EnemyType Type;

		public int ResearchProgress => Mathf.Clamp(Killed * PointsOnKill + LootDrop * PointsOnLoot + CardDrop * 100, 0, ResearchPointTotal);

		public string Name => PREFIX + LocKey + NAME_SUFFIX;

		public string Description => PREFIX + LocKey + DESC_SUFFIX;

		public string Lore => PREFIX + LocKey + LORE_SUFFIX;

		public string DevNotes => PREFIX + LocKey + DEV_NOTES_SUFFIX;

		public string NameLocalized => LocalizationManager.GetTranslation(Name);

		public string DescriptionLocalized => LocalizationManager.GetTranslation(Description);

		public string LoreLocalized => LocalizationManager.GetTranslation(Lore);

		public string DevNotesLocalized => LocalizationManager.GetTranslation(DevNotes);

		private List<EnemySprite> GetSpritesFromEnemy(Enemy enemy)
		{
			SpriteRenderer[] componentsInChildren = enemy.GetComponentsInChildren<SpriteRenderer>();
			List<EnemySprite> list = new List<EnemySprite>();
			SpriteRenderer[] array = componentsInChildren;
			foreach (SpriteRenderer spriteRenderer in array)
			{
				if (spriteRenderer.enabled && !(spriteRenderer.sprite == null))
				{
					EnemySprite item = new EnemySprite(spriteRenderer.sprite, spriteRenderer.color, spriteRenderer.gameObject.transform.localPosition);
					list.Add(item);
				}
			}
			return list;
		}

		public void SaveData()
		{
			if (_saveData != null)
			{
				_saveData.Update(this);
			}
			else
			{
				Debug.LogWarning("Save Data for " + LocKey + " enemy was null");
			}
		}

		public void ImportSaveData(EnemySaveData saveData)
		{
			Seen = saveData.Seen;
			Killed = saveData.Killed;
			KilledBy = saveData.KilledBy;
			CardDrop = saveData.CardDrop;
			LootDrop = saveData.LootDrop;
			SeenInBestiary = saveData.SeenInBestiary;
			_saveData = saveData;
		}

		public LootDropAnimation GetLootDropObject()
		{
			GameObject gameObject = new GameObject("LootDrop");
			SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = lootDropSprite;
			spriteRenderer.sortingLayerName = LOOT_SPRITE_LAYER;
			return gameObject.AddComponent<LootDropAnimation>();
		}
	}
}
