using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Relics;
using Seeding;
using ToolBox.Serialization;
using UnityEngine;

public class SecretSeedManager : ScriptableObject
{
	private class SecretSeedManagerSaveData : SaveObjectData
	{
		public static readonly string KEY = "SecretSeedManager";

		[SerializeField]
		private string _currentSecretSeedName;

		[SerializeField]
		private SecretSeedSaveData _currentSecretSeedData;

		public override string Name => KEY;

		public string CurrentSecretSeedName => _currentSecretSeedName;

		public SecretSeedSaveData CurrentSecretSeedData => _currentSecretSeedData;

		public SecretSeedManagerSaveData(string currentSecretSeedName, SecretSeedSaveData currentSecretSeedData)
			: base(deleteOnLose: true, DataSerializer.SaveType.RUN)
		{
			_currentSecretSeedName = currentSecretSeedName;
			_currentSecretSeedData = currentSecretSeedData;
		}
	}

	[SerializeField]
	private RelicManager _relicManager;

	[SerializeField]
	private DeckManager _deckManager;

	[ReorderableList]
	[SerializeField]
	private List<SecretSeed> _secretSeeds;

	private SecretSeed _currentSecretSeed;

	public List<SecretSeed> secretSeeds => _secretSeeds;

	public SecretSeed currentSecretSeed => _currentSecretSeed;

	public void CheckAndSetUpSecretSeed()
	{
		_currentSecretSeed = null;
		foreach (SecretSeed secretSeed in _secretSeeds)
		{
			if (StaticGameData.currentSeed == secretSeed.seed)
			{
				_currentSecretSeed = secretSeed;
				break;
			}
		}
		if (_currentSecretSeed != null)
		{
			_currentSecretSeed.SetUpSecretSeed(_deckManager, _relicManager);
		}
	}

	public bool IsSecretSeed(string seed)
	{
		foreach (SecretSeed secretSeed in _secretSeeds)
		{
			if (seed == secretSeed.seed)
			{
				return true;
			}
		}
		return false;
	}

	private void OnValidate()
	{
		if (_secretSeeds.Count() != _secretSeeds.Distinct().Count())
		{
			Debug.LogWarning("Secret Seeds contains duplicate items.");
		}
	}

	public void SaveSecretSeedData()
	{
		string currentSecretSeedName = null;
		SecretSeedSaveData currentSecretSeedData = null;
		if (_currentSecretSeed != null)
		{
			currentSecretSeedName = _currentSecretSeed.name;
			currentSecretSeedData = _currentSecretSeed.GetSecretSeedSaveData();
		}
		new SecretSeedManagerSaveData(currentSecretSeedName, currentSecretSeedData).Save();
	}

	public void LoadSecretSeedData()
	{
		SecretSeedManagerSaveData secretSeedManagerSaveData = (SecretSeedManagerSaveData)DataSerializer.Load<SaveObjectData>(SecretSeedManagerSaveData.KEY, DataSerializer.SaveType.RUN);
		if (secretSeedManagerSaveData == null || secretSeedManagerSaveData.CurrentSecretSeedName == null)
		{
			return;
		}
		foreach (SecretSeed secretSeed in _secretSeeds)
		{
			if (secretSeedManagerSaveData.CurrentSecretSeedName == secretSeed.name)
			{
				_currentSecretSeed = secretSeed;
				_currentSecretSeed.Initialize(_deckManager, _relicManager);
				_currentSecretSeed.LoadSecretSeedData(secretSeedManagerSaveData.CurrentSecretSeedData);
			}
		}
	}
}
