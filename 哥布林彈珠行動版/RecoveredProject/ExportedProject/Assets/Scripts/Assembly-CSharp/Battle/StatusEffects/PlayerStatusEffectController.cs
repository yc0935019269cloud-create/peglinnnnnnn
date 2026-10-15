using System;
using System.Collections.Generic;
using System.Linq;
using Battle.Attacks.AttackBehaviours;
using Battle.Enemies;
using Battle.PegBehaviour;
using Challenges;
using Cruciball;
using Peglin.Achievements;
using Peglin.ClassSystem;
using Relics;
using Saving;
using UnityEngine;

namespace Battle.StatusEffects
{
	[RequireComponent(typeof(PlayerHealthController))]
	public class PlayerStatusEffectController : MonoBehaviour
	{
		public delegate void AddStatusEffects(StatusEffect[] statusEffects, StatusEffectSource source);

		public delegate void PlayerStatusEffectsUpdated();

		public static AddStatusEffects ExternalStatusEffectsAdditionRequest;

		public static PlayerStatusEffectsUpdated OnPlayerStatusEffectsUpdated;

		public static PlayerStatusEffectsUpdated OnPlayerLifelinkAdded;

		public static Action BroadcastStatusEffectsModified = () =>
		{
		};

		public GameObject statusEffectInspectionPrompt;

		public StatusEffectData statusEffectData;

		private PegManager _pegManager;

		public RelicManager _relicManager;

		private StatusEffectIconManager _statusEffectUI;

		private List<StatusEffect> _statusEffects;

		private PlayerHealthController _playerHealthController;

		private int _totalStatusIntensityGainedThisBattle;

		private StatusEffectType[] _positiveStatusEffects = new StatusEffectType[7]
		{
			StatusEffectType.Strength,
			StatusEffectType.Finesse,
			StatusEffectType.Balance,
			StatusEffectType.Dexterity,
			StatusEffectType.Ballwark,
			StatusEffectType.Ballusion,
			StatusEffectType.Intangiball
		};

		private StatusEffectType[] _negativeStatusEffects = new StatusEffectType[8]
		{
			StatusEffectType.Blind,
			StatusEffectType.Confusion,
			StatusEffectType.Stunned,
			StatusEffectType.Thorned,
			StatusEffectType.Poison,
			StatusEffectType.Thorned,
			StatusEffectType.Exploitaball,
			StatusEffectType.Transpherency
		};

		public int totalStatusIntensityGainedThisBattle => _totalStatusIntensityGainedThisBattle;

		private void OnEnable()
		{
			if (_statusEffectUI == null)
			{
				_statusEffectUI = GetComponentInChildren<StatusEffectIconManager>(includeInactive: true);
			}
			ExternalStatusEffectsAdditionRequest = (AddStatusEffects)Delegate.Combine(ExternalStatusEffectsAdditionRequest, new AddStatusEffects(ApplyStatusEffects));
			BattleController.onRefreshPotionActivated = (BattleController.RefreshPotionActivated)Delegate.Combine(BattleController.onRefreshPotionActivated, new BattleController.RefreshPotionActivated(CheckRefreshEffects));
			BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Combine(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(CheckCritEffects));
			PegShieldOverlay.OnShieldPegDestroyed = (PegShieldOverlay.ShieldPegDestroyedEvent)Delegate.Combine(PegShieldOverlay.OnShieldPegDestroyed, new PegShieldOverlay.ShieldPegDestroyedEvent(CheckShieldBreakEffects));
			Enemy.OnNegativeStatusEffectAdded = (Enemy.EnemyStatusEffectAction)Delegate.Combine(Enemy.OnNegativeStatusEffectAdded, new Enemy.EnemyStatusEffectAction(HandleEnemyNegStatusAdded));
		}

