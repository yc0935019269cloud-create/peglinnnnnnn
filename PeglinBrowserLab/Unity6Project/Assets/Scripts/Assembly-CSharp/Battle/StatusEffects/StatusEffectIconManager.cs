using System;
using System.Collections.Generic;
using Battle.Enemies;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.StatusEffects
{
	public class StatusEffectIconManager : MonoBehaviour
	{
		public delegate void EnemyStatusEffectsUpdated();

		public delegate void StatusEffectEvent(StatusEffectIcon icon);

		public static EnemyStatusEffectsUpdated OnEnemyStatusEffectsUpdated;

		public StatusEffectEvent OnStatusEffectCreated;

		public Transform horizontalContainer;

		public FloatingText floatingText;

		[SerializeField]
		private GameObject _statusEffectIconPrefab;

		public bool isOnPlayer;

		private Canvas _canvas;

		private Dictionary<StatusEffectType, StatusEffectIcon> _iconDictionary = new Dictionary<StatusEffectType, StatusEffectIcon>();

		private bool debugDeleted;

		public StatusEffectData EffectData { get; set; }

		private void Awake()
		{
			if (!debugDeleted)
			{
				horizontalContainer.MMDestroyAllChildren();
				debugDeleted = true;
			}
			_canvas = GetComponent<Canvas>();
		}

		private void Update()
		{
			if (isOnPlayer || _canvas == null)
			{
				return;
			}
			bool flag = false;
			foreach (StatusEffectIcon value in _iconDictionary.Values)
			{
				if (value != null && EventSystem.current.currentSelectedGameObject == value.gameObject)
				{
					flag = true;
				}
			}
			if (flag)
			{
				_canvas.sortingOrder = 4;
			}
			else
			{
				_canvas.sortingOrder = 3;
			}
		}

		public void UpdateStatusEffects(StatusEffect[] statusEffects)
		{
			if (!debugDeleted)
			{
				horizontalContainer.MMDestroyAllChildren();
				debugDeleted = true;
			}
			Enemy component = GetComponent<Enemy>();
			if (EffectData == null && component != null)
			{
				Debug.LogWarning("Attempting to assign status effect to Enemy with no access to EffectData. Enemy: " + GetComponentInParent<Enemy>().gameObject.name);
				return;
			}
			foreach (StatusEffect effect in statusEffects)
			{
				UpdateStatusEffect(effect);
			}
			OnEnemyStatusEffectsUpdated?.Invoke();
		}

		public void UpdateStatusEffect(StatusEffect effect)
		{
			if (effect.Intensity != 0 && _iconDictionary.ContainsKey(effect.EffectType) && _iconDictionary[effect.EffectType] != null && _iconDictionary[effect.EffectType].IsDestructionAnimationPlaying)
			{
				_iconDictionary[effect.EffectType].iconForcefullyRemovedDuringDestructionAnimation = true;
				UnityEngine.Object.Destroy(_iconDictionary[effect.EffectType].gameObject);
				_iconDictionary.Remove(effect.EffectType);
			}
			if (_iconDictionary.ContainsKey(effect.EffectType))
			{
				StatusEffectIcon statusEffectIcon = _iconDictionary[effect.EffectType];
				if (effect.Intensity - statusEffectIcon.intensity > 0)
				{
					statusEffectIcon.Pulse();
				}
				if (effect.Intensity <= 0)
				{
					statusEffectIcon.RemoveIcon(_iconDictionary);
					RemoveStatusEffectCallbacks(statusEffectIcon);
				}
				else
				{
					statusEffectIcon.SetData(effect.Intensity);
				}
			}
			else
			{
				if (effect.Intensity == 0)
				{
					return;
				}
				StatusEffectIcon component = UnityEngine.Object.Instantiate(_statusEffectIconPrefab, horizontalContainer).GetComponent<StatusEffectIcon>();
				component.SetData(EffectData.GetStatusEffectIcon(effect.EffectType), effect.Intensity, effect.EffectType, isOnPlayer);
				Canvas.ForceUpdateCanvases();
				_iconDictionary.Add(effect.EffectType, component);
				if (effect.EffectType == StatusEffectType.Ballwark)
				{
					PlayerHealthController.OnArmourHit = (PlayerHealthController.PlayerHealthEvent)Delegate.Combine(PlayerHealthController.OnArmourHit, new PlayerHealthController.PlayerHealthEvent(component.GetComponent<FlashStatusEffectIcon>().FlashWhite));
				}
				else if (effect.EffectType == StatusEffectType.Ballusion)
				{
					PlayerHealthController.OnDodge = (PlayerHealthController.PlayerHealthEvent)Delegate.Combine(PlayerHealthController.OnDodge, new PlayerHealthController.PlayerHealthEvent(component.GetComponent<FlashStatusEffectIcon>().FlashWhite));
				}
				else if (effect.EffectType == StatusEffectType.Blind)
				{
					Enemy componentInParent = GetComponentInParent<Enemy>();
					if (componentInParent != null)
					{
						componentInParent.OnEnemyMissed = (Enemy.EnemyMissEvent)Delegate.Combine(componentInParent.OnEnemyMissed, new Enemy.EnemyMissEvent(component.GetComponent<FlashStatusEffectIcon>().FlashWhite));
					}
				}
				component.Pulse();
				OnStatusEffectCreated?.Invoke(component);
			}
		}

		private void ShowFloatingEffectText(StatusEffectType effect, int intensity)
		{
			string text = $"+{intensity} <sprite name=\"{effect.ToString().ToUpper()}\">";
			floatingText.Activate(text);
		}

		public StatusEffectIcon GetIconForEffect(StatusEffectType effect)
		{
			return _iconDictionary[effect];
		}

		public void AddComponentToUI(Type type, StatusEffectType effectType)
		{
			_iconDictionary[effectType].gameObject.AddComponent(type);
		}

		private void RemoveStatusEffectCallbacks(StatusEffectIcon icon)
		{
			if (icon.effectType == StatusEffectType.Ballwark)
			{
				PlayerHealthController.OnArmourHit = (PlayerHealthController.PlayerHealthEvent)Delegate.Remove(PlayerHealthController.OnArmourHit, new PlayerHealthController.PlayerHealthEvent(icon.gameObject.GetComponent<FlashStatusEffectIcon>().FlashWhite));
			}
			else if (icon.effectType == StatusEffectType.Ballusion)
			{
				PlayerHealthController.OnDodge = (PlayerHealthController.PlayerHealthEvent)Delegate.Remove(PlayerHealthController.OnDodge, new PlayerHealthController.PlayerHealthEvent(icon.gameObject.GetComponent<FlashStatusEffectIcon>().FlashWhite));
			}
			else if (icon.effectType == StatusEffectType.Blind)
			{
				Enemy componentInParent = GetComponentInParent<Enemy>();
				if (componentInParent != null)
				{
					componentInParent.OnEnemyMissed = (Enemy.EnemyMissEvent)Delegate.Remove(componentInParent.OnEnemyMissed, new Enemy.EnemyMissEvent(icon.gameObject.GetComponent<FlashStatusEffectIcon>().FlashWhite));
				}
			}
		}
	}
}
