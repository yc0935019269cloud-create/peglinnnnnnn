using System;
using Battle.Enemies;
using Newtonsoft.Json;
using ToolBox.Serialization;
using UnityEngine;

namespace Saving
{
	public class PersistentSaveData : SaveObjectData
	{
		public bool ActiveCruciball;

		[JsonProperty]
		[SerializeField]
		private int[] _cruciballLevels;

		[JsonProperty]
		[SerializeField]
		private string[] _unlockedOrbs;

		[JsonProperty]
		[SerializeField]
		private int[] _unlockedRelics;

		[JsonProperty]
		[SerializeField]
		private int[] _unlockedChallenges;

		[JsonProperty]
		[SerializeField]
		private string[] _queuedOrbUnlocks;

		[JsonProperty]
		[SerializeField]
		private int[] _queuedRelicUnlocks;

		[JsonProperty]
		[SerializeField]
		private bool[] _achievementData;

		[JsonProperty]
		[SerializeField]
		private bool _hasPurchasedFullGameUnlock;

		[JsonProperty]
		[SerializeField]
		private int[] _selectedCruciballLevels;

		[JsonProperty]
		[SerializeField]
		private EnemyData.BestiarySaveData _bestiarySaveData;

		[JsonProperty]
		[SerializeField]
		public DateTime saveTimestamp;

		[JsonProperty]
		[SerializeField]
		public int _balladinProgress;

		[JsonProperty]
		[SerializeField]
		public int _roundrelProgress;

		[JsonProperty]
		[SerializeField]
		public int _spinventorProgress;

		[JsonIgnore]
		public override string Name => PersistentPlayerData.SAVE_KEY;

		[JsonIgnore]
		public int[] CruciballLevels
		{
			get
			{
				return _cruciballLevels;
			}
			set
			{
				_cruciballLevels = value;
			}
		}

		[JsonIgnore]
		public string[] UnlockedOrbs => _unlockedOrbs;

		[JsonIgnore]
		public string[] QueuedOrbUnlocks => _queuedOrbUnlocks;

		[JsonIgnore]
		public int[] UnlockedRelics => _unlockedRelics;

		[JsonIgnore]
		public int[] UnlockedChallenges => _unlockedChallenges;

		[JsonIgnore]
		public int[] QueuedRelicUnlocks => _queuedRelicUnlocks;

		[JsonIgnore]
		public EnemyData.BestiarySaveData BestiarySaveData
		{
			get
			{
				return _bestiarySaveData;
			}
			set
			{
				_bestiarySaveData = value;
			}
		}

		[JsonIgnore]
		public bool[] AchievementData
		{
			get
			{
				return _achievementData;
			}
			set
			{
				_achievementData = value;
			}
		}

		[JsonIgnore]
		public bool HasPurchasedFullGameUnlock => _hasPurchasedFullGameUnlock;

		[JsonIgnore]
		public int[] SelectedCruciballLevels
		{
			get
			{
				return _selectedCruciballLevels;
			}
			set
			{
				_selectedCruciballLevels = value;
			}
		}

		public PersistentSaveData()
			: base(deleteOnLose: false, DataSerializer.SaveType.BASE)
		{
		}

		public PersistentSaveData(int[] maxCruciballLevel, int[] unlockedRelics, string[] unlockedOrbs, int[] queuedRelicUnlocks, string[] queuedOrbUnlocks, int[] unlockedChallenges, bool[] achievementData, bool hasPurchasedFullGameUnlock, int[] selectedCruciballLevels, EnemyData.BestiarySaveData bestiary, int balladinProgress, int roundrelProgress, int spinventorProgress)
			: base(deleteOnLose: false, DataSerializer.SaveType.BASE)
		{
			_cruciballLevels = maxCruciballLevel;
			_unlockedOrbs = unlockedOrbs;
			_unlockedRelics = unlockedRelics;
			_unlockedChallenges = unlockedChallenges;
			_queuedOrbUnlocks = queuedOrbUnlocks;
			_queuedRelicUnlocks = queuedRelicUnlocks;
			_achievementData = achievementData;
			_hasPurchasedFullGameUnlock = hasPurchasedFullGameUnlock;
			_selectedCruciballLevels = selectedCruciballLevels;
			_bestiarySaveData = bestiary;
			Debug.Log($"Save Progress for class unlocks: Balladin={balladinProgress}, Roundrel={roundrelProgress}, Spinventor={spinventorProgress}");
			_balladinProgress = balladinProgress;
			_roundrelProgress = roundrelProgress;
			_spinventorProgress = spinventorProgress;
		}
	}
}
