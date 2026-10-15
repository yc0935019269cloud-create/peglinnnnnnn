using System;
using System.Collections.Generic;
using Battle.Attacks;
using Battle.Enemies;
using Battle.Pachinko.BallBehaviours;
using Battle.Pachinko.Obstacles;
using Battle.PegBehaviour;
using Cruciball;
using MoreMountains.Tools;
using Peglin.Achievements;
using PeglinUtils;
using Relics;
using Stats;
using UnityEngine;

namespace Battle
{
	public class PegManager
	{
		public delegate void RefreshedPegsEvent(int numPegs);

		public delegate void ExternalPegboardAddGoldRequest(int num, Vector3 flyingCoinEffectStartPos = default(Vector3));

		public RefreshedPegsEvent OnPegsRefreshed;

		public static ExternalPegboardAddGoldRequest AddGoldToPegsRequest = (int gold, Vector3 flyingCoinEffectStartPos) =>
		{
		};

		public static Action ExternalBombCreatedNotification = () =>
		{
		};

		public static Action OnSpecialPegsShuffled = () =>
		{
		};

		private const float BramballVineMinLength = 3.2f;

		private const float BramballVineMaxLength = 6.4f;

		private const float C20LeshyBramballVineMinLength = 4.8f;

		private const float C20LeshyBramballVineMaxLength = 12f;

		public bool isC20Leshy;

		public RunStats.BossType bossType;

		protected List<Peg> _allPegs;

		protected List<Bomb> _bombs;

		protected List<BouncerPeg> _bouncerPegs;

		public List<Peg> delayedDeathPegs = new List<Peg>();

		private CruciballManager _cruciballManager;

		protected RelicManager _relicManager;

		protected List<Peg> _resetPegs = new List<Peg>();

		protected List<Peg> _critPegs = new List<Peg>();

		private List<PegBoardBramballVine> _vines = new List<PegBoardBramballVine>();

		protected Peg _lastHitResetPeg;

		private BattleController _battleController;

		private PredictionManager _predictionManager;

		private bool _isLayoutAlternating;

		private int _queuedDamageReductionSlime;

		[HideInInspector]
		public SerializableRandomState specialPegShufflingRandomState;

		[HideInInspector]
		public List<SerializableRandomState> bossMechanicsRandomStatesPerAct;

		public bool PegsLoaded { get; protected set; }

		public List<Peg> allPegs => _allPegs;

		public List<BouncerPeg> bouncerPegs => _bouncerPegs;

		public Peg lastHitResetPeg => _lastHitResetPeg;

		public PegManager(RelicManager relicManager, CruciballManager cruciballManager, BattleController battleController, PredictionManager predictionManager)
		{
			_cruciballManager = cruciballManager;
			_relicManager = relicManager;
			_battleController = battleController;
			_predictionManager = predictionManager;
		}

