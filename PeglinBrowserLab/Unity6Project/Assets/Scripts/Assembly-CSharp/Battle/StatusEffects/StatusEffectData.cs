using Peglin.OdinSerializer.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Battle.StatusEffects
{
	[CreateAssetMenu(menuName = "Battle/Status Effect Data")]
	public class StatusEffectData : ScriptableObject
	{
		public static readonly float BLIND_BASE_MISS_CHANCE = 0f;

		public static readonly float BLIND_ADDITIONAL_PER_STAT = 0.01f;

		public static readonly float POISON_DAMAGE_MULTIPLIER = 10f;

		public static readonly float BALLUSION_EVASION_PER_STACK = 0.01f;

		public static readonly float BALLUSION_REDUCTION_PERCENTAGE = 0.5f;

		public static readonly float EXPLOITABALL_BONUS_DAMAGE_PER_STACK = 0.1f;

		private static readonly string NAME_SUFFIX = "_name";

		private static readonly string DESC_SUFFIX = "_desc";

		public Sprite StunnedStatusIcon;

		[FormerlySerializedAs("StunnedLocDesc")]
		public string StunnedLocKey;

		public Sprite ReflectStatusIcon;

		[FormerlySerializedAs("ReflectLocDesc")]
		public string ReflectLocKey;

		public Sprite PoisonIcon;

		[FormerlySerializedAs("PoisonLocDesc")]
		public string PoisonLocKey;

		[FormerlySerializedAs("PlayerPoisonLocDesc")]
		public string PlayerPoisonLocKey;

		public Sprite ExploitaballIcon;

		[FormerlySerializedAs("ExploitaballLocDesc")]
		public string ExploitaballLocKey;

		public Sprite TranspherencyIcon;

		[FormerlySerializedAs("TranspherencyLocDesc")]
		public string TranspherencyLocKey;

		public Sprite ConfusionStatusIcon;

		[FormerlySerializedAs("ConfusionLocDesc")]
		public string ConfusionLocKey;

		public Sprite BalanceIcon;

		[FormerlySerializedAs("BalanceLocDesc")]
		public string BalanceLocKey;

		public Sprite FinesseIcon;

		[FormerlySerializedAs("FinesseLocDesc")]
		public string FinesseLocKey;

		public Sprite DexterityIcon;

		[FormerlySerializedAs("DexterityLocDesc")]
		public string DexterityLocKey;

		public Sprite BallwarkIcon;

		[FormerlySerializedAs("BallwarkLocDesc")]
		public string BallwarkLocKey;

		[FormerlySerializedAs("DmgBuffIcon")]
		public Sprite StrengthIcon;

		[FormerlySerializedAs("StrengthLocDesc")]
		public string StrengthLocKey;

		public Sprite BallusionIcon;

		[FormerlySerializedAs("BallusionLocDesc")]
		public string BallusionLocKey;

		public Sprite IntangiballIcon;

		[FormerlySerializedAs("IntangiballLocDesc")]
		public string IntangiballLocKey;

		public Sprite ThornedStatusIcon;

		[FormerlySerializedAs("ThornedLocDesc")]
		public string ThornedLocKey;

		[FormerlySerializedAs("PlayerThornedLocDesc")]
		public string PlayerThornedLocKey;

		public Sprite BlindStatusIcon;

		[FormerlySerializedAs("BlindLocDesc")]
		public string BlindLocKey;

		[FormerlySerializedAs("PlayerBlindLocDesc")]
		public string PlayerBlindLocKey;

		public Sprite LifelinkIcon;

		public string LifelinkLocKey;

		public string PlayerLifelinkLocKey;

		public Sprite BalltiplierIcon;

		public string BalltiplierLocKey = "balltiplier";

		public Sprite GetStatusEffectIcon(StatusEffectType type)
		{
			return type switch
			{
				StatusEffectType.Thorned => ThornedStatusIcon, 
				StatusEffectType.Stunned => StunnedStatusIcon, 
				StatusEffectType.Blind => BlindStatusIcon, 
				StatusEffectType.Reflect => ReflectStatusIcon, 
				StatusEffectType.Confusion => ConfusionStatusIcon, 
				StatusEffectType.Strength => StrengthIcon, 
				StatusEffectType.Dexterity => DexterityIcon, 
				StatusEffectType.Finesse => FinesseIcon, 
				StatusEffectType.Balance => BalanceIcon, 
				StatusEffectType.Ballwark => BallwarkIcon, 
				StatusEffectType.Poison => PoisonIcon, 
				StatusEffectType.Ballusion => BallusionIcon, 
				StatusEffectType.Intangiball => IntangiballIcon, 
				StatusEffectType.Exploitaball => ExploitaballIcon, 
				StatusEffectType.Transpherency => TranspherencyIcon, 
				StatusEffectType.Lifelink => LifelinkIcon, 
				StatusEffectType.Multiattack => BalltiplierIcon, 
				_ => null, 
			};
		}

		public string GetStatusEffectDesc(StatusEffectType type, bool isOnPlayer)
		{
			if (!isOnPlayer)
			{
				return GetStatusEffectDescOnEnemy(type);
			}
			return GetStatusEffectDescOnPlayer(type);
		}

		public string GetStatusEffectDescOnEnemy(StatusEffectType type)
		{
			string text = type switch
			{
				StatusEffectType.Thorned => ThornedLocKey, 
				StatusEffectType.Stunned => StunnedLocKey, 
				StatusEffectType.Blind => BlindLocKey, 
				StatusEffectType.Reflect => ReflectLocKey, 
				StatusEffectType.Confusion => ConfusionLocKey, 
				StatusEffectType.Strength => StrengthLocKey, 
				StatusEffectType.Dexterity => DexterityLocKey, 
				StatusEffectType.Finesse => FinesseLocKey, 
				StatusEffectType.Balance => BalanceLocKey, 
				StatusEffectType.Ballwark => BallwarkLocKey, 
				StatusEffectType.Poison => PoisonLocKey, 
				StatusEffectType.Ballusion => BallusionLocKey, 
				StatusEffectType.Intangiball => IntangiballLocKey, 
				StatusEffectType.Exploitaball => ExploitaballLocKey, 
				StatusEffectType.Transpherency => TranspherencyLocKey, 
				StatusEffectType.Lifelink => LifelinkLocKey, 
				StatusEffectType.Multiattack => BalltiplierLocKey, 
				_ => "", 
			};
			if (!text.IsNullOrWhitespace())
			{
				return text + DESC_SUFFIX;
			}
			return "";
		}

		public string GetStatusEffectDescOnPlayer(StatusEffectType type)
		{
			string text = type switch
			{
				StatusEffectType.Thorned => PlayerThornedLocKey, 
				StatusEffectType.Blind => PlayerBlindLocKey, 
				StatusEffectType.Confusion => ConfusionLocKey, 
				StatusEffectType.Strength => StrengthLocKey, 
				StatusEffectType.Dexterity => DexterityLocKey, 
				StatusEffectType.Finesse => FinesseLocKey, 
				StatusEffectType.Balance => BalanceLocKey, 
				StatusEffectType.Ballwark => BallwarkLocKey, 
				StatusEffectType.Poison => PlayerPoisonLocKey, 
				StatusEffectType.Ballusion => BallusionLocKey, 
				StatusEffectType.Intangiball => IntangiballLocKey, 
				StatusEffectType.Exploitaball => ExploitaballLocKey, 
				StatusEffectType.Transpherency => TranspherencyLocKey, 
				_ => "", 
			};
			if (!text.IsNullOrWhitespace())
			{
				return text + DESC_SUFFIX;
			}
			return "";
		}

		public string GetStatusEffectName(StatusEffectType type)
		{
			string text = type switch
			{
				StatusEffectType.Thorned => ThornedLocKey, 
				StatusEffectType.Stunned => StunnedLocKey, 
				StatusEffectType.Blind => BlindLocKey, 
				StatusEffectType.Reflect => ReflectLocKey, 
				StatusEffectType.Confusion => ConfusionLocKey, 
				StatusEffectType.Strength => StrengthLocKey, 
				StatusEffectType.Dexterity => DexterityLocKey, 
				StatusEffectType.Finesse => FinesseLocKey, 
				StatusEffectType.Balance => BalanceLocKey, 
				StatusEffectType.Ballwark => BallwarkLocKey, 
				StatusEffectType.Poison => PoisonLocKey, 
				StatusEffectType.Ballusion => BallusionLocKey, 
				StatusEffectType.Intangiball => IntangiballLocKey, 
				StatusEffectType.Exploitaball => ExploitaballLocKey, 
				StatusEffectType.Transpherency => TranspherencyLocKey, 
				_ => "", 
			};
			if (!text.IsNullOrWhitespace())
			{
				return text + NAME_SUFFIX;
			}
			return "";
		}
	}
}
