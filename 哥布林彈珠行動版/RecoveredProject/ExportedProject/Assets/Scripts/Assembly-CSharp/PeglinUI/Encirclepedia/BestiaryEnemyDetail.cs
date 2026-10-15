using System;
using System.Collections.Generic;
using System.Globalization;
using Battle.Enemies;
using Coffee.UIEffects;
using Cruciball;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Saving;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.Encirclepedia
{
	public class BestiaryEnemyDetail : MonoBehaviour
	{
		[Header("Scene References")]
		public Image enemyImage;

		public Image enemyLootImage;

		public Animator lootAnimator;

		public Animator killAnimator;

		public Animator cardAnimator;

		public TextMeshProUGUI enemyName;

		public TextMeshProUGUI lore;

		public TextMeshProUGUI desc;

		public TextMeshProUGUI dev;

		public TextMeshProUGUI melee;

		public TextMeshProUGUI ranged;

		public TextMeshProUGUI hp;

		public TextMeshProUGUI cruciballHp;

		public TextMeshProUGUI researchPointsCurrent;

		public TextMeshProUGUI researchPointsTotal;

		public TextMeshProUGUI killXp;

		public TextMeshProUGUI lootXp;

		public Slider researchSlider;

		public Animator devNotesAnimator;

		public Animator descriptionAnimator;

		public Image areaBackground;

		public GameObject cardBackground;

		public GameObject cardForeground;

		public sineWobble enemyWobble;

		public UIShiny enemyShine;

		public Image killTrophyMissing;

		public Image lootTrophyMissing;

		public Image cardTrophyMissing;

		public Button rightArrow;

		public Button leftArrow;

		[Header("External References")]
		public Sprite forestImage;

		public Sprite castleImage;

		public Sprite minesImage;

		public Sprite coreImage;

		private const float TWEEN_DELAY = 0.15f;

		private float delayBetweenAnimators;

		private Queue<Animator> _animatorQueue = new Queue<Animator>();

		private Animator _currentAnimator;

		private Queue<Animator> _setAnimatorsToEnd = new Queue<Animator>();

		public void Initialize(EnemyData data, EnemyData.EnemySaveData previous, BestiaryManager manager)
		{
			SetStaticData(data);
			SetupAnimations(data, previous);
			leftArrow.onClick.AddListener(manager.ViewPreviousEnemy);
			rightArrow.onClick.AddListener(manager.ViewNextEnemy);
		}

		private void SetupAnimations(EnemyData data, EnemyData.EnemySaveData previous)
		{
			bool flag = data.CardDrop > 0;
			if (previous == null)
			{
				Debug.Log("Previous data is null!");
				researchPointsCurrent.text = data.ResearchProgress.ToString();
				researchSlider.value = (float)data.ResearchProgress / (float)data.ResearchPointTotal;
				devNotesAnimator.gameObject.SetActive(data.ResearchProgress >= data.ResearchPointTotal);
				lootAnimator.gameObject.SetActive(data.LootDrop > 0);
				killAnimator.gameObject.SetActive(data.Killed > 0);
				cardAnimator.gameObject.SetActive(flag);
				cardBackground.gameObject.SetActive(flag);
				cardForeground.gameObject.SetActive(flag);
				enemyWobble.enabled = flag;
				enemyShine.enabled = flag;
				return;
			}
			researchPointsCurrent.text = previous.ResearchProgress.ToString();
			researchPointsCurrent.DOCounter(previous.ResearchProgress, data.ResearchProgress, 1f, addThousandsSeparator: false).SetDelay(0.15f);
			float value = (float)previous.ResearchProgress / (float)data.ResearchPointTotal;
			float endValue = (float)data.ResearchProgress / (float)data.ResearchPointTotal;
			researchSlider.value = value;
			TweenerCore<float, float, FloatOptions> tweenerCore = researchSlider.DOValue(endValue, 1f).SetDelay(0.15f);
			bool flag2 = data.ResearchProgress >= data.ResearchPointTotal;
			bool flag3 = (float)data.ResearchProgress >= (float)data.ResearchPointTotal / 2f;
			if (previous.ResearchProgress == data.ResearchProgress)
			{
				devNotesAnimator.gameObject.SetActive(flag2);
				if (flag2)
				{
					_setAnimatorsToEnd.Enqueue(devNotesAnimator);
				}
				descriptionAnimator.gameObject.SetActive(flag3);
				if (flag3)
				{
					_setAnimatorsToEnd.Enqueue(descriptionAnimator);
				}
			}
			else
			{
				if ((float)previous.ResearchProgress < (float)data.ResearchPointTotal / 2f && (float)data.ResearchProgress >= (float)data.ResearchPointTotal / 2f)
				{
					tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
					{
						_animatorQueue.Enqueue(descriptionAnimator);
					}));
				}
				else
				{
					descriptionAnimator.gameObject.SetActive((float)data.ResearchProgress >= (float)data.ResearchPointTotal / 2f);
				}
				if (previous.ResearchProgress < data.ResearchPointTotal && data.ResearchProgress >= data.ResearchPointTotal)
				{
					tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
					{
						_animatorQueue.Enqueue(devNotesAnimator);
					}));
				}
				else
				{
					devNotesAnimator.gameObject.SetActive(data.ResearchProgress >= data.ResearchPointTotal);
				}
			}
			if (previous.Killed == 0 && data.Killed > 0)
			{
				_animatorQueue.Enqueue(killAnimator);
			}
			else
			{
				killAnimator.gameObject.SetActive(data.Killed > 0);
				if (data.Killed > 0)
				{
					_setAnimatorsToEnd.Enqueue(killAnimator);
				}
			}
			if (previous.LootDrop == 0 && data.LootDrop > 0)
			{
				_animatorQueue.Enqueue(lootAnimator);
			}
			else
			{
				lootAnimator.gameObject.SetActive(data.LootDrop > 0);
				if (data.LootDrop > 0)
				{
					_setAnimatorsToEnd.Enqueue(lootAnimator);
				}
			}
			if ((previous.CardDrop == 0) & flag)
			{
				_animatorQueue.Enqueue(cardAnimator);
			}
			else
			{
				cardAnimator.gameObject.SetActive(flag);
				if (flag)
				{
					_setAnimatorsToEnd.Enqueue(cardAnimator);
				}
			}
			cardBackground.gameObject.SetActive(flag);
			cardForeground.gameObject.SetActive(flag);
			enemyWobble.enabled = flag;
			enemyShine.enabled = flag;
			previous.Update(data);
		}

		private void SetStaticData(EnemyData data)
		{
			enemyImage.sprite = data.encirclepediaSprite;
			enemyLootImage.sprite = data.lootDropSprite;
			enemyName.text = data.NameLocalized;
			lore.text = data.LoreLocalized;
			desc.text = data.DescriptionLocalized;
			dev.text = data.DevNotesLocalized;
			if (data.Type == EnemyData.EnemyType.MINIBOSS && PersistentPlayerData.Instance.IsCruciballLevelUnlockedForAnyClass(16))
			{
				float minibossDamageModifierForLocation = CruciballManager.GetMinibossDamageModifierForLocation(data.location);
				float num = Mathf.Ceil(data.MeleeAttackDamage * minibossDamageModifierForLocation);
				if (num > 0f)
				{
					melee.text = data.MeleeAttackDamage.ToString(CultureInfo.InvariantCulture) + "/<style=c17>" + num.ToString(CultureInfo.InvariantCulture) + "</style>";
				}
				else
				{
					melee.text = data.MeleeAttackDamage.ToString(CultureInfo.InvariantCulture);
				}
				float num2 = Mathf.Ceil(data.RangedAttackDamage * minibossDamageModifierForLocation);
				if (num2 > 0f)
				{
					ranged.text = data.RangedAttackDamage.ToString(CultureInfo.InvariantCulture) + "/<style=c17>" + num2.ToString(CultureInfo.InvariantCulture) + "</style>";
				}
				else
				{
					ranged.text = data.RangedAttackDamage.ToString(CultureInfo.InvariantCulture);
				}
			}
			else
			{
				melee.text = data.MeleeAttackDamage.ToString(CultureInfo.InvariantCulture);
				ranged.text = data.RangedAttackDamage.ToString(CultureInfo.InvariantCulture);
			}
			if (data.LocKey == "dragon_boss")
			{
				ranged.text = $"{data.RangedAttackDamage}x5";
			}
			hp.text = data.MaxHealth.ToString(CultureInfo.InvariantCulture);
			cruciballHp.text = data.MaxHealthCruciball.ToString(CultureInfo.InvariantCulture);
			if (data.LocKey == "mimic_miniboss")
			{
				melee.text = "???";
				ranged.text = "???";
				hp.text = "???";
				cruciballHp.text = "???";
			}
			else if (data.LocKey == "slime_painbow")
			{
				hp.text = "???";
				cruciballHp.text = "???";
			}
			researchPointsTotal.text = data.ResearchPointTotal.ToString();
			killXp.text = data.PointsOnKill.ToString(CultureInfo.InvariantCulture);
			lootXp.text = data.PointsOnLoot.ToString(CultureInfo.InvariantCulture);
			data.SeenInBestiary = true;
			data.SaveData();
			devNotesAnimator.gameObject.SetActive(value: false);
			descriptionAnimator.gameObject.SetActive(value: false);
			killAnimator.gameObject.SetActive(value: false);
			lootAnimator.gameObject.SetActive(value: false);
			cardAnimator.gameObject.SetActive(value: false);
			cardBackground.gameObject.SetActive(value: false);
			cardForeground.gameObject.SetActive(value: false);
			enemyWobble.enabled = false;
			enemyShine.enabled = false;
			killTrophyMissing.enabled = data.Killed <= 0;
			lootTrophyMissing.enabled = data.LootDrop <= 0;
			cardTrophyMissing.enabled = data.CardDrop <= 0;
			Image image = areaBackground;
			image.sprite = data.location switch
			{
				EnemyData.LocationTypes.FOREST => forestImage, 
				EnemyData.LocationTypes.CASTLE => castleImage, 
				EnemyData.LocationTypes.MINES => minesImage, 
				EnemyData.LocationTypes.CORE => coreImage, 
				_ => null, 
			};
		}

		public void ChangeActiveEnemy(EnemyData data, EnemyData.EnemySaveData previous)
		{
			ResetAnimations();
			SetStaticData(data);
			SetupAnimations(data, previous);
		}

		public void SetupArrows(bool left, bool right)
		{
			leftArrow.gameObject.SetActive(left);
			rightArrow.gameObject.SetActive(right);
		}

		private void ResetAnimations()
		{
			Animator result;
			while (_animatorQueue.TryDequeue(out result))
			{
				if (result.gameObject.activeInHierarchy)
				{
					result.Play("stamp_animation", 0, 1f);
				}
			}
			Animator result2;
			while (_setAnimatorsToEnd.TryDequeue(out result2))
			{
				if (result2.gameObject.activeInHierarchy)
				{
					result2.Play("stamp_animation", 0, 1f);
				}
			}
			DOTween.Kill(researchSlider);
			DOTween.Kill(researchPointsCurrent);
		}

		private void Update()
		{
			Animator result;
			while (_setAnimatorsToEnd.TryDequeue(out result))
			{
				result.Play("stamp_animation", 0, 1f);
			}
			if (_animatorQueue.Count > 0 && delayBetweenAnimators <= 0f)
			{
				_currentAnimator = _animatorQueue.Dequeue();
				if (_currentAnimator == devNotesAnimator || _currentAnimator == descriptionAnimator)
				{
					delayBetweenAnimators = 0.75f;
				}
				else
				{
					delayBetweenAnimators = 0.25f;
				}
				_currentAnimator.gameObject.SetActive(value: true);
				_currentAnimator.Play("stamp_animation");
			}
			else if (_animatorQueue.Count > 0)
			{
				delayBetweenAnimators -= Time.deltaTime;
			}
		}

		private void OnDisable()
		{
			DOTween.Kill(researchSlider);
			DOTween.Kill(researchPointsCurrent);
			leftArrow.onClick.RemoveAllListeners();
			rightArrow.onClick.RemoveAllListeners();
		}
	}
}
