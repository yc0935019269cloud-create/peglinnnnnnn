using System.Collections.Generic;
using System.Globalization;
using Battle.Attacks;
using Battle.Attacks.DamageModifiers;
using Battle.Pachinko.BallBehaviours;
using Battle.StatusEffects;
using Challenges;
using I2.Loc;
using MoreMountains.Tools;
using PeglinUI.OrbDisplay;
using Relics;
using Saving;
using TMPro;
using UnityEngine;

namespace PeglinUI
{
	public class Tooltip : MonoBehaviour
	{
		public enum LockedDescriptionPresets
		{
			RELIC_DEFAULT = 0,
			ORB_DEFAULT = 1,
			WIN_WITH_RELIC = 2,
			WIN_WITH_ORB = 3,
			FIND_IN_SCENARIO = 4,
			PLAY_SEED = 5,
			HIGHER_CRUCIBALL = 6,
			THREE_QUESTION_MARKS = 7
		}

		public static Color32 DEFAULT_DAMAGE_COLOR = Color.black;

		public static Color32 BETTER_DAMAGE_COLOR = new Color32(0, 106, 0, byte.MaxValue);

		public static Color32 WORSE_DAMAGE_COLOR = new Color32(132, 0, 0, byte.MaxValue);

		public static readonly string RELIC_LOCKED_BASE_KEY = "Menu/locked_relic_description";

		public static readonly string LOCKED_ORB_DESC_BASE_KEY = "Menu/locked_orb_description";

		public static readonly string WIN_WITH_RELIC_KEY = "Menu/win_with_relic_to_unlock";

		public static readonly string WIN_WITH_ORB_KEY = "Menu/win_with_orb_to_unlock";

		public static readonly string FIND_IN_SCENARIO_KEY = "Menu/find_in_scenario_to_unlock";

		public static readonly string PLAY_SEED_KEY = "Menu/play_seed_to_unlock";

		public static readonly string HIGHER_CRUCIBALL = "Menu/play_higher_cruciball_to_unlock";

		public static readonly string SHOP_ONLY_KEY = "Menu/locked_relic_shop_only";

		public static readonly string ASSEMBALL_KEY = "assemball";

		public static readonly string THREE_QUESTION_MARKS = "Menu/three_question_marks";

		public static readonly Dictionary<LockedDescriptionPresets, string> LockedDescPresetToKeyMap = new Dictionary<LockedDescriptionPresets, string>
		{
			{
				LockedDescriptionPresets.RELIC_DEFAULT,
				RELIC_LOCKED_BASE_KEY
			},
			{
				LockedDescriptionPresets.ORB_DEFAULT,
				LOCKED_ORB_DESC_BASE_KEY
			},
			{
				LockedDescriptionPresets.WIN_WITH_RELIC,
				WIN_WITH_RELIC_KEY
			},
			{
				LockedDescriptionPresets.WIN_WITH_ORB,
				WIN_WITH_ORB_KEY
			},
			{
				LockedDescriptionPresets.FIND_IN_SCENARIO,
				FIND_IN_SCENARIO_KEY
			},
			{
				LockedDescriptionPresets.PLAY_SEED,
				PLAY_SEED_KEY
			},
			{
				LockedDescriptionPresets.HIGHER_CRUCIBALL,
				HIGHER_CRUCIBALL
			},
			{
				LockedDescriptionPresets.THREE_QUESTION_MARKS,
				THREE_QUESTION_MARKS
			}
		};

		private const float KEYWORD_WIDTH = 180f;

		private const float KEYWORD_HEIGHT = 75f;

		[Header("Parameters")]
		public StatusEffectData EffectData;

		public Color defaultColor;

		public Color betterColor;

		public Color worseColor;

		[Header("Scene Refs")]
		public RelicManager _relicManager;

		public TextMeshProUGUI nameText;

		public TextMeshProUGUI damageText;

		public TextMeshProUGUI critText;

		public TextMeshProUGUI descriptionText;

		public GameObject damageInfo;

		public TooltipKeywordDescriptions keywordDesc;

		public CruciballNumberDisplayManager cruciballIndicator;

		public ChallengeSet globalChallenges;

		private int _keywordDescCount;

		private RectTransform _rectTransform;

		private Attack _attack;

