using System;
using Battle.Attacks;
using Battle.Attacks.DamageModifiers;
using Battle.StatusEffects;
using I2.Loc;
using Relics;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class SnowballDmgAndSize : AttackBaseDamageModifier
	{
		public delegate void SnowballSizeIncreased(SpriteRenderer spriteRenderer);

		public static SnowballSizeIncreased OnSnowballSizeIncreased = (SpriteRenderer spriteRenderer) =>
		{
		};

		public int[] hitsToIncreaseToTheNextSize = new int[4] { 10, 20, 40, 80 };

		public int hitsPerDamageIncrease = 10;

		public int baseDamageIncrease = 1;

		public int critDamageIncrease = 2;

		[Space(15f)]
		public SpriteRenderer pachickoSpriteRenderer;

		public CircleCollider2D pachinkoCircleCollider;

		public MultiballsGetSmaller.OrbSizes _startingOrbSize;

		public Sprite[] orbSizeSprites;

		public float[] orbColliderRadius;

		public Vector3[] orbSpritePositionOffsetsForSizes;

		public static int allSnowballsCurrentSizeIndex = 0;

		private static int allSnowballsCurrentBaseDmgIncrease = 0;

		private static int allSnowballsCurrentCritDmgIncrease = 0;

		private static int currentHitsToNextSize = 0;

		private static int currentHitsForDamageScalingLvl1 = 0;

		private static int currentHitsForDamageScalingLvl2 = 0;

		private static int currentHitsForDamageScalingLvl3 = 0;

		private PachinkoBall _pachinkoBall;

		private Attack _attack;

		private LocalizationParamsManager locParams;

		private void Awake()
		{
			_pachinkoBall = GetComponent<PachinkoBall>();
			_attack = GetComponent<Attack>();
			UpdateSize();
		}

		private void OnEnable()
		{
			PachinkoBall pachinkoBall = _pachinkoBall;
			pachinkoBall.OnPachinkoBallPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Combine(pachinkoBall.OnPachinkoBallPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
			BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Combine(BattleController.OnBattleEnded, new BattleController.BattleEvent(ResetStaticVariables));
			OnSnowballSizeIncreased = (SnowballSizeIncreased)Delegate.Combine(OnSnowballSizeIncreased, new SnowballSizeIncreased(OnGlobalSnowballSizeIncrease));
			UpdateSize();
		}

		private void OnDisable()
		{
			PachinkoBall pachinkoBall = _pachinkoBall;
			pachinkoBall.OnPachinkoBallPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Remove(pachinkoBall.OnPachinkoBallPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
			BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Remove(BattleController.OnBattleEnded, new BattleController.BattleEvent(ResetStaticVariables));
			OnSnowballSizeIncreased = (SnowballSizeIncreased)Delegate.Remove(OnSnowballSizeIncreased, new SnowballSizeIncreased(OnGlobalSnowballSizeIncrease));
		}

		public static void ResetStaticVariables()
		{
			allSnowballsCurrentSizeIndex = 0;
			allSnowballsCurrentBaseDmgIncrease = 0;
			allSnowballsCurrentCritDmgIncrease = 0;
			currentHitsToNextSize = 0;
			currentHitsForDamageScalingLvl1 = 0;
			currentHitsForDamageScalingLvl2 = 0;
			currentHitsForDamageScalingLvl3 = 0;
		}

		private void PegHit(Peg peg, PachinkoBall pBall)
		{
			if (_pachinkoBall.IsDummy || peg.pegType != Peg.PegType.REGULAR || !(pBall == _pachinkoBall))
			{
				return;
			}
			if (allSnowballsCurrentSizeIndex < hitsToIncreaseToTheNextSize.Length)
			{
				currentHitsToNextSize++;
				if (currentHitsToNextSize == hitsToIncreaseToTheNextSize[allSnowballsCurrentSizeIndex])
				{
					IncreaseSize();
					currentHitsToNextSize = 0;
				}
			}
			switch (_attack.Level)
			{
			case 1:
				currentHitsForDamageScalingLvl1++;
				if (currentHitsForDamageScalingLvl1 == hitsPerDamageIncrease)
				{
					ScaleDamage();
					currentHitsForDamageScalingLvl1 = 0;
				}
				break;
			case 2:
				currentHitsForDamageScalingLvl2++;
				if (currentHitsForDamageScalingLvl2 == hitsPerDamageIncrease)
				{
					ScaleDamage();
					currentHitsForDamageScalingLvl2 = 0;
				}
				break;
			case 3:
				currentHitsForDamageScalingLvl3++;
				if (currentHitsForDamageScalingLvl3 == hitsPerDamageIncrease)
				{
					ScaleDamage();
					currentHitsForDamageScalingLvl3 = 0;
				}
				break;
			}
		}

		private void IncreaseSize()
		{
			allSnowballsCurrentSizeIndex++;
			if (allSnowballsCurrentSizeIndex >= orbSizeSprites.Length)
			{
				allSnowballsCurrentSizeIndex = orbSizeSprites.Length - 1;
				return;
			}
			UpdateSize();
			OnSnowballSizeIncreased?.Invoke(pachickoSpriteRenderer);
		}

		private void ScaleDamage()
		{
			allSnowballsCurrentBaseDmgIncrease += baseDamageIncrease;
			allSnowballsCurrentCritDmgIncrease += critDamageIncrease;
		}

		public void UpdateSize()
		{
			int num = (int)(_startingOrbSize - allSnowballsCurrentSizeIndex);
			pachickoSpriteRenderer.sprite = orbSizeSprites[allSnowballsCurrentSizeIndex];
			pachinkoCircleCollider.radius = orbColliderRadius[allSnowballsCurrentSizeIndex];
			float num2 = MultiballsGetSmaller.SPRITE_SCALE_FOR_SIZES[num];
			pachickoSpriteRenderer.transform.localScale = new Vector3(num2, num2, pachickoSpriteRenderer.transform.localScale.z);
		}

		private void OnGlobalSnowballSizeIncrease(SpriteRenderer snowballSpriteRenderer)
		{
			UpdateSize();
		}

		public override int GetDamageMod(DeckManager deckManager, RelicManager relicManager, PlayerHealthController healthController, PlayerStatusEffectController statusEffectController, int critCount = 0)
		{
			if (critCount != 0)
			{
				return allSnowballsCurrentCritDmgIncrease;
			}
			return allSnowballsCurrentBaseDmgIncrease;
		}

		private void OnValidate()
		{
			if (locParams == null)
			{
				locParams = GetComponent<LocalizationParamsManager>();
			}
			locParams.SetParameterValue("SNOWBALL_BASE_DAMAGE_INCREASE", baseDamageIncrease.ToString());
			locParams.SetParameterValue("SNOWBALL_CRIT_DAMAGE_INCREASE", critDamageIncrease.ToString());
			locParams.SetParameterValue("SNOWBALL_DAMAGE_HITS_NEEDED", hitsPerDamageIncrease.ToString());
		}
	}
}
