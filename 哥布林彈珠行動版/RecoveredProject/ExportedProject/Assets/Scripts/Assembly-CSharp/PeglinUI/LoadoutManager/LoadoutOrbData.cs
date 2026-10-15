using Battle.Attacks;
using UnityEngine;

namespace PeglinUI.LoadoutManager
{
	public class LoadoutOrbData
	{
		public delegate void AmountChanged(int amt);

		public AmountChanged OnAmountChanged;

		public AmountChanged OnLevelChanged;

		public string orbName;

		public GameObject orbPrefab;

		private int _amount;

		private int _level = 1;

		public int MaxLevel = 3;

		private bool _canBeLeveledUp = true;

		public int Amount
		{
			get
			{
				return _amount;
			}
			set
			{
				_amount = value;
				OnAmountChanged?.Invoke(_amount);
			}
		}

		public int Level
		{
			get
			{
				return _level;
			}
			set
			{
				_level = value;
				OnLevelChanged?.Invoke(_level);
			}
		}

		public bool CanBeLeveledUp => _canBeLeveledUp;

		public LoadoutOrbData(GameObject orbPrefab, int amount)
		{
			this.orbPrefab = orbPrefab;
			Amount = amount;
			Attack component = orbPrefab.GetComponent<Attack>();
			orbName = component.locNameString;
			int num = component.Level;
			Level = component.Level;
			_canBeLeveledUp = component.NextLevelPrefab != null;
			while (component.NextLevelPrefab != null)
			{
				num++;
				component = component.NextLevelPrefab.GetComponent<Attack>();
			}
			MaxLevel = num;
		}
	}
}