		public float width
		{
			get
			{
				if (keywordDescActive)
				{
					if (_keywordDescCount <= 2)
					{
						return _rectTransform.rect.width + 180f;
					}
					return _rectTransform.rect.width + 360f;
				}
				return _rectTransform.rect.width;
			}
		}

		public float height
		{
			get
			{
				if (!(_rectTransform.rect.height > (float)_keywordDescCount * 75f))
				{
					return (float)_keywordDescCount * 75f;
				}
				return _rectTransform.rect.height;
			}
		}

		public Vector3 position
		{
			get
			{
				return _rectTransform.anchoredPosition;
			}
			set
			{
				_rectTransform.anchoredPosition = new Vector3(value.x, value.y);
			}
		}

		private bool keywordDescActive => _keywordDescCount > 0;

		private void Awake()
		{
			_rectTransform = GetComponent<RectTransform>();
		}

		public void Initialize(StatusEffectType type, bool isOnPlayer)
		{
			string translation = LocalizationManager.GetTranslation("Statuses/" + EffectData.GetStatusEffectDesc(type, isOnPlayer));
			descriptionText.text = translation;
			nameText.gameObject.SetActive(value: false);
			damageInfo.gameObject.SetActive(value: false);
			_keywordDescCount = 0;
			keywordDesc.UpdateString("");
		}

		public void Initialize(Attack attack)
		{
			nameText.gameObject.SetActive(value: true);
			damageInfo.gameObject.SetActive(value: true);
			_attack = attack;
			nameText.text = attack.GetNameWithLevel();
			UpdateDamage(_attack, damageText, critText);
			descriptionText.text = attack.Description;
			TooltipKeywordDescriptions componentInChildren = GetComponentInChildren<TooltipKeywordDescriptions>();
			if (componentInChildren != null && !string.IsNullOrEmpty(attack.EnglishDescription))
			{
				_keywordDescCount = componentInChildren.UpdateString(attack.EnglishDescription);
			}
			cruciballIndicator.gameObject.SetActive(value: false);
			string key = (_attack.locNameString.StartsWith(ASSEMBALL_KEY) ? ASSEMBALL_KEY : _attack.locNameString);
			if (PersistentPlayerData.Instance.PermanentStats.historicOrbPlayData.TryGetValue(key, out var value) && value.highestCruciballBeat > 0)
			{
				cruciballIndicator.gameObject.SetActive(SettingsManager.Instance.ShowCruciballCompletionOnTooltips);
				cruciballIndicator.UpdateDisplay(value.highestCruciballBeat);
			}
		}

		public void InitializeLocked(Attack attack)
		{
			nameText.gameObject.SetActive(value: true);
			damageInfo.gameObject.SetActive(value: true);
			nameText.text = "???";
			damageText.text = "?";
			critText.text = "?";
			damageText.color = DEFAULT_DAMAGE_COLOR;
			critText.color = DEFAULT_DAMAGE_COLOR;
			string translation = LocalizationManager.GetTranslation(attack.LockedName);
			if (string.IsNullOrEmpty(translation))
			{
				translation = LocalizationManager.GetTranslation(LockedDescPresetToKeyMap[attack.lockedDescriptionPreset]);
			}
			descriptionText.text = translation;
			GetComponentInChildren<TooltipKeywordDescriptions>().transform.MMDestroyAllChildren();
			_keywordDescCount = 0;
			cruciballIndicator.gameObject.SetActive(value: false);
		}

		public void Initialize(Relic relic)
		{
			nameText.gameObject.SetActive(value: true);
			damageInfo.gameObject.SetActive(value: false);
			nameText.text = LocalizationManager.GetTranslation(relic.nameKey);
			descriptionText.GetComponent<Localize>().Term = relic.descKey;
			TooltipKeywordDescriptions componentInChildren = GetComponentInChildren<TooltipKeywordDescriptions>();
			if (componentInChildren != null)
			{
				string translation = LocalizationManager.GetTranslation(relic.descKey, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: true, null, "English");
				_keywordDescCount = componentInChildren.UpdateString(translation);
			}
			cruciballIndicator.gameObject.SetActive(value: false);
			if (PersistentPlayerData.Instance.PermanentStats.historicRelicData.TryGetValue(relic.effect, out var value) && value.highestCruciballBeat > 0)
			{
				cruciballIndicator.gameObject.SetActive(SettingsManager.Instance.ShowCruciballCompletionOnTooltips);
				cruciballIndicator.UpdateDisplay(value.highestCruciballBeat);
			}
		}

