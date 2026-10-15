using System.Collections.Generic;
using Relics;
using UnityEngine;

namespace Seeding
{
	[CreateAssetMenu(fileName = "SecretSeed", menuName = "Seeding/SecretSeed")]
	public class SecretSeed : ScriptableObject
	{
		[Tooltip("The seed in the form 10 symbol indexes from 0-8.")]
		public string seed;

		public List<Relic> unlockAndAddRelics = new List<Relic>();

		protected RelicManager _relicManager;

		protected DeckManager _deckManager;

		public void Initialize(DeckManager deckManager, RelicManager relicManager)
		{
			_relicManager = relicManager;
			_deckManager = deckManager;
		}

		public virtual void SetUpSecretSeed(DeckManager deckManager, RelicManager relicManager)
		{
			Initialize(deckManager, relicManager);
			UnlockAndAddRelics();
		}

		protected void UnlockAndAddRelics()
		{
			foreach (Relic unlockAndAddRelic in unlockAndAddRelics)
			{
				_relicManager.AddRelic(unlockAndAddRelic);
			}
		}

		protected virtual void Awake()
		{
			if (seed.Length != 10)
			{
				Debug.LogError($"Secret seed {base.name} not of correct length. {seed.Length} instead of {10}");
			}
			for (int i = 0; i < seed.Length; i++)
			{
				int num = int.Parse(seed.Substring(i, 1));
				if (num < 0 || num >= 9)
				{
					Debug.LogError($"{num} is not a valid seed symbol index in {base.name}.");
				}
			}
		}

		public virtual SecretSeedSaveData GetSecretSeedSaveData()
		{
			return null;
		}

		public virtual void LoadSecretSeedData(SecretSeedSaveData saveData)
		{
		}
	}
}
