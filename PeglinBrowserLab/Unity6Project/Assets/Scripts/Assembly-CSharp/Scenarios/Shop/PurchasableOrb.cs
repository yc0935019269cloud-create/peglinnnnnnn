using Battle.Attacks;
using PeglinUI.PostBattle;
using Relics;
using Saving;
using UnityEngine;

namespace Scenarios.Shop
{
	public class PurchasableOrb : IPurchasableItem
	{
		private GameObject _orbPrefab;

		private DeckManager _deckManager;

		private SpriteRenderer _sr;

		private Attack _attack;

		private int _cost;

		private int _costMultiplier;

		private RelicManager _relicManager;

		public PurchasableOrb(GameObject orbPrefab, DeckManager dm, int costMultiplier, RelicManager relicManager, bool cruciballIncreasedPrice = false)
		{
			_orbPrefab = orbPrefab;
			_deckManager = dm;
			_sr = _orbPrefab.GetComponentInChildren<SpriteRenderer>();
			_attack = _orbPrefab.GetComponent<Attack>();
			_costMultiplier = costMultiplier;
			switch (_orbPrefab.GetComponent<PachinkoBall>().orbRarity)
			{
			case PachinkoBall.OrbRarity.COMMON:
				_cost = 10;
				break;
			case PachinkoBall.OrbRarity.UNCOMMON:
				_cost = 20;
				break;
			case PachinkoBall.OrbRarity.RARE:
				_cost = 30;
				break;
			default:
				_cost = 10;
				break;
			}
			if (_attack.IsStone)
			{
				_cost -= 10;
			}
			if (cruciballIncreasedPrice)
			{
				_cost += 5;
			}
			_relicManager = relicManager;
		}

		public void PurchaseItem()
		{
			_deckManager.AddOrbToDeck(_orbPrefab);
		}

		public Sprite GetPurchasableImage()
		{
			return _sr.sprite;
		}

		public Vector3 GetSpriteLocalScale()
		{
			return _sr.transform.localScale * 1.5f;
		}

		public Color GetSpriteColor()
		{
			return _sr.color;
		}

		public int GetCost()
		{
			return _cost * _costMultiplier;
		}

		public void ShowTooltip(Vector3 position, Vector3 offset, bool followMousePosition = false)
		{
			_attack.SoftInit(_relicManager);
			TooltipManager.Instance.ShowTooltipOrb(_attack, position, offset, followMousePosition);
		}

		public bool GetUnlocked()
		{
			if (!PersistentPlayerData.Instance.UnlockedOrbs.Contains(_attack.locNameString))
			{
				return PersistentPlayerData.Instance.QueuedOrbUnlocks.Contains(_attack.locNameString);
			}
			return true;
		}

		public void ShowUpgradeDetailsPanel(UpgradeConfirmationOrbInspect panel)
		{
			panel.PopulateForNewOrb(_orbPrefab, GetCost());
		}
	}
}
