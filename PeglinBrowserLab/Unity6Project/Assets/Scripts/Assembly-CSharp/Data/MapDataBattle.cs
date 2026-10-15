using System.Collections.Generic;
using Relics;
using Stats;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data
{
	[CreateAssetMenu(fileName = "New BattleData", menuName = "BattleData")]
	public class MapDataBattle : MapData
	{
		public RelicRarity grantedRelicRarity;

		public int availableGoldModifier;

		public bool grantChallenge;

		public Sprite mapIconOverride;

		public PegLayout pegLayout;

		public WaveGroup[] waveGroups;

		public List<StarterSpawn> starterSpawns;

		public GameObject[] battleDoodads;

		public int NumberOfSlots = 7;

		[FormerlySerializedAs("boss")]
		public RunStats.BossType bossType;

		public C19BattleData c19BattleData;

		public override SceneType GetSceneType()
		{
			return SceneType.BATTLE;
		}
	}
}
