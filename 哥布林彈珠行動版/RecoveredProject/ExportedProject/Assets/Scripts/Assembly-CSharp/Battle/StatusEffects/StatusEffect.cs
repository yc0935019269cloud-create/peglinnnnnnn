using System;
using System.Collections.Generic;
using Relics;

namespace Battle.StatusEffects
{
	[Serializable]
	public class StatusEffect
	{
		private sealed class EffectTypeEqualityComparer : IEqualityComparer<StatusEffect>
		{
			public bool Equals(StatusEffect x, StatusEffect y)
			{
				if (x == y)
				{
					return true;
				}
				if (x == null)
				{
					return false;
				}
				if (y == null)
				{
					return false;
				}
				if (x.GetType() != y.GetType())
				{
					return false;
				}
				return x.EffectType == y.EffectType;
			}

			public int GetHashCode(StatusEffect obj)
			{
				return (int)obj.EffectType;
			}
		}

		private const int MAX_INTENSITY = 999;

		public StatusEffectType EffectType;

		public int Intensity;

		public static IEqualityComparer<StatusEffect> EffectTypeComparer { get; } = new EffectTypeEqualityComparer();

		public StatusEffect(StatusEffect prevEffect)
		{
			EffectType = prevEffect.EffectType;
			Intensity = prevEffect.Intensity;
		}

		public StatusEffect(StatusEffectType effectType = StatusEffectType.None, int intensity = 1)
		{
			EffectType = effectType;
			Intensity = Math.Min(intensity, GetMaxIntensityForEffect(effectType));
		}

		public static bool IsStatusEffectPerishable(StatusEffectType effectType)
		{
			return effectType switch
			{
				StatusEffectType.Stunned => true, 
				StatusEffectType.Thorned => true, 
				StatusEffectType.Confusion => true, 
				StatusEffectType.Poison => true, 
				_ => false, 
			};
		}

		public static bool IsStatusEffectEphemeral(StatusEffectType effectType)
		{
			if ((uint)(effectType - 5) <= 1u)
			{
				return true;
			}
			return false;
		}

		public static bool IsStatusEffectDebuff(StatusEffectType effectType)
		{
			switch (effectType)
			{
			case StatusEffectType.Thorned:
			case StatusEffectType.Stunned:
			case StatusEffectType.Blind:
			case StatusEffectType.Confusion:
			case StatusEffectType.Slimed:
			case StatusEffectType.Webbed:
			case StatusEffectType.Poison:
			case StatusEffectType.FlamePegs:
			case StatusEffectType.Exploitaball:
			case StatusEffectType.Transpherency:
				return true;
			default:
				return false;
			}
		}

		public static bool IsStatusEffectBuff(StatusEffectType effectType)
		{
			if ((uint)(effectType - 8) <= 4u || effectType == StatusEffectType.Ballusion || effectType == StatusEffectType.Lifelink)
			{
				return true;
			}
			return false;
		}

		public static int GetMaxIntensityForEffect(StatusEffectType effectType, RelicManager relicManager = null)
		{
			switch (effectType)
			{
			case StatusEffectType.Ballusion:
				return 999;
			case StatusEffectType.Blind:
				return 100;
			case StatusEffectType.Poison:
			case StatusEffectType.Intangiball:
				return 99000;
			default:
				return 999;
			}
		}
	}
}