		public void InitializeLocked(Relic relic)
		{
			nameText.gameObject.SetActive(value: true);
			damageInfo.gameObject.SetActive(value: false);
			nameText.text = "???";
			string translation = LocalizationManager.GetTranslation(relic.lockedInfoKey);
			if (string.IsNullOrEmpty(translation))
			{
				string term = LockedDescPresetToKeyMap[relic.lockedDescriptionPreset];
				if (relic.lockedDescriptionPreset == LockedDescriptionPresets.RELIC_DEFAULT && _relicManager.ShopRelicPool.Contains(relic))
				{
					term = SHOP_ONLY_KEY;
				}
				translation = LocalizationManager.GetTranslation(term);
			}
			descriptionText.text = translation;
			TooltipKeywordDescriptions componentInChildren = GetComponentInChildren<TooltipKeywordDescriptions>();
			if (componentInChildren != null)
			{
				componentInChildren.UpdateString("none");
			}
			cruciballIndicator.gameObject.SetActive(value: false);
			_keywordDescCount = 0;
		}

		public void Initialize(ChallengeEffect effect)
		{
			nameText.gameObject.SetActive(value: true);
			damageInfo.gameObject.SetActive(value: false);
			ChallengeData dataForChallengeFromGlobal = ChallengeManager.GetDataForChallengeFromGlobal(effect, globalChallenges);
			nameText.text = LocalizationManager.GetTranslation(dataForChallengeFromGlobal.nameKey);
			descriptionText.GetComponent<Localize>().Term = dataForChallengeFromGlobal.descKey;
			cruciballIndicator.gameObject.SetActive(value: false);
			TooltipKeywordDescriptions componentInChildren = GetComponentInChildren<TooltipKeywordDescriptions>();
			if (componentInChildren != null)
			{
				string translation = LocalizationManager.GetTranslation(dataForChallengeFromGlobal.descKey, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: true, null, "English");
				_keywordDescCount = componentInChildren.UpdateString(translation);
			}
		}

		public void InitializeLocked(ChallengeEffect challenge)
		{
			nameText.gameObject.SetActive(value: true);
			damageInfo.gameObject.SetActive(value: false);
			nameText.text = "???";
			descriptionText.text = "???";
			cruciballIndicator.gameObject.SetActive(value: false);
			_keywordDescCount = 0;
		}

		public static void UpdateDamage(Attack _attack, TextMeshProUGUI damageText, TextMeshProUGUI critText)
		{
			if (_attack == null)
			{
				return;
			}
			if ((bool)_attack.GetComponent<SummoningCirclePachinkoBall>() || (bool)_attack.GetComponent<CopyOrbOnReload>())
			{
				damageText.text = "?";
				damageText.color = DEFAULT_DAMAGE_COLOR;
				critText.text = "?";
				critText.color = DEFAULT_DAMAGE_COLOR;
				return;
			}
			float num = _attack.DamagePerPeg;
			float num2 = _attack.GetModifiedDamagePerPeg();
			float num3 = _attack.CritDamagePerPeg;
			float num4 = _attack.GetModifiedDamagePerPeg(1);
			if (DeckManager.completeDeck != null && !DeckManager.completeDeck.Find((GameObject x) => x.GetComponent<Attack>().instanceID == _attack.instanceID) && _attack.TryGetComponent<BuffOtherOrbsInDeck>(out var component) && component.orbLocStringNamesToBuff.Contains(_attack.locNameString))
			{
				num2 += (float)component.regularDamage;
				num4 += (float)component.critDamage;
			}
			damageText.text = num2.ToString(CultureInfo.InvariantCulture);
			if (num2 > num)
			{
				damageText.color = BETTER_DAMAGE_COLOR;
			}
			else if (num2 < num)
			{
				damageText.color = WORSE_DAMAGE_COLOR;
			}
			else
			{
				damageText.color = DEFAULT_DAMAGE_COLOR;
			}
			critText.text = num4.ToString();
			if (num4 > num3)
			{
				critText.color = BETTER_DAMAGE_COLOR;
			}
			else if (num4 < num3)
			{
				critText.color = WORSE_DAMAGE_COLOR;
			}
			else
			{
				critText.color = DEFAULT_DAMAGE_COLOR;
			}
		}
	}
}