		public void InitializePegs(List<Peg> pegs, Bomb[] bombs, List<BouncerPeg> bouncerPegs)
		{
			_allPegs = new List<Peg>(pegs);
			_bombs = new List<Bomb>(bombs);
			_bouncerPegs = new List<BouncerPeg>(bouncerPegs);
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegActivated));
			Peg.OnPegCollisionHit = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegCollisionHit, new Peg.PegHitEvent(HandlePegCollisionHit));
			DuplicateSpecialPegs.OnDuplicatorbHitSpecialPeg = (DuplicateSpecialPegs.DuplicatorbHitSpecialPeg)Delegate.Combine(DuplicateSpecialPegs.OnDuplicatorbHitSpecialPeg, new DuplicateSpecialPegs.DuplicatorbHitSpecialPeg(DuplicateSpecialPeg));
			PegBoardBramballVine.OnBramballVineDestroyed = (PegBoardBramballVine.BramballVineDestroyed)Delegate.Combine(PegBoardBramballVine.OnBramballVineDestroyed, new PegBoardBramballVine.BramballVineDestroyed(RemoveDestroyedVinesFromList));
			PegBoardBramballVine.OnVineMutliballCreated = (PegBoardBramballVine.VineMutliballCreated)Delegate.Combine(PegBoardBramballVine.OnVineMutliballCreated, new PegBoardBramballVine.VineMutliballCreated(CheckForBramballInVinesAchievement));
			ExternalBombCreatedNotification = (Action)Delegate.Combine(ExternalBombCreatedNotification, new Action(OnExternalBombCreatedNotification));
			PegsLoaded = true;
			_isLayoutAlternating = _battleController.pegLayoutAlternator != null;
		}

		public void UnsubscribeFromDelegates()
		{
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegActivated));
			Peg.OnPegCollisionHit = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegCollisionHit, new Peg.PegHitEvent(HandlePegCollisionHit));
			DuplicateSpecialPegs.OnDuplicatorbHitSpecialPeg = (DuplicateSpecialPegs.DuplicatorbHitSpecialPeg)Delegate.Remove(DuplicateSpecialPegs.OnDuplicatorbHitSpecialPeg, new DuplicateSpecialPegs.DuplicatorbHitSpecialPeg(DuplicateSpecialPeg));
			PegBoardBramballVine.OnBramballVineDestroyed = (PegBoardBramballVine.BramballVineDestroyed)Delegate.Remove(PegBoardBramballVine.OnBramballVineDestroyed, new PegBoardBramballVine.BramballVineDestroyed(RemoveDestroyedVinesFromList));
			PegBoardBramballVine.OnVineMutliballCreated = (PegBoardBramballVine.VineMutliballCreated)Delegate.Remove(PegBoardBramballVine.OnVineMutliballCreated, new PegBoardBramballVine.VineMutliballCreated(CheckForBramballInVinesAchievement));
			ExternalBombCreatedNotification = (Action)Delegate.Remove(ExternalBombCreatedNotification, new Action(OnExternalBombCreatedNotification));
		}

		public void AddPeg(Peg peg)
		{
			_allPegs.Add(peg);
		}

		public void AddBouncerPeg(BouncerPeg bouncerPeg)
		{
			_bouncerPegs.Add(bouncerPeg);
		}

		public void AddPegs(List<Peg> newPegs)
		{
			_allPegs.AddRange(newPegs);
		}

		public void AddBombs(List<Bomb> bombs)
		{
			_bombs.AddRange(bombs);
		}

		public void AddBomb(Bomb bomb)
		{
			_bombs.Add(bomb);
			if (StaticGameData.CurrentRunStats != null)
			{
				if (bomb.isRigged)
				{
					StaticGameData.CurrentRunStats.bombsCreatedRigged++;
				}
				else
				{
					StaticGameData.CurrentRunStats.bombsCreated++;
				}
			}
		}

		public void ClearPegList()
		{
			_allPegs.Clear();
		}

		public void ShuffleSpecialPegs(bool forceRefresh = false)
		{
			ShufflePegs(_critPegs, Peg.PegType.CRIT, !_relicManager.AttemptUseRelic(RelicEffect.DUPLICATE_SPECIAL_PEGS));
			if (forceRefresh || _relicManager.RelicEffectActive(RelicEffect.SHUFFLE_REFRESH_PEG))
			{
				ShufflePegs(_resetPegs, Peg.PegType.RESET, !_relicManager.AttemptUseRelic(RelicEffect.DUPLICATE_SPECIAL_PEGS));
			}
			OnSpecialPegsShuffled?.Invoke();
		}

		public void ShuffleCritPegs()
		{
			ShufflePegs(_critPegs, Peg.PegType.CRIT, !_relicManager.AttemptUseRelic(RelicEffect.DUPLICATE_SPECIAL_PEGS));
		}

		public void FailSafeCreateRefreshPegs()
		{
			List<Peg> list = new List<Peg>();
			List<Peg> list2 = new List<Peg>();
			foreach (Peg allPeg in _allPegs)
			{
				if (!allPeg.IsDisabled() && allPeg.gameObject.activeInHierarchy && allPeg.SupportsPegType(Peg.PegType.RESET) && allPeg.pegType != Peg.PegType.VINE && allPeg.pegType != Peg.PegType.SPINFECTION)
				{
					list.Add(allPeg);
				}
				else if (allPeg.pegType == Peg.PegType.CRIT || allPeg is RegularPeg { specialPegBlocker: not false })
				{
					list2.Add(allPeg);
				}
			}
			if (list.Count > 0)
			{
				for (int i = 0; i < GetPegCount(Peg.PegType.RESET); i++)
				{
					Peg peg = CreateSpecialPeg(Peg.PegType.RESET, list, seeded: true);
					if (peg != null)
					{
						_resetPegs.Add(peg);
					}
				}
			}
			else if (list.Count == 0 && list2.Count > 0)
			{
				list2[UnityEngine.Random.Range(0, list2.Count)].ConvertPegToType(Peg.PegType.RESET);
			}
		}

		public bool AreAllPegsPopped()
		{
			foreach (Peg allPeg in _allPegs)
			{
				if (!allPeg.IsDisabled())
				{
					return false;
				}
			}
			return true;
		}

		public void CreateRefreshPegs()
		{
			CreateSpecialPegs(_resetPegs, Peg.PegType.RESET, null, -1, seeded: true);
		}

		private void DuplicateSpecialPeg(Peg peg, int numberOfDuplicatesToAdd)
		{
			List<Peg> list = null;
			if (peg.pegType == Peg.PegType.CRIT && _critPegs.Contains(peg))
			{
				list = _critPegs;
			}
			else
			{
				if (peg.pegType != Peg.PegType.RESET || !_resetPegs.Contains(peg))
				{
					Debug.LogWarning("Tried to duplicate a non special peg.");
					return;
				}
				list = _resetPegs;
			}
			Peg peg2 = CreateSpecialPeg(peg.pegType, GetAllActiveAndValidConversionPegsForType(peg.pegType), seeded: true);
			if (peg2 == null)
			{
				return;
			}
			peg.justDuplicated = true;
			peg.SetIsBonusSpecialPeg(newIsBonusSpecialPeg: true);
			list.Remove(peg);
			list.Add(peg2);
			Peg.OnPegTypeConverted?.Invoke(peg.pegType, peg.GetCenterOfPeg());
			numberOfDuplicatesToAdd--;
			foreach (Peg item in CreateBonusSpecialPegs(peg.pegType, numberOfDuplicatesToAdd))
			{
				Peg.OnPegTypeConverted?.Invoke(peg.pegType, item.GetCenterOfPeg());
			}
		}

		public List<Peg> CreateBonusSpecialPegs(Peg.PegType pegType, int number)
		{
			List<Peg> allActiveAndValidConversionPegsForType = GetAllActiveAndValidConversionPegsForType(pegType);
			List<Peg> list = new List<Peg>();
			for (int i = 0; i < number; i++)
			{
				Peg peg = CreateSpecialPeg(pegType, allActiveAndValidConversionPegsForType, seeded: true);
				if (peg != null)
				{
					peg.SetIsBonusSpecialPeg(newIsBonusSpecialPeg: true);
					list.Add(peg);
				}
			}
			if (pegType == Peg.PegType.BOMB)
			{
				ExternalBombCreatedNotification?.Invoke();
			}
			return list;
		}

		protected void ShufflePegs(List<Peg> pegsToShuffle, Peg.PegType type, bool resetPrevious = true)
		{
			List<Peg> list = new List<Peg>();
			list.AddRange(pegsToShuffle);
			pegsToShuffle.Clear();
			CreateSpecialPegs(pegsToShuffle, type, list, -1, seeded: true);
			foreach (Peg item in list)
			{
				if (item != null && item.pegType != Peg.PegType.DESTROYED)
				{
					if (resetPrevious)
					{
						item.Reset(BattleController.criticalActive);
						continue;
					}
					item.SetIsBonusSpecialPeg(newIsBonusSpecialPeg: true);
					Peg.OnPegTypeConverted?.Invoke(item.pegType, item.GetCenterOfPeg());
				}
				else if (pegsToShuffle.Contains(item))
				{
					pegsToShuffle.Remove(item);
				}
			}
			int num = GetPegCount(type) - pegsToShuffle.Count;
			if (num > 0)
			{
				if (type == Peg.PegType.RESET)
				{
					num += pegsToShuffle.Count;
				}
				CreateSpecialPegs(pegsToShuffle, type, null, num, seeded: true);
			}
		}

		public void ApplyGoldToPegs(int numCoins, bool doParticleEffect = true, Vector3 flyingCoinEffectStartPos = default(Vector3))
		{
			bool flag = flyingCoinEffectStartPos != default(Vector3);
			_allPegs.Shuffle();
			int num = 0;
			foreach (Peg allPeg in _allPegs)
			{
				if (num >= numCoins)
				{
					break;
				}
				LongPeg longPeg = allPeg as LongPeg;
				RegularPeg regularPeg = allPeg as RegularPeg;
				if (allPeg != null && ((object)regularPeg == null || !regularPeg.coinBlocker) && !allPeg.IsDisabled() && allPeg.NumCoins() <= 0 && ((object)longPeg == null || !longPeg.hit))
				{
					if (!flag)
					{
						allPeg.AddCoin(doParticleEffect);
					}
					else
					{
						_battleController.CreateFlyingCoinEffect().GetComponent<FlyingCoinEffect>().DoCoinAdditionAnimation(flyingCoinEffectStartPos, allPeg);
					}
					num++;
				}
			}
			int num2 = num;
			while (num < numCoins)
			{
				foreach (Peg allPeg2 in _allPegs)
				{
					if (num >= numCoins)
					{
						break;
					}
					LongPeg longPeg2 = allPeg2 as LongPeg;
					RegularPeg regularPeg2 = allPeg2 as RegularPeg;
					if (allPeg2 != null && ((object)regularPeg2 == null || !regularPeg2.coinBlocker) && !allPeg2.IsDisabled() && allPeg2.NumCoins() < 9 && ((object)longPeg2 == null || !longPeg2.hit))
					{
						if (!flag)
						{
							allPeg2.AddCoin(doParticleEffect);
						}
						else
						{
							_battleController.CreateFlyingCoinEffect().GetComponent<FlyingCoinEffect>().DoCoinAdditionAnimation(flyingCoinEffectStartPos, allPeg2);
						}
						num++;
					}
				}
				if (num == num2)
				{
					break;
				}
			}
		}

		public int RemoveGoldFromPegsWithoutCollecting(int numCoins, Vector3 flyingCoinEffectEndPos = default(Vector3))
		{
			bool flag = flyingCoinEffectEndPos != default(Vector3);
			_allPegs.Shuffle();
			int num = 0;
			foreach (Peg allPeg in _allPegs)
			{
				if (num >= numCoins)
				{
					break;
				}
				LongPeg longPeg = allPeg as LongPeg;
				RegularPeg regularPeg = allPeg as RegularPeg;
				if (allPeg != null && ((object)regularPeg == null || !regularPeg.coinBlocker) && !allPeg.IsDisabled() && allPeg.NumCoins() > 0 && ((object)longPeg == null || !longPeg.hit))
				{
					if (!flag)
					{
						allPeg.RemoveCoinsWithoutCollecting(1);
					}
					else
					{
						_battleController.CreateFlyingCoinEffect().GetComponent<FlyingCoinEffect>().DoCoinRemovalAnimation(flyingCoinEffectEndPos, allPeg);
					}
					num++;
				}
			}
			return num;
		}

		public void ApplySlimeToRandomPegs(Peg.SlimeType slimeType, int numToSlime)
		{
			List<Peg> list = new List<Peg>(_allPegs);
			list.AddRange(_bouncerPegs.ToArray());
			if (bossType == RunStats.BossType.DRAGON && slimeType == Peg.SlimeType.FlameOverlay)
			{
				list.Sort((Peg x, Peg y) => (int)Mathf.Sign(x.GetInstanceID() - y.GetInstanceID()));
				bossMechanicsRandomStatesPerAct[0].SeededShuffle(list);
			}
			else
			{
				list.MMShuffle();
			}
			foreach (Peg item in list)
			{
				if (!item.IsDisabled() && item.isActiveAndEnabled && item.SupportsPegType(Peg.PegType.SLIME_ONLY, slimeType))
				{
					if (--numToSlime < 0)
					{
						break;
					}
					item.ApplySlimeToPeg(slimeType);
				}
			}
		}

		public int CreateBramballVines(int numVinesToCreate)
		{
			List<Peg> list = new List<Peg>(_allPegs.ToArray());
			if (list.Count < 2)
			{
				return 0;
			}
			if (bossType == RunStats.BossType.LESHY)
			{
				list.Sort((Peg x, Peg y) => (int)Mathf.Sign(x.GetInstanceID() - y.GetInstanceID()));
			}
			List<Peg> list2 = new List<Peg>(list.ToArray());
			int num = 0;
			for (int num2 = 0; num2 < numVinesToCreate; num2++)
			{
				for (int num3 = 0; num3 < 100; num3++)
				{
					list = new List<Peg>(list2.ToArray());
					int index;
					int index2;
					if (bossType == RunStats.BossType.LESHY)
					{
						index = bossMechanicsRandomStatesPerAct[0].GetRandomRangeValue(0, list.Count - 1);
						list.RemoveAt(index);
						index2 = bossMechanicsRandomStatesPerAct[0].GetRandomRangeValue(0, list.Count - 1);
					}
					else
					{
						index = UnityEngine.Random.Range(0, list.Count - 1);
						list.RemoveAt(index);
						index2 = UnityEngine.Random.Range(0, list.Count - 1);
					}
					Peg peg = list[index];
					Peg peg2 = list[index2];
					if (!peg.IsDisabled() && peg.SupportsPegType(Peg.PegType.VINE) && peg.pegType != Peg.PegType.VINE && peg.pegType != Peg.PegType.CRIT && peg.pegType != Peg.PegType.RESET && !peg2.IsDisabled() && peg2.SupportsPegType(Peg.PegType.VINE) && peg2.pegType != Peg.PegType.VINE && peg2.pegType != Peg.PegType.CRIT && peg2.pegType != Peg.PegType.RESET)
					{
						Vector3 vector = peg.transform.position;
						if (peg is LongPeg)
						{
							vector = peg.GetCenterOfPeg();
						}
						Vector3 vector2 = peg2.transform.position;
						if (peg2 is LongPeg)
						{
							vector2 = peg2.GetCenterOfPeg();
						}
						float magnitude = (vector - vector2).magnitude;
						float num4 = 3.2f;
						float num5 = 6.4f;
						if (isC20Leshy)
						{
							num4 = 4.8f;
							num5 = 12f;
						}
						if (!(magnitude < num4) && !(magnitude > num5))
						{
							PegBoardBramballVine component = _battleController.CreateBramballVine().GetComponent<PegBoardBramballVine>();
							component.Initialize(peg, peg2, _predictionManager);
							peg.bramballVine = component;
							peg2.bramballVine = component;
							peg.ConvertPegToType(Peg.PegType.VINE);
							peg2.ConvertPegToType(Peg.PegType.VINE);
							_vines.Add(component);
							num++;
							break;
						}
					}
				}
			}
			return num;
		}

		public int RemoveRandomBramballVines(int amount)
		{
			int num = 0;
			for (int i = 0; i < amount; i++)
			{
				_vines.Shuffle();
				PegBoardBramballVine pegBoardBramballVine = null;
				foreach (PegBoardBramballVine vine in _vines)
				{
					if (vine.allOrbsCaughtInVine.Count <= 0)
					{
						pegBoardBramballVine = vine;
						break;
					}
				}
				if (pegBoardBramballVine == null)
				{
					break;
				}
				pegBoardBramballVine.DestroyVine(doDeathEffects: false);
				num++;
			}
			return num;
		}

		private void CheckForBramballInVinesAchievement()
		{
			Achievement achievement = AchievementData.Achievements[AchievementData.AchievementId.THREE_BRAMBALLS_STUCK_IN_VINES];
			if (achievement.Unlocked)
			{
				return;
			}
			int num = 0;
			foreach (PegBoardBramballVine vine in _vines)
			{
				foreach (OrbCaughtInVine item in vine.allOrbsCaughtInVine)
				{
					if (item.isBramball)
					{
						num++;
					}
					if (num >= 3)
					{
						AchievementManager.Instance.UnlockAchievement(achievement);
						break;
					}
				}
			}
		}

		private void RemoveDestroyedVinesFromList(PegBoardBramballVine destroyedVine)
		{
			_vines.Remove(destroyedVine);
		}

		public void QueueDamageReductionSlime(int numSlime)
		{
			_queuedDamageReductionSlime += numSlime;
		}

		public void AddQueuedDamageReductionSlime()
		{
			ApplySlimeToRandomPegs(Peg.SlimeType.DamageReductionSlime, _queuedDamageReductionSlime);
			_queuedDamageReductionSlime = 0;
		}

		public int ConvertRegularBombsToRigged(int numToConvert)
		{
			int num = 0;
			for (int i = 0; i < _bombs.Count; i++)
			{
				if (num >= numToConvert)
				{
					return num;
				}
				if (!_bombs[i].isRigged && !_bombs[i].IsDisabled())
				{
					_bombs[i].ConvertToRigged();
					num++;
				}
			}
			return num;
		}

		public List<Peg> ApplyShieldToRegularPegs(int numToConvert)
		{
			_allPegs.Shuffle();
			List<Peg> list = new List<Peg>();
			int num = 0;
			foreach (Peg allPeg in _allPegs)
			{
				if (num >= numToConvert)
				{
					break;
				}
				if (allPeg != null && allPeg.SupportsShielding())
				{
					allPeg.ApplyShielding(claimed: true);
					list.Add(allPeg);
					num++;
				}
			}
			return list;
		}

		public void CreateSpecialPegs(List<Peg> specialPegs, Peg.PegType type, List<Peg> avoidPegs = null, int count = -1, bool seeded = false)
		{
			if (count < 0)
			{
				count = GetPegCount(type);
			}
			if (type == Peg.PegType.RESET)
			{
				foreach (Peg specialPeg in specialPegs)
				{
					if (specialPeg.gameObject.activeInHierarchy && !specialPeg.IsDisabled())
					{
						count--;
					}
				}
			}
			List<Peg> list = GetAllActiveAndValidConversionPegsForType(type, avoidPegs);
			if (list.Count == 0 && (type == Peg.PegType.CRIT || type == Peg.PegType.RESET))
			{
				list = GetAllActiveAndValidConversionSpecialPegBlockerPegs(type, avoidPegs);
			}
			for (int i = 0; i < count; i++)
			{
				Peg peg = CreateSpecialPeg(type, list, seeded);
				if (peg != null)
				{
					specialPegs.Add(peg);
				}
			}
		}

		public int GetPegCount(Peg.PegType type)
		{
			switch (type)
			{
			case Peg.PegType.CRIT:
			{
				int num2 = ((_cruciballManager != null && _cruciballManager.FewerCritPegs()) ? 1 : 2);
				if (_relicManager.RelicEffectActive(RelicEffect.ADDITIONAL_CRIT1))
				{
					num2++;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.ADDITIONAL_CRIT2))
				{
					num2 += 2;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.CRIT_PIT))
				{
					num2 += 2;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.ADDITIONAL_CRIT3))
				{
					num2 += 3;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.REDUCE_CRIT))
				{
					num2--;
				}
				return num2;
			}
			case Peg.PegType.RESET:
			{
				int num = ((_cruciballManager != null && _cruciballManager.FewerRefreshPegs()) ? 1 : 2);
				if (_relicManager.RelicEffectActive(RelicEffect.ADDITIONAL_REFRESH1))
				{
					num++;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.ADDITIONAL_REFRESH2))
				{
					num += 2;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.ADDITIONAL_REFRESH3))
				{
					num += 3;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.REDUCE_REFRESH))
				{
					num--;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.ONLY_REFRESH_X_PEGS))
				{
					num += 10;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.ALL_ORBS_MORBID))
				{
					num = Mathf.FloorToInt((float)num / 2f);
				}
				return num;
			}
			default:
				return 1;
			}
		}

		protected Peg CreateSpecialPeg(Peg.PegType type, List<Peg> validPegs, bool seeded = false)
		{
			if (validPegs.Count == 0)
			{
				return null;
			}
			Peg peg;
			if (seeded)
			{
				validPegs.Sort((Peg x, Peg y) => (int)Mathf.Sign(x.GetInstanceID() - y.GetInstanceID()));
				peg = validPegs[specialPegShufflingRandomState.GetRandomRangeValue(0, validPegs.Count)];
			}
			else
			{
				peg = validPegs[UnityEngine.Random.Range(0, validPegs.Count)];
			}
			validPegs.Remove(peg);
			peg.ConvertPegToType(type);
			return peg;
		}

		private List<Peg> GetAllActiveAndValidConversionPegsForType(Peg.PegType pegType, List<Peg> avoidPegs = null)
		{
			List<Peg> list = new List<Peg>();
			foreach (Peg allPeg in _allPegs)
			{
				if (IsPegActiveAndAValidConversionPegForType(allPeg, pegType, avoidPegs))
				{
					list.Add(allPeg);
				}
			}
			return list;
		}

		private bool IsPegActiveAndAValidConversionPegForType(Peg p, Peg.PegType pegType, List<Peg> avoidPegs = null)
		{
			if (!p.IsDisabled() && p.gameObject.activeInHierarchy && p.SupportsPegType(pegType))
			{
				if (avoidPegs != null)
				{
					return !avoidPegs.Contains(p);
				}
				return true;
			}
			return false;
		}

		private List<Peg> GetAllActiveAndValidConversionSpecialPegBlockerPegs(Peg.PegType pegType, List<Peg> avoidPegs = null)
		{
			List<Peg> list = new List<Peg>();
			foreach (Peg allPeg in _allPegs)
			{
				if (IsPegActiveAndAValidConversionPegForType(allPeg, pegType, avoidPegs))
				{
					continue;
				}
				RegularPeg regularPeg = allPeg as RegularPeg;
				if (regularPeg != null && regularPeg.specialPegBlocker)
				{
					regularPeg.specialPegBlocker = false;
					bool num = IsPegActiveAndAValidConversionPegForType(regularPeg, pegType, avoidPegs);
					regularPeg.specialPegBlocker = true;
					if (num)
					{
						list.Add(regularPeg);
					}
				}
			}
			return list;
		}

		public void ResetPegs(bool crit)
		{
			int num = 0;
			int num2 = int.MaxValue;
			if (_relicManager.RelicEffectActive(RelicEffect.ONLY_REFRESH_X_PEGS))
			{
				int num3 = _allPegs.Count;
				if (_relicManager.RelicEffectActive(RelicEffect.BOMBS_RESPAWN))
				{
					num3 += _bombs.Count;
				}
				num2 = Mathf.Max(5, Mathf.RoundToInt((float)num3 * 0.08f));
				_allPegs.Shuffle();
				_bombs.Shuffle();
			}
			List<Bomb> list = new List<Bomb>();
			if (_relicManager.AttemptUseRelic(RelicEffect.BOMBS_RESPAWN))
			{
				foreach (Bomb bomb in _bombs)
				{
					if (!bomb.CanResetBomb())
					{
						continue;
					}
					if (num >= num2 && _relicManager.AttemptUseRelic(RelicEffect.ONLY_REFRESH_X_PEGS))
					{
						break;
					}
					if (bomb == null)
					{
						list.Add(bomb);
						continue;
					}
					bomb.Reset();
					if (!_isLayoutAlternating || ((!_battleController.pegLayoutAlternator.pegboardAActive || _battleController.pegLayoutAlternator.pegboardAPegs.Contains(bomb)) && (_battleController.pegLayoutAlternator.pegboardAActive || _battleController.pegLayoutAlternator.pegboardBPegs.Contains(bomb))))
					{
						num++;
					}
				}
			}
			foreach (Bomb item in list)
			{
				_bombs.Remove(item);
			}
			foreach (Peg allPeg in _allPegs)
			{
				if (num >= num2 && _relicManager.AttemptUseRelic(RelicEffect.ONLY_REFRESH_X_PEGS))
				{
					break;
				}
				if (!(allPeg == _lastHitResetPeg) && ResetPeg(allPeg, crit) && (!_isLayoutAlternating || ((!_battleController.pegLayoutAlternator.pegboardAActive || _battleController.pegLayoutAlternator.pegboardAPegs.Contains(allPeg)) && (_battleController.pegLayoutAlternator.pegboardAActive || _battleController.pegLayoutAlternator.pegboardBPegs.Contains(allPeg)))))
				{
					num++;
					if (delayedDeathPegs.Contains(allPeg))
					{
						(allPeg as LongPeg).HardReset();
						delayedDeathPegs.Remove(allPeg);
					}
					if (_relicManager.AttemptUseRelic(RelicEffect.REFRESH_UPGRADES_PEGS))
					{
						allPeg.AddBuff(1);
					}
				}
			}
			_lastHitResetPeg = null;
			if (num > 0)
			{
				if (_relicManager.AttemptUseRelic(RelicEffect.REFRESH_DAMAGES_PEG_COUNT))
				{
					Enemy.OnAllEnemiesDamaged?.Invoke(num);
				}
				OnPegsRefreshed?.Invoke(num);
				if (StaticGameData.CurrentRunStats != null && StaticGameData.CurrentRunStats.isRunActive)
				{
					StaticGameData.CurrentRunStats.pegsRefreshed += num;
				}
			}
			if (_relicManager.AttemptUseRelic(RelicEffect.CREATE_GOLD_ON_REFRESH))
			{
				ApplyGoldToPegs(2);
			}
			if (lastHitResetPeg != null)
			{
				delayedDeathPegs.Add(lastHitResetPeg);
			}
		}

		public void ResetDelayedDeathPegs()
		{
			foreach (Peg delayedDeathPeg in delayedDeathPegs)
			{
				LongPeg longPeg = delayedDeathPeg as LongPeg;
				if ((bool)longPeg)
				{
					if (longPeg.ShouldPopPegOnHit())
					{
						longPeg.SetActiveStatus(active: false);
					}
					else if (!longPeg.IsDisabled())
					{
						longPeg.HardReset();
					}
					else
					{
						longPeg.ResetHitCount();
					}
				}
			}
			delayedDeathPegs.Clear();
		}

		public void BombsThrown()
		{
			foreach (Bomb bomb in _bombs)
			{
				bomb.BombsThrown();
			}
		}

		protected bool ResetPeg(Peg peg, bool crit)
		{
			peg.ResetHitCount();
			if (peg.IsPopped() && peg.pegType != Peg.PegType.DESTROYED)
			{
				if (!_relicManager.RelicEffectActive(RelicEffect.PEG_TO_BOMB) || !peg.SupportsPegType(Peg.PegType.BOMB))
				{
					if (!peg.gameObject.activeInHierarchy)
					{
						peg.gameObject.SetActive(value: true);
					}
					peg.Reset(crit);
					return true;
				}
				if (!(UnityEngine.Random.value < _relicManager.PEG_TO_BOMB_CHANCE))
				{
					peg.Reset(crit);
					return true;
				}
				_relicManager.AttemptUseRelic(RelicEffect.PEG_TO_BOMB);
				peg.ConvertPegToType(Peg.PegType.BOMB);
				Peg.OnPegTypeConverted?.Invoke(Peg.PegType.BOMB, peg.GetCenterOfPeg(), _relicManager.RelicEffectActive(RelicEffect.ALL_BOMBS_RIGGED));
				ExternalBombCreatedNotification?.Invoke();
			}
			else if (peg.IsDelayedDeath() && peg.pegType != Peg.PegType.CRIT && peg.pegType != Peg.PegType.RESET)
			{
				bool result = !Peg.PegTypeIndestructible(peg.pegType) && peg.IsWaitingForDeath();
				peg.Reset(crit);
				return result;
			}
			return false;
		}

		public void ConvertPegsToBombs(int numToConvert, bool rigged = false, bool seeded = true)
		{
			List<Peg> allActiveAndValidConversionPegsForType = GetAllActiveAndValidConversionPegsForType(Peg.PegType.BOMB);
			int num = Mathf.Min(numToConvert, allActiveAndValidConversionPegsForType.Count);
			if (seeded)
			{
				allActiveAndValidConversionPegsForType.Sort((Peg x, Peg y) => (int)Mathf.Sign(x.GetInstanceID() - y.GetInstanceID()));
				specialPegShufflingRandomState.SeededShuffle(allActiveAndValidConversionPegsForType);
			}
			else
			{
				allActiveAndValidConversionPegsForType.Shuffle();
			}
			List<Peg> list = new List<Peg>();
			foreach (Peg item in allActiveAndValidConversionPegsForType)
			{
				if (num <= 0)
				{
					break;
				}
				if (item.SupportsPegType(Peg.PegType.BOMB))
				{
					item.ConvertPegToType(Peg.PegType.BOMB);
					if (rigged)
					{
						item.GetComponentInChildren<Bomb>().ConvertToRigged();
					}
					Peg.OnPegTypeConverted?.Invoke(Peg.PegType.BOMB, item.GetCenterOfPeg(), rigged);
					list.Add(item);
					num--;
				}
			}
		}

		public void ConvertPegToMaxHpPeg(int numToConvert)
		{
			List<Peg> allActiveAndValidConversionPegsForType = GetAllActiveAndValidConversionPegsForType(Peg.PegType.MAX_HP);
			int num = Mathf.Min(numToConvert, allActiveAndValidConversionPegsForType.Count);
			allActiveAndValidConversionPegsForType.Shuffle();
			List<Peg> list = new List<Peg>();
			foreach (Peg item in allActiveAndValidConversionPegsForType)
			{
				if (num <= 0)
				{
					break;
				}
				if (item.SupportsPegType(Peg.PegType.MAX_HP))
				{
					item.ConvertPegToType(Peg.PegType.MAX_HP);
					Peg.OnPegTypeConverted?.Invoke(Peg.PegType.MAX_HP, item.GetCenterOfPeg());
					list.Add(item);
					num--;
				}
			}
		}

		public int CountSpecialPegsOnBoard(Peg.PegType pegType, bool riggedBomb = false)
		{
			int num = 0;
			foreach (Peg allPeg in _allPegs)
			{
				if (allPeg.pegType == pegType && !allPeg.IsDisabled())
				{
					num++;
				}
			}
			if (pegType == Peg.PegType.BOMB)
			{
				foreach (Bomb bomb2 in _bombs)
				{
					Bomb bomb = bomb2 as Bomb;
					if (bomb != null && !bomb2.IsDisabled() && bomb.isRigged == riggedBomb)
					{
						num++;
					}
				}
			}
			return num;
		}

		public (int, int, int) CountShieldPegsOnBoard()
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (Peg allPeg in _allPegs)
			{
				if (allPeg.IsDisabled() || !allPeg.shielded)
				{
					continue;
				}
				PegShieldOverlay componentInChildren = allPeg.GetComponentInChildren<PegShieldOverlay>();
				if (componentInChildren != null && componentInChildren.CheckShieldActive())
				{
					num++;
					if (allPeg.buffAmount >= 0)
					{
						num2 += allPeg.buffAmount;
					}
					if (allPeg.buffAmount < 0)
					{
						num3 += allPeg.buffAmount;
					}
				}
			}
			return (num, num2, num3);
		}

		public void BreakAllShieldPegs()
		{
			foreach (Peg allPeg in _allPegs)
			{
				allPeg.BreakShieldPegImmediately();
			}
		}

		public void RemoveSlimeTypeFromAllPegs(Peg.SlimeType slimeTypeToRemove)
		{
			foreach (Peg allPeg in _allPegs)
			{
				if (allPeg.slimeType == slimeTypeToRemove)
				{
					allPeg.RemoveSlime(forceRemoval: true);
				}
			}
			foreach (BouncerPeg bouncerPeg in _bouncerPegs)
			{
				if (bouncerPeg.slimeType == slimeTypeToRemove)
				{
					bouncerPeg.RemoveSlime(forceRemoval: true);
				}
			}
		}

		public List<Peg> GetPegsToTurnIntoDullPegs(int amount, bool canDullPoppedPegs = true, List<RegularPeg> pegList = null)
		{
			List<Peg> list = new List<Peg>();
			if (amount <= 0)
			{
				return list;
			}
			int num = amount;
			List<Peg> list2 = ((pegList != null) ? new List<Peg>(pegList.ToArray()) : new List<Peg>(_allPegs.ToArray()));
			list2.Shuffle();
			Peg firstConvertedPeg = null;
			foreach (Peg item in list2)
			{
				if ((canDullPoppedPegs || !item.IsDisabled()) && item.SupportsPegType(Peg.PegType.DULL))
				{
					item.ConvertPegToType(Peg.PegType.DULL);
					firstConvertedPeg = item;
					num--;
					list.Add(item);
					break;
				}
			}
			if (firstConvertedPeg == null || num == 0)
			{
				return list;
			}
			list2.Sort((Peg x, Peg y) => (int)Mathf.Sign((x.transform.position - firstConvertedPeg.transform.position).sqrMagnitude - (y.transform.position - firstConvertedPeg.transform.position).sqrMagnitude));
			foreach (Peg item2 in list2)
			{
				if ((canDullPoppedPegs || !item2.IsDisabled()) && item2.SupportsPegType(Peg.PegType.DULL) && !list.Contains(item2))
				{
					item2.ConvertPegToType(Peg.PegType.DULL);
					num--;
					list.Add(item2);
				}
				if (num == 0)
				{
					break;
				}
			}
			return list;
		}

		public void ConvertAllCrystalPegsToRegular()
		{
			foreach (Peg allPeg in _allPegs)
			{
				if (allPeg.pegType == Peg.PegType.CRYSTAL)
				{
					allPeg.Reset(BattleController.criticalActive);
				}
			}
		}

		public List<Peg> GetPegsToTurnIntoSpinfectionPegs(int amount, List<Peg> pegsList = null)
		{
			List<Peg> list = new List<Peg>();
			if (pegsList != null && pegsList.Count > 0)
			{
				foreach (Peg allPeg in _allPegs)
				{
					if (!pegsList.Contains(allPeg))
					{
						list.Add(allPeg);
					}
				}
			}
			List<Peg> list2 = new List<Peg>();
			List<Peg> allActiveAndValidConversionPegsForType = GetAllActiveAndValidConversionPegsForType(Peg.PegType.SPINFECTION, list);
			int count = allActiveAndValidConversionPegsForType.Count;
			for (int i = 0; i < Mathf.Min(amount, count); i++)
			{
				Peg item = allActiveAndValidConversionPegsForType[UnityEngine.Random.Range(0, allActiveAndValidConversionPegsForType.Count)];
				list2.Add(item);
				allActiveAndValidConversionPegsForType.Remove(item);
			}
			return list2;
		}

		public bool IsThereAnOngoingLightningSlimeChain()
		{
			foreach (Peg allPeg in _allPegs)
			{
				if (allPeg.lightningSlimeChainGoing)
				{
					return true;
				}
			}
			return false;
		}

		public void ResolveCritiqualibriumRelic(Attack currentOrbAttack)
		{
			if (!_relicManager.AttemptUseRelic(RelicEffect.ADD_OR_REMOVE_CRIT))
			{
				return;
			}
			int pegCount = GetPegCount(Peg.PegType.CRIT);
			pegCount = ((!(currentOrbAttack != null) || currentOrbAttack.GetModifiedDamagePerPeg() <= currentOrbAttack.GetModifiedDamagePerPeg(1)) ? (pegCount + 1) : (pegCount - 1));
			int num = Mathf.Abs(pegCount - _critPegs.Count);
			if (_critPegs.Count > pegCount)
			{
				for (int i = 0; i < num; i++)
				{
					Peg peg = _critPegs[UnityEngine.Random.Range(0, _critPegs.Count)];
					peg.Reset(BattleController.criticalActive);
					_critPegs.Remove(peg);
				}
			}
			else if (_critPegs.Count < pegCount)
			{
				for (int j = 0; j < num; j++)
				{
					Peg item = CreateSpecialPeg(Peg.PegType.CRIT, GetAllActiveAndValidConversionPegsForType(Peg.PegType.CRIT));
					_critPegs.Add(item);
				}
			}
		}

		public void ConvertAllPegsToSpecialPeg(Peg.PegType type)
		{
			foreach (Peg allPeg in _allPegs)
			{
				if (!allPeg.IsDisabled() && allPeg.gameObject.activeInHierarchy)
				{
					allPeg.ConvertPegToType(type);
				}
			}
		}

		public void ApplySlimeToAllPegs(Peg.SlimeType type)
		{
			foreach (Peg allPeg in _allPegs)
			{
				if (!allPeg.IsDisabled() && allPeg.gameObject.activeInHierarchy)
				{
					allPeg.ApplySlimeToPeg(type);
				}
			}
			foreach (BouncerPeg bouncerPeg in _bouncerPegs)
			{
				if (!bouncerPeg.IsDisabled() && bouncerPeg.gameObject.activeInHierarchy)
				{
					bouncerPeg.ApplySlimeToPeg(type);
				}
			}
		}

		private void OnExternalBombCreatedNotification()
		{
			if (_relicManager != null && _relicManager.AttemptUseRelic(RelicEffect.CREATE_ADDITIONAL_BOMB))
			{
				List<Peg> allActiveAndValidConversionPegsForType = GetAllActiveAndValidConversionPegsForType(Peg.PegType.BOMB);
				Peg peg = CreateSpecialPeg(Peg.PegType.BOMB, allActiveAndValidConversionPegsForType, seeded: true);
				if (peg != null)
				{
					Peg.OnPegTypeConverted?.Invoke(Peg.PegType.BOMB, peg.GetCenterOfPeg(), _relicManager.RelicEffectActive(RelicEffect.ALL_BOMBS_RIGGED));
				}
			}
		}

		private void HandlePegCollisionHit(Peg.PegType type, Peg peg)
		{
			if (type != Peg.PegType.CRIT || peg.isBonusSpecialPeg || !_relicManager.AttemptUseRelic(RelicEffect.HITTING_CRIT_ADDS_TEMP_CRITS))
			{
				return;
			}
			foreach (Peg item in CreateBonusSpecialPegs(Peg.PegType.CRIT, 2))
			{
				Peg.OnPegTypeConverted?.Invoke(Peg.PegType.CRIT, item.GetCenterOfPeg());
			}
		}

		protected void HandlePegActivated(Peg.PegType type, Peg peg)
		{
			if (StaticGameData.CurrentRunStats != null)
			{
				StaticGameData.CurrentRunStats.pegsHit++;
			}
			if (peg != null && BattleController.CurrentBattleState != BattleController.BattleState.AWAITING_SHOT && peg.IsDelayedDeath() && !delayedDeathPegs.Contains(peg))
			{
				delayedDeathPegs.Add(peg);
			}
			switch (type)
			{
			case Peg.PegType.CRIT:
				if (StaticGameData.CurrentRunStats != null)
				{
					StaticGameData.CurrentRunStats.pegsHitCrit++;
				}
				if (_relicManager.RelicEffectActive(RelicEffect.CRIT_ALSO_REFRESH))
				{
					_lastHitResetPeg = peg;
				}
				if (_critPegs.Contains(peg))
				{
					_critPegs.Remove(peg);
				}
				break;
			case Peg.PegType.RESET:
				if (StaticGameData.CurrentRunStats != null)
				{
					StaticGameData.CurrentRunStats.pegsHitRefresh++;
				}
				_lastHitResetPeg = peg;
				if (_resetPegs.Contains(peg))
				{
					_resetPegs.Remove(peg);
				}
				break;
			}
		}
	}
}
