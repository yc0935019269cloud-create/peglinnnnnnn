using System;
using System.Globalization;
using Currency;
using Map;
using PixelCrushers.DialogueSystem;
using Relics;
using UnityEngine;

namespace Scenarios
{
	public class DialogueScriptableObjectInteractions : ScriptableObject
	{
		public static Action OnScenarioEnded = () =>
		{
		};

		public DeckManager deckManager;

		public RelicManager relicManager;

		public FloatVariable playerHealth;

		public FloatVariable playerMaxHealth;

		public void PopulateDialogueHealthVars()
		{
			DialogueLua.SetVariable("currentHealth", playerHealth.Value);
			DialogueLua.SetVariable("maxHealth", playerMaxHealth.Value);
			DialogueLua.SetVariable("halfMaxHealth", Mathf.FloorToInt(playerMaxHealth.Value / 2f));
			DialogueLua.SetVariable("10PercentMaxHealth", Mathf.RoundToInt(playerMaxHealth.Value * 0.1f));
		}

		public void PopulateDialogueGoldVars()
		{
			int goldAmount = CurrencyManager.Instance.GoldAmount;
			DialogueLua.SetVariable("allGold", goldAmount);
			DialogueLua.SetVariable("halfGold", Mathf.CeilToInt((float)goldAmount * 0.5f));
			DialogueLua.SetVariable("thirdGold", Mathf.CeilToInt((float)goldAmount * 0.2f));
			DialogueLua.SetVariable("midasDamageForGoldGain", Mathf.CeilToInt((float)goldAmount * 0.2f));
		}

		public void PopulateDialogueUpgradeableOrbVars()
		{
			int count = deckManager.GetUpgradeableOrbs().Count;
			DialogueLua.SetVariable("numUpgradeableOrbs", count.ToString(CultureInfo.InvariantCulture));
			bool flag = DialogueLua.GetVariable("upgradeCost").asInt <= CurrencyManager.Instance.GoldAmount;
			DialogueLua.SetVariable("canUpgrade", flag && count > 0);
		}

		public void PopulateRemovableOrbsVar()
		{
			DialogueLua.SetVariable("numRemovableOrbs", deckManager.GetRemovableOrbs().Count.ToString(CultureInfo.InvariantCulture));
		}

		public void PopulateDialogueCostToLeave()
		{
			int num = Mathf.Min(DialogueLua.GetVariable("leaveCost").asInt, CurrencyManager.Instance.GoldAmount);
			DialogueLua.SetVariable("leaveCost", num);
		}

		public void PopulateDialogueGamblinInvestment()
		{
			int remainingCountdownForRelic = relicManager.GetRemainingCountdownForRelic(RelicEffect.GAMBLIN_IOU);
			DialogueLua.SetVariable("investmentReturn", remainingCountdownForRelic * Mathf.CeilToInt((float)MapController.instance.Act * 1.5f));
		}

		public void HealPlayer(float heal)
		{
			playerHealth.Add(heal);
		}

		public void HealPlayerByDialogueVar(string healVar)
		{
			float asFloat = DialogueLua.GetVariable(healVar).asFloat;
			playerHealth.Add(asFloat);
		}

		public void IncreaseMaxHealth(float health)
		{
			if (relicManager.AttemptUseRelic(RelicEffect.INCREASE_MAX_HP_GAIN))
			{
				health++;
			}
			playerMaxHealth.Add(health);
			playerHealth.Add(health);
		}

		public void IncreaseMaxHealthByDialogueVar(string amountVar)
		{
			float num = DialogueLua.GetVariable(amountVar).asInt;
			if (relicManager.AttemptUseRelic(RelicEffect.INCREASE_MAX_HP_GAIN))
			{
				num++;
			}
			playerMaxHealth.Add(num);
			playerHealth.Add(num);
		}

		public void ReduceMaxHealth(float health)
		{
			playerMaxHealth.Subtract(health);
			playerHealth.Add(0f);
		}

		public void ReduceMaxHealthByHalf()
		{
			playerMaxHealth.Subtract(Mathf.FloorToInt(playerMaxHealth.Value / 2f));
			playerHealth.Add(0f);
		}

		public void AcceptVampiresDeal()
		{
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.vampireDealTaken = true;
			}
		}

		public void DamagePlayer(float damage)
		{
			if (relicManager.AttemptUseRelic(RelicEffect.IMMORTAL))
			{
				return;
			}
			if (damage >= playerHealth.Value && relicManager.AttemptUseRelic(RelicEffect.PREVENT_LETHAL_DAMAGE))
			{
				damage = 0f;
				if (!relicManager.RelicEffectActive(RelicEffect.HALVE_INCOMING_DAMAGE))
				{
					relicManager.AddRelic(relicManager.GetRelicForEffectFromAllData(RelicEffect.HALVE_INCOMING_DAMAGE));
				}
			}
			else if (relicManager.AttemptUseRelic(RelicEffect.HALVE_INCOMING_DAMAGE))
			{
				damage = Mathf.Floor(damage / 2f);
			}
			if (relicManager.AttemptUseRelic(RelicEffect.REDUCE_LOST_HEALTH))
			{
				damage--;
			}
			playerHealth.Subtract(Mathf.Max(damage, 0f));
		}

		public void DamagePlayerByDialogueVar(string damageVar)
		{
			float asFloat = DialogueLua.GetVariable(damageVar).asFloat;
			DamagePlayer(asFloat);
		}

		public void AddGoldFromDialogueVar(string goldVar)
		{
			CurrencyManager.Instance.AddGold(DialogueLua.GetVariable(goldVar).asInt);
			CurrencyManager.Instance.GoldAddedUpdateTextRequest();
		}

		public void RemoveGoldFromDialogueVar(string goldVar)
		{
			int amount = Mathf.Min(DialogueLua.GetVariable(goldVar).asInt, CurrencyManager.Instance.GoldAmount);
			CurrencyManager.Instance.RemoveGold(amount);
		}

		public void SetGamblerIOUAmount()
		{
			int asInt = DialogueLua.GetVariable("investAmt").asInt;
			relicManager.SetRemainingCountdownForRelic(RelicEffect.GAMBLIN_IOU, asInt);
			CurrencyManager.Instance.RemoveGold(asInt);
		}

		public void SetRandomRoll()
		{
			DialogueLua.SetVariable("randomRoll", UnityEngine.Random.Range(1, 101));
		}

		public void SetRandomMidasGoldGain()
		{
			DialogueLua.SetVariable("randomGoldGain", UnityEngine.Random.Range(30, 70));
		}

		public void RemoveRandomOrb()
		{
			deckManager.RemoveRandomOrbFromDeck();
		}

		public void BroadcastEventEnded()
		{
			OnScenarioEnded?.Invoke();
		}
	}
}
