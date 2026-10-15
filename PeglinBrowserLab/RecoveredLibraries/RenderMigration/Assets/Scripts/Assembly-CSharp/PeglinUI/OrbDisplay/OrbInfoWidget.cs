using System;
using System.Globalization;
using Battle;
using Battle.Attacks;
using Battle.Pachinko.BallBehaviours;
using Battle.StatusEffects;
using I2.Loc;
using MoreMountains.Tools;
using PeglinUI.ControllerSupport;
using PeglinUI.PostBattle;
using Rewired.Integration.UnityUI;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace PeglinUI.OrbDisplay
{
	public class OrbInfoWidget : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI _nameText;

		[SerializeField]
		private TextMeshProUGUI _damageText;

		[SerializeField]
		private TextMeshProUGUI _critText;

		[SerializeField]
		private TextMeshProUGUI _descriptionText;

		[SerializeField]
		private Color _defaultColor;

		[SerializeField]
		private Color _betterColor;

		[SerializeField]
		private Color _worseColor;

		[SerializeField]
		private UpgradeConfirmationOrbInspect _upgradeConfirmationPanelInspect;

		private Attack _attack;

		private RewiredEventSystem _rewiredEventSystem;

		private void OnEnable()
		{
			BattleController.OnBombDetonated = (Action)Delegate.Combine(BattleController.OnBombDetonated, new Action(UpdateDamage));
			BattleController.OnPlayerSelfDamageTaken = (PlayerHealthController.PlayerHealthEvent)Delegate.Combine(BattleController.OnPlayerSelfDamageTaken, new PlayerHealthController.PlayerHealthEvent(HandleSelfDamageTaken));
			PlayerStatusEffectController.BroadcastStatusEffectsModified = (Action)Delegate.Combine(PlayerStatusEffectController.BroadcastStatusEffectsModified, new Action(HandleUpdatedStatusEffects));
		}

		private void OnDisable()
		{
			BattleController.OnBombDetonated = (Action)Delegate.Remove(BattleController.OnBombDetonated, new Action(UpdateDamage));
			BattleController.OnPlayerSelfDamageTaken = (PlayerHealthController.PlayerHealthEvent)Delegate.Remove(BattleController.OnPlayerSelfDamageTaken, new PlayerHealthController.PlayerHealthEvent(HandleSelfDamageTaken));
			PlayerStatusEffectController.BroadcastStatusEffectsModified = (Action)Delegate.Remove(PlayerStatusEffectController.BroadcastStatusEffectsModified, new Action(HandleUpdatedStatusEffects));
		}

		public void Initialize(Attack attack)
		{
			_attack = attack;
			_nameText.text = attack.GetNameWithLevel();
			UpdateDamage();
			_descriptionText.text = attack.Description;
			TooltipKeywordDescriptions componentInChildren = GetComponentInChildren<TooltipKeywordDescriptions>();
			if (componentInChildren != null && !string.IsNullOrEmpty(attack.EnglishDescription))
			{
				componentInChildren.UpdateString(attack.EnglishDescription);
			}
		}

		public void InitializeLocked(Attack attack)
		{
			_nameText.text = "???";
			_damageText.text = "?";
			_damageText.color = _defaultColor;
			_critText.text = "?";
			_critText.color = _defaultColor;
			string translation = LocalizationManager.GetTranslation(attack.LockedName);
			if (string.IsNullOrEmpty(translation))
			{
				translation = LocalizationManager.GetTranslation(Tooltip.LOCKED_ORB_DESC_BASE_KEY);
			}
			_descriptionText.text = translation;
			GetComponentInChildren<TooltipKeywordDescriptions>().transform.MMDestroyAllChildren();
		}

		private void HandleSelfDamageTaken(float damage)
		{
			UpdateDamage();
		}

		private void HandleUpdatedStatusEffects()
		{
			UpdateDamage();
		}

		public void UpdateDamage()
		{
			if (_attack == null)
			{
				return;
			}
			if ((bool)_attack.GetComponent<SummoningCirclePachinkoBall>() || (bool)_attack.GetComponent<CopyOrbOnReload>())
			{
				_damageText.text = "?";
				_damageText.color = _defaultColor;
				_critText.text = "?";
				_critText.color = _defaultColor;
				return;
			}
			float num = _attack.DamagePerPeg;
			float num2 = _attack.GetModifiedDamagePerPeg();
			float num3 = _attack.CritDamagePerPeg;
			float num4 = _attack.GetModifiedDamagePerPeg(1);
			_damageText.text = num2.ToString(CultureInfo.InvariantCulture);
			if (num2 > num)
			{
				_damageText.color = _betterColor;
			}
			else if (num2 < num)
			{
				_damageText.color = _worseColor;
			}
			else
			{
				_damageText.color = _defaultColor;
			}
			_critText.text = num4.ToString();
			if (num4 > num3)
			{
				_critText.color = _betterColor;
			}
			else if (num4 < num3)
			{
				_critText.color = _worseColor;
			}
			else
			{
				_critText.color = _defaultColor;
			}
		}

		public void MoveToPosition(Vector3 targetPos, Vector2 offset)
		{
			RectTransform component = GetComponent<RectTransform>();
			Rect rect = component.rect;
			Vector2 pivot = component.pivot;
			Vector2 normalized = offset.normalized;
			Vector3 vector = new Vector3(normalized.x * rect.width * pivot.x, normalized.y * rect.height * pivot.y);
			base.transform.position = targetPos + (Vector3)offset + vector;
			_ = component.anchoredPosition;
			new Vector2(Screen.width, Screen.height);
			Camera main = Camera.main;
			if (main != null)
			{
				Vector2 vector2 = main.transform.position;
				main.GetComponent<PixelPerfectCamera>();
				float orthographicSize = main.orthographicSize;
				float num = orthographicSize * main.aspect;
				Vector2 vector3 = vector2 + new Vector2(0f - num, orthographicSize);
				Vector2 vector4 = vector2 + new Vector2(num, 0f - orthographicSize);
				bool flag = false;
				Vector3 position = base.transform.position;
				if (position.x + rect.width * (1f - pivot.x) > vector4.x || position.x - rect.width * pivot.x < vector3.x)
				{
					flag = true;
					offset.x *= -1f;
				}
				if (position.y + rect.height * pivot.y > vector3.y || position.y - rect.height * (1f - pivot.y) < vector4.y)
				{
					flag = true;
					offset.y *= -1f;
				}
				if (flag)
				{
					normalized = offset.normalized;
					vector = new Vector3(normalized.x * rect.width * pivot.x, normalized.y * rect.height * pivot.y);
					base.transform.position = targetPos + (Vector3)offset + vector;
				}
			}
		}

		public void MoveToPositionScreenSpace(RectTransform iconRect, Vector2 anchorPoint)
		{
			Rect rect = GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect;
			RectTransform component = GetComponent<RectTransform>();
			Rect rect2 = component.rect;
			Vector3 position = iconRect.transform.position;
			Rect rect3 = iconRect.rect;
			component.position = position;
			Vector2 anchoredPosition = component.anchoredPosition;
			Vector2 zero = Vector2.zero;
			Vector2 pivot = iconRect.pivot;
			if (anchorPoint.x < 0.5f)
			{
				zero.x = 0f - (rect3.width * pivot.x + rect2.width * component.pivot.x);
			}
			else if (anchorPoint.x > 0.5f)
			{
				zero.x = rect3.width * pivot.x + rect2.width * component.pivot.x;
			}
			if (anchorPoint.y < 0.5f)
			{
				zero.y = (0f - rect3.height) * pivot.y;
			}
			else if (anchorPoint.y > 0.5f)
			{
				zero.y = rect3.height * pivot.y + rect2.height * component.pivot.y;
			}
			component.anchoredPosition += zero;
			float num = rect.min.x + rect2.width * component.pivot.x;
			float num2 = rect.max.x - component.sizeDelta.x * 2f * (1f - component.pivot.x);
			float num3 = 0f - rect.height + rect2.height * component.pivot.y;
			float num4 = 0f;
			Vector2 anchoredPosition2 = component.anchoredPosition;
			if (anchoredPosition2.x < num)
			{
				zero.x = rect3.width * pivot.x + rect2.width * component.pivot.x;
			}
			else if (anchoredPosition2.x > num2)
			{
				zero.x = 0f - (rect3.width * pivot.x + rect2.width * component.pivot.x);
			}
			if (anchoredPosition2.y < num3)
			{
				zero.y = rect3.height * pivot.y + rect2.height * component.pivot.y;
			}
			else if (anchoredPosition2.y > num4)
			{
				zero.y = (0f - rect3.height) * pivot.y;
			}
			component.anchoredPosition = anchoredPosition + zero;
			anchoredPosition = component.anchoredPosition;
			component.anchoredPosition = new Vector2(Mathf.Clamp(anchoredPosition.x, num, num2), Mathf.Clamp(anchoredPosition.y, num3, num4));
		}

		public void ShowLargeOrbPreview()
		{
			if (!(_attack == null))
			{
				BattleController battleController = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<BattleController>();
				if (!(battleController == null) && !(battleController.PlayerHealthController.CurrentHealth <= 0f))
				{
					GameObject setActiveOnReturn = ((_rewiredEventSystem != null) ? _rewiredEventSystem.currentSelectedGameObject : base.gameObject);
					_upgradeConfirmationPanelInspect.gameObject.SetActive(value: true);
					_upgradeConfirmationPanelInspect.PopulateForInspectOrb(_attack.gameObject);
					_upgradeConfirmationPanelInspect.GetComponent<SubMenuControllerConnection>().setActiveOnReturn = setActiveOnReturn;
				}
			}
		}
	}
}