		private void OnDisable()
		{
			ExternalStatusEffectsAdditionRequest = (AddStatusEffects)Delegate.Remove(ExternalStatusEffectsAdditionRequest, new AddStatusEffects(ApplyStatusEffects));
			BattleController.onRefreshPotionActivated = (BattleController.RefreshPotionActivated)Delegate.Remove(BattleController.onRefreshPotionActivated, new BattleController.RefreshPotionActivated(CheckRefreshEffects));
			BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Remove(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(CheckCritEffects));
			PegShieldOverlay.OnShieldPegDestroyed = (PegShieldOverlay.ShieldPegDestroyedEvent)Delegate.Remove(PegShieldOverlay.OnShieldPegDestroyed, new PegShieldOverlay.ShieldPegDestroyedEvent(CheckShieldBreakEffects));
			Enemy.OnNegativeStatusEffectAdded = (Enemy.EnemyStatusEffectAction)Delegate.Remove(Enemy.OnNegativeStatusEffectAdded, new Enemy.EnemyStatusEffectAction(HandleEnemyNegStatusAdded));
			if (_relicManager != null && _relicManager.RelicEffectActive(RelicEffect.LOSE_BALLWARK_GAIN_BALLANCE))
			{
				ConsumeStatusEffectOnAttack.OnStatusEffectConsumed = (ConsumeStatusEffectOnAttack.StatusEffectConsumed)Delegate.Remove(ConsumeStatusEffectOnAttack.OnStatusEffectConsumed, new ConsumeStatusEffectOnAttack.StatusEffectConsumed(CheckForBallwarkBallance));
			}
		}

		private void Start()
		{
			if (_pegManager == null && !UnityEngine.Object.FindObjectOfType<BattleController>())
			{
				Init(null);
			}
		}

		public void Init(PegManager pegManager)
		{
			_statusEffects = new List<StatusEffect>();
			if (_statusEffectUI == null)
			{
				_statusEffectUI = GetComponentInChildren<StatusEffectIconManager>();
			}
			if (_statusEffectUI != null)
			{
				_statusEffectUI.EffectData = statusEffectData;
			}
			_pegManager = pegManager;
			_playerHealthController = GetComponent<PlayerHealthController>();
			if (_relicManager.RelicEffectActive(RelicEffect.LOSE_BALLWARK_GAIN_BALLANCE))
			{
				ConsumeStatusEffectOnAttack.OnStatusEffectConsumed = (ConsumeStatusEffectOnAttack.StatusEffectConsumed)Delegate.Combine(ConsumeStatusEffectOnAttack.OnStatusEffectConsumed, new ConsumeStatusEffectOnAttack.StatusEffectConsumed(CheckForBallwarkBallance));
			}
			if (_statusEffectUI == null)
			{
				_statusEffectUI = GetComponentInChildren<StatusEffectIconManager>(includeInactive: true);
			}
			CheckForStartupEffects();
			UpdateRelicManagerOnStatus();
		}

		private void CheckForBallwarkBallance(StatusEffectType effect)
		{
			if (effect == StatusEffectType.Ballwark && _relicManager.AttemptUseRelic(RelicEffect.LOSE_BALLWARK_GAIN_BALLANCE))
			{
				ApplyStatusEffect(new StatusEffect(StatusEffectType.Balance), StatusEffectSource.PLAYER);
			}
		}

