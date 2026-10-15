using System;
using UnityEngine;

namespace Currency
{
	public class CurrencyManager : MonoBehaviour
	{
		public delegate void CurrencyEvent(int originalAmount, int currencyChange, bool silent);

		public static CurrencyEvent OnGoldAdded;

		public static CurrencyEvent OnGoldRemoved;

		public static Action OnUpdateGoldTextRequest;

		private static CurrencyManager _instance;

		public static CurrencyManager Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new GameObject("CurrencyManager").AddComponent<CurrencyManager>();
				}
				return _instance;
			}
		}

		public int GoldAmount { get; protected set; }

		public void ClearGold()
		{
			RemoveGold(GoldAmount, silent: true, collectStats: false);
		}

		private void Awake()
		{
			if (_instance == null)
			{
				_instance = this;
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		public void AddGold(int amount, bool silent = false)
		{
			int goldAmount = GoldAmount;
			GoldAmount += amount;
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.coinsEarned += amount;
			}
			OnGoldAdded?.Invoke(goldAmount, amount, silent);
		}

		public void GoldAddedUpdateTextRequest()
		{
			OnUpdateGoldTextRequest?.Invoke();
		}

		public void RemoveGold(int amount, bool silent = false, bool collectStats = true)
		{
			int goldAmount = GoldAmount;
			GoldAmount -= amount;
			if ((StaticGameData.CurrentRunStats != null) & collectStats)
			{
				StaticGameData.CurrentRunStats.coinsSpent -= amount;
			}
			OnGoldRemoved?.Invoke(goldAmount, -amount, silent);
		}

		public bool CanAfford(int amount)
		{
			return GoldAmount >= amount;
		}
	}
}
