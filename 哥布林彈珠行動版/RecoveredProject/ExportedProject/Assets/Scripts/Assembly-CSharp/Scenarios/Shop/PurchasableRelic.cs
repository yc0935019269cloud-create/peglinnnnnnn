using PeglinUI.PostBattle;
using Relics;
using Saving;
using UnityEngine;

namespace Scenarios.Shop
{
	public class PurchasableRelic : IPurchasableItem
	{
		private Relic _relic;

		private RelicManager _relicManager;

		private int _costMultiplier;

		private int _baseCost = 50;

		private int _baseCostShopOnly = 30;

		private int _cost = 50;

		private int _numPriceIncreases;

		public PurchasableRelic(Relic relic, RelicManager rm, int costMultiplier)
		{
			if (rm.IsRelicShopOnlyRelic(relic))
			{
				_baseCost = _baseCostShopOnly;
			}
			_cost = _baseCost;
			_costMultiplier = costMultiplier;
			_relic = relic;
			_relicManager = rm;
		}

		public void PurchaseItem()
		{
			Debug.Log("Purchased relic " + _relic);
			_relicManager.AddRelic(_relic);
		}

		public Sprite GetPurchasableImage()
		{
			return _relic.sprite;
		}

		public Vector3 GetSpriteLocalScale()
		{
			return Vector3.one;
		}

		public Color GetSpriteColor()
		{
			return Color.white;
		}

		public int GetCost()
		{
			return _cost * _costMultiplier;
		}

		public void ShowTooltip(Vector3 position, Vector3 offset, bool followMousePosition = false)
		{
			TooltipManager.Instance.ShowTooltipRelic(_relic, position, offset, followMousePosition);
		}

		public bool GetUnlocked()
		{
			if (!PersistentPlayerData.Instance.UnlockedRelics.Contains(_relic.effect) && !PersistentPlayerData.Instance.QueuedRelicUnlocks.Contains(_relic.effect))
			{
				return _relic.effect == RelicEffect.NONE;
			}
			return true;
		}

		public void ShowUpgradeDetailsPanel(UpgradeConfirmationRelic panel)
		{
			panel.Populate(_relic, GetCost());
		}

		public void IncreasePrice()
		{
			if (_numPriceIncreases <= 1)
			{
				_cost += 5;
			}
			else
			{
				_cost += 10;
			}
			_numPriceIncreases++;
		}

		public void ResetPrice()
		{
			_cost = _baseCost;
			_numPriceIncreases = 0;
		}
	}
}