		public void ApplyStartingBonuses()
		{
			if (_relicManager.AttemptUseRelic(RelicEffect.START_WITH_STR))
			{
				ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength), StatusEffectSource.PLAYER);
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.NO_DISCARD))
			{
				ApplyStatusEffect(new StatusEffect(StatusEffectType.Balance, RelicManager.SEALED_CONVICTION_BALLANCE), StatusEffectSource.PLAYER);
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.REDUCE_REFRESH))
			{
				ApplyStatusEffect(new StatusEffect(StatusEffectType.Finesse, 5), StatusEffectSource.PLAYER);
			}
		}

		private void CheckForStartupEffects()
		{
			if (_relicManager != null)
			{
				if (_relicManager.AttemptUseRelic(RelicEffect.CONFUSION_RELIC))
				{
					ApplyStatusEffect(new StatusEffect(StatusEffectType.Confusion, 4), StatusEffectSource.PLAYER);
				}
				if (_relicManager.RelicEffectActive(RelicEffect.RETAIN_DODGE_BETWEEN_BATTLES))
				{
					ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballusion, _relicManager.GetRemainingCountdownForRelic(RelicEffect.RETAIN_DODGE_BETWEEN_BATTLES)), StatusEffectSource.PLAYER);
				}
			}
			if (ChallengeManager.Instance.AttemptCheckChallenge(ChallengeEffect.BLIND_NEXT_ACT))
			{
				ApplyStatusEffect(new StatusEffect(StatusEffectType.Blind, 100), StatusEffectSource.PLAYER);
			}
		}

		public void ApplyStatusEffects(StatusEffect[] statusEffects, StatusEffectSource source)
		{
			foreach (StatusEffect statusEffect in statusEffects)
			{
				ApplyStatusEffect(statusEffect, source);
			}
		}

		public void CheckRefreshEffects()
		{
			if (_relicManager.AttemptUseRelic(RelicEffect.REFRESH_BUFF))
			{
				if (UnityEngine.Random.value < 0.5f)
				{
					ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength), StatusEffectSource.PLAYER);
				}
				else
				{
					ApplyStatusEffect(new StatusEffect(StatusEffectType.Finesse), StatusEffectSource.PLAYER);
				}
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.STRENGTH_ON_REFRESH))
			{
				ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength), StatusEffectSource.PLAYER);
			}
		}

		private void CheckCritEffects()
		{
			if (_relicManager.AttemptUseRelic(RelicEffect.BALLUSION_ON_CRIT))
			{
				ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballusion, 10), StatusEffectSource.PLAYER);
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.CRITS_FOR_SPINESSE))
			{
				ApplyStatusEffect(new StatusEffect(StatusEffectType.Finesse), StatusEffectSource.PLAYER);
			}
		}

		private void CheckShieldBreakEffects(PegShieldOverlay pegShield)
		{
			if (_relicManager.AttemptUseRelic(RelicEffect.ADD_BALLWARK_WHEN_SHIELD_PEG_BROKEN))
			{
				ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballwark, 4), StatusEffectSource.PLAYER);
			}
		}

		private void HandleEnemyNegStatusAdded(StatusEffect statuseffect)
		{
			if (_relicManager != null && _relicManager.AttemptUseRelic(RelicEffect.STATUS_EFFECTS_ADD_DODGE))
			{
				ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballusion, statuseffect.Intensity), StatusEffectSource.PLAYER);
			}
		}

		public void ApplyStatusEffect(StatusEffect statusEffect, StatusEffectSource source)
		{
			if (statusEffect.EffectType == StatusEffectType.None)
			{
				return;
			}
			if (_pegManager != null)
			{
				switch (statusEffect.EffectType)
				{
				case StatusEffectType.Slimed:
					_pegManager.ApplySlimeToRandomPegs(Peg.SlimeType.Slow, statusEffect.Intensity);
					return;
				case StatusEffectType.Webbed:
					_pegManager.ApplySlimeToRandomPegs(Peg.SlimeType.Web, statusEffect.Intensity);
					return;
				case StatusEffectType.Rigged:
				{
					int num = statusEffect.Intensity - _pegManager.ConvertRegularBombsToRigged(statusEffect.Intensity);
					if (num > 0)
					{
						_pegManager.ConvertPegsToBombs(Mathf.CeilToInt((float)num * 0.5f), rigged: true);
						PegManager.ExternalBombCreatedNotification?.Invoke();
					}
					return;
				}
				case StatusEffectType.ShieldPegs:
					_pegManager.ApplyShieldToRegularPegs(statusEffect.Intensity);
					return;
				case StatusEffectType.FlamePegs:
					_pegManager.ApplySlimeToRandomPegs(Peg.SlimeType.FlameOverlay, statusEffect.Intensity);
					return;
				case StatusEffectType.Lifelink:
					OnPlayerLifelinkAdded?.Invoke();
					break;
				}
			}
			int num2 = CheckRelicIntensityEffects(statusEffect);
			if (source == StatusEffectSource.PLAYER && _relicManager.AttemptUseRelic(RelicEffect.DOUBLE_STATUS_EFFECTS))
			{
				num2 = Mathf.RoundToInt((float)num2 * 2f);
			}
			if (statusEffect.EffectType == StatusEffectType.Thorned && _pegManager != null)
			{
				num2 = _pegManager.CreateBramballVines(num2);
			}
			if (StaticGameData.CurrentRunStats != null && StatusEffect.IsStatusEffectBuff(statusEffect.EffectType))
			{
				if (StaticGameData.CurrentRunStats.stacksPerStatusEffect.ContainsKey(statusEffect.EffectType))
				{
					StaticGameData.CurrentRunStats.stacksPerStatusEffect[statusEffect.EffectType] += num2;
				}
				else
				{
					StaticGameData.CurrentRunStats.stacksPerStatusEffect.Add(statusEffect.EffectType, num2);
				}
				if (_positiveStatusEffects.Contains(statusEffect.EffectType))
				{
					_totalStatusIntensityGainedThisBattle += num2;
				}
				if (statusEffect.EffectType == StatusEffectType.Ballwark && AchievementManager.AchievementsOn)
				{
					StaticGameData.CurrentRunStats.balladinProgress += num2;
					Debug.Log($"Added {num2} to balladin progress, total = {StaticGameData.CurrentRunStats.balladinProgress}");
					if (!CruciballManager.IsClassUnlocked(Class.Balladin) && PersistentPlayerData.Instance.BalladinProgress + StaticGameData.CurrentRunStats.balladinProgress >= PersistentPlayerData.BALLADIN_PROGRESS_MAX && !PersistentPlayerData.Instance.QueuedClassUnlocks.Contains(Class.Balladin))
					{
						PersistentPlayerData.Instance.QueuedClassUnlocks.Add(Class.Balladin);
					}
				}
			}
			foreach (StatusEffect statusEffect2 in _statusEffects)
			{
				if (statusEffect2.EffectType == statusEffect.EffectType)
				{
					statusEffect2.Intensity = Mathf.Min(statusEffect2.Intensity + num2, StatusEffect.GetMaxIntensityForEffect(statusEffect2.EffectType, _relicManager));
					if (_statusEffectUI != null)
					{
						_statusEffectUI.UpdateStatusEffects(_statusEffects.ToArray());
					}
					UpdateRelicManagerOnStatus();
					BroadcastStatusEffectsModified?.Invoke();
					if (statusEffect.EffectType == StatusEffectType.Blind)
					{
						UpdatePlayerBlindness();
					}
					return;
				}
			}
			_statusEffects.Add(new StatusEffect(statusEffect.EffectType, Mathf.Min(num2, StatusEffect.GetMaxIntensityForEffect(statusEffect.EffectType, _relicManager))));
			if (statusEffect.EffectType == StatusEffectType.Blind)
			{
				UpdatePlayerBlindness();
			}
			if (_statusEffectUI != null)
			{
				_statusEffectUI.UpdateStatusEffects(_statusEffects.ToArray());
			}
			UpdateRelicManagerOnStatus();
			BroadcastStatusEffectsModified?.Invoke();
		}

		public void RemoveStatusEffectIntensity(StatusEffectType effectType, int intensity)
		{
			foreach (StatusEffect statusEffect in _statusEffects)
			{
				if (statusEffect.EffectType == effectType)
				{
					statusEffect.Intensity = Mathf.Clamp(statusEffect.Intensity - intensity, 0, StatusEffect.GetMaxIntensityForEffect(statusEffect.EffectType, _relicManager));
					if (_statusEffectUI != null)
					{
						_statusEffectUI.UpdateStatusEffects(_statusEffects.ToArray());
					}
					UpdateRelicManagerOnStatus();
					if (statusEffect.Intensity == 0)
					{
						_statusEffects.Remove(statusEffect);
					}
					break;
				}
			}
		}

		public void CreateTemporaryCritPegs(int num = 1)
		{
			if (_pegManager == null)
			{
				return;
			}
			foreach (Peg item in _pegManager.CreateBonusSpecialPegs(Peg.PegType.CRIT, num))
			{
				Peg.OnPegTypeConverted?.Invoke(Peg.PegType.CRIT, item.GetCenterOfPeg());
			}
		}

		public void QueueDamageReductionSlime(int numSlime)
		{
			if (_pegManager != null)
			{
				_pegManager.QueueDamageReductionSlime(numSlime);
			}
		}

		private int CheckRelicIntensityEffects(StatusEffect statusEffect)
		{
			int num = statusEffect.Intensity;
			if (statusEffect.EffectType == StatusEffectType.Ballwark)
			{
				if (statusEffect.Intensity >= 1 && _relicManager.AttemptUseRelic(RelicEffect.ARMOUR_PLUS_ONE))
				{
					num += 2;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.BALLWARK_TO_MUSCIRCLE))
				{
					for (int i = 0; i < num; i++)
					{
						if (_relicManager.AttemptUseRelic(RelicEffect.BALLWARK_TO_MUSCIRCLE))
						{
							int num2 = ((!_relicManager.AttemptUseRelic(RelicEffect.STRENGTH_PLUS_ONE)) ? 1 : 2);
							if (_relicManager.AttemptUseRelic(RelicEffect.INCREASE_POSITIVE_STATUS))
							{
								num2++;
							}
							FindAndUpdateStatusEffect(StatusEffectType.Strength, num2);
						}
					}
				}
				if (_relicManager.RelicEffectActive(RelicEffect.GAINING_BALLWARK_GIVES_MUSCIRCLE))
				{
					for (int j = 0; j < num; j++)
					{
						if (_relicManager.AttemptUseRelic(RelicEffect.GAINING_BALLWARK_GIVES_MUSCIRCLE))
						{
							ApplyStatusEffect(new StatusEffect(StatusEffectType.Strength), StatusEffectSource.PLAYER);
						}
					}
				}
				_playerHealthController.AddArmour(num);
			}
			if (statusEffect.EffectType == StatusEffectType.Strength)
			{
				if (statusEffect.Intensity >= 1 && _relicManager.AttemptUseRelic(RelicEffect.STRENGTH_PLUS_ONE))
				{
					num++;
				}
				if (statusEffect.Intensity >= 1 && _relicManager.RelicEffectActive(RelicEffect.BALLWARK_TO_MUSCIRCLE))
				{
					int num3 = (_relicManager.AttemptUseRelic(RelicEffect.ARMOUR_PLUS_ONE) ? 4 : 2);
					if (_relicManager.AttemptUseRelic(RelicEffect.INCREASE_POSITIVE_STATUS))
					{
						num3++;
					}
					_playerHealthController.AddArmour(num3);
					FindAndUpdateStatusEffect(StatusEffectType.Ballwark, num3);
				}
				if (statusEffect.Intensity >= 1 && _relicManager.AttemptUseRelic(RelicEffect.GAINING_MUSCIRCLE_GIVES_BALLWARK))
				{
					int num4 = 2;
					_playerHealthController.AddArmour(num4);
					ApplyStatusEffect(new StatusEffect(StatusEffectType.Ballwark, num4), StatusEffectSource.PLAYER);
				}
			}
			if (statusEffect.EffectType == StatusEffectType.Ballusion && statusEffect.Intensity >= 1 && _relicManager.AttemptUseRelic(RelicEffect.BALLUSION_DOUBLE_GAIN_AND_LOSS))
			{
				num *= 2;
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.INCREASE_POSITIVE_STATUS) && _positiveStatusEffects.Contains(statusEffect.EffectType) && statusEffect.Intensity >= 1)
			{
				num++;
			}
			return num;
		}

		private void FindAndUpdateStatusEffect(StatusEffectType effectType, int intensity)
		{
			foreach (StatusEffect statusEffect in _statusEffects)
			{
				if (statusEffect.EffectType == effectType)
				{
					statusEffect.Intensity = Mathf.Min(statusEffect.Intensity + intensity, StatusEffect.GetMaxIntensityForEffect(statusEffect.EffectType, _relicManager));
					if (_statusEffectUI != null)
					{
						_statusEffectUI.UpdateStatusEffects(_statusEffects.ToArray());
					}
					UpdateRelicManagerOnStatus();
					return;
				}
			}
			_statusEffects.Add(new StatusEffect(effectType, intensity));
		}

		public void UpdateStatusEffect(StatusEffect effect)
		{
			bool flag = false;
			StatusEffect statusEffect = null;
			foreach (StatusEffect statusEffect2 in _statusEffects)
			{
				if (statusEffect2.EffectType == effect.EffectType)
				{
					statusEffect = statusEffect2;
					flag = true;
					if (_positiveStatusEffects.Contains(effect.EffectType) && effect.Intensity > statusEffect2.Intensity)
					{
						_totalStatusIntensityGainedThisBattle += effect.Intensity - statusEffect2.Intensity;
					}
					statusEffect2.Intensity = effect.Intensity;
					_statusEffectUI.UpdateStatusEffect(effect);
					OnPlayerStatusEffectsUpdated?.Invoke();
					break;
				}
			}
			if (!flag)
			{
				ApplyStatusEffect(effect, StatusEffectSource.PLAYER);
			}
			else if (statusEffect != null && statusEffect.Intensity == 0)
			{
				_statusEffects.Remove(statusEffect);
			}
		}

		public bool HasEffect(StatusEffectType type)
		{
			return EffectStrength(type) > 0;
		}

		public bool HasNegativeStatusEffect()
		{
			foreach (StatusEffect statusEffect in _statusEffects)
			{
				if (_negativeStatusEffects.Contains(statusEffect.EffectType) && statusEffect.Intensity > 0)
				{
					return true;
				}
			}
			return false;
		}

		public int EffectStrength(StatusEffectType type)
		{
			if (_statusEffects == null)
			{
				return 0;
			}
			foreach (StatusEffect statusEffect in _statusEffects)
			{
				if (type == statusEffect.EffectType)
				{
					return statusEffect.Intensity;
				}
			}
			return 0;
		}

		public void ResolvePlayerSpinfection()
		{
			List<StatusEffect> list = new List<StatusEffect>();
			float num = 0f;
			StatusEffect[] array = _statusEffects.ToArray();
			foreach (StatusEffect statusEffect in array)
			{
				if (statusEffect.EffectType == StatusEffectType.Poison)
				{
					num = statusEffect.Intensity;
					statusEffect.Intensity--;
					if (statusEffect.Intensity <= 0)
					{
						list.Add(statusEffect);
					}
				}
			}
			if (num > 0f)
			{
				_playerHealthController.Damage(num, melee: false, null, PlayerHealthController.DamageSource.STATUS_EFFECT);
			}
			if (_statusEffectUI != null)
			{
				_statusEffectUI.UpdateStatusEffects(_statusEffects.ToArray());
			}
			foreach (StatusEffect item in list)
			{
				_statusEffects.Remove(item);
			}
			UpdateRelicManagerOnStatus();
		}

		public void ResolveStatusEffects()
		{
			List<StatusEffect> list = new List<StatusEffect>();
			StatusEffect[] array = _statusEffects.ToArray();
			foreach (StatusEffect statusEffect in array)
			{
				if (StatusEffect.IsStatusEffectPerishable(statusEffect.EffectType) && statusEffect.EffectType != StatusEffectType.Poison && statusEffect.EffectType != StatusEffectType.Thorned)
				{
					statusEffect.Intensity--;
					if (statusEffect.Intensity <= 0)
					{
						list.Add(statusEffect);
					}
				}
			}
			if (_statusEffectUI != null)
			{
				_statusEffectUI.UpdateStatusEffects(_statusEffects.ToArray());
			}
			foreach (StatusEffect item in list)
			{
				_statusEffects.Remove(item);
			}
			UpdatePlayerBlindness();
			UpdateRelicManagerOnStatus();
		}

		public void UpdatePlayerBlindness()
		{
			List<Peg> list = new List<Peg>();
			if (_pegManager != null)
			{
				if (_pegManager.allPegs == null)
				{
					return;
				}
				list.AddRange(_pegManager.allPegs);
				list.AddRange(_pegManager.bouncerPegs);
			}
			else
			{
				list = new List<Peg>(UnityEngine.Object.FindObjectsOfType<Peg>());
			}
			float num = (float)EffectStrength(StatusEffectType.Blind) / 100f;
			int num2 = Mathf.RoundToInt((float)list.Count * num);
			List<Peg> list2 = new List<Peg>();
			List<Peg> list3 = new List<Peg>();
			foreach (Peg item in list)
			{
				if (item.isInvisibleFromBlind)
				{
					list2.Add(item);
				}
				else
				{
					list3.Add(item);
				}
			}
			if (list2.Count < num2)
			{
				for (int i = 0; i < num2 - list2.Count; i++)
				{
					int index = UnityEngine.Random.Range(0, list3.Count);
					list3[index].SetInvisibleFromBlind(value: true);
					list3.RemoveAt(index);
				}
			}
			else if (list2.Count > num2)
			{
				for (int j = 0; j < list2.Count - num2; j++)
				{
					int index2 = UnityEngine.Random.Range(0, list2.Count);
					list2[index2].SetInvisibleFromBlind(value: false);
					list2.RemoveAt(index2);
				}
			}
		}

		private void UpdateRelicManagerOnStatus()
		{
			_relicManager.UpdateStatusDependentRelics(this);
		}

		public StatusEffect[] GetStatusEffects()
		{
			return _statusEffects.ToArray();
		}

		public void HideStatusEffectUI()
		{
			_statusEffectUI.gameObject.SetActive(value: false);
		}
	}
}
