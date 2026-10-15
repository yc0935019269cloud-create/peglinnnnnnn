using Map;
using UnityEngine;
using Worldmap;

namespace Seeding
{
	[CreateAssetMenu(fileName = "ChangeMapSecretSeed", menuName = "Seeding/ChangeMapSecretSeed")]
	public class ChangeMapSecretSeed : SecretSeed
	{
		[Space(15f)]
		[Range(0f, 1f)]
		public float unknownChance = 0.34f;

		[Range(0f, 1f)]
		public float scenrioChance = 0.095f;

		[Range(0f, 1f)]
		public float minibossChance = 0.115f;

		[Range(0f, 1f)]
		public float battleChance = 0.345f;

		[Range(0f, 1f)]
		public float treasureChance = 0.03f;

		[Range(0f, 1f)]
		public float storeChance = 0.05f;

		[Range(0f, 1f)]
		public float pegMinigameChance = 0.025f;

		[Tooltip("Override the line of chests that are always in the middle of maps with a different node type. Set to NONE to let it be random.")]
		public RoomType setFixedChestMidLineTo = RoomType.TREASURE;

		[Tooltip("Ignore nodes that are set to not be able to become minibosses, such as preventing the first few rows from spawning minibosses.")]
		public bool overrideCantBeMiniboss;

		protected override void Awake()
		{
			base.Awake();
			if (Mathf.Abs(unknownChance + scenrioChance + minibossChance + battleChance + treasureChance + storeChance + pegMinigameChance - 1f) > 0.001f)
			{
				Debug.LogError("Secret seed " + base.name + " node chances do not add up to 1.0 . They add up to " + $"{unknownChance + scenrioChance + minibossChance + battleChance + treasureChance + storeChance + pegMinigameChance}");
			}
		}

		public virtual void ChangeMap(MapController mapController, MapNode[] nodes)
		{
			foreach (MapNode mapNode in nodes)
			{
				if (mapNode.RoomType != RoomType.BOSS)
				{
					if (mapNode.isFixedNode && mapNode.RoomType == RoomType.TREASURE && setFixedChestMidLineTo != RoomType.NONE)
					{
						mapNode.RoomType = setFixedChestMidLineTo;
						mapNode.GenerateIcon();
					}
					else
					{
						mapNode.RoomType = GetNewRoomType(mapNode);
						mapNode.GenerateIcon();
					}
				}
			}
		}

		protected RoomType GetNewRoomType(MapNode node)
		{
			float num = 0f;
			float value = Random.value;
			if (value < unknownChance)
			{
				return RoomType.UNKNOWN;
			}
			num += unknownChance;
			if (value < num + scenrioChance)
			{
				return RoomType.SCENARIO;
			}
			num += scenrioChance;
			if (value < num + minibossChance && (overrideCantBeMiniboss || node.canBeMiniboss))
			{
				return RoomType.MINI_BOSS;
			}
			num += minibossChance;
			if (value <= num + battleChance)
			{
				return RoomType.BATTLE;
			}
			num += battleChance;
			if (value <= num + treasureChance)
			{
				return RoomType.TREASURE;
			}
			num += treasureChance;
			if (value <= num + storeChance && (overrideCantBeMiniboss || node.canBeMiniboss))
			{
				return RoomType.STORE;
			}
			num += storeChance;
			return RoomType.PEG_MINIGAME;
		}

		public override SecretSeedSaveData GetSecretSeedSaveData()
		{
			return null;
		}

		public override void LoadSecretSeedData(SecretSeedSaveData saveData)
		{
		}
	}
}
