using System;
using System.Collections;
using PeglinUtils;
using UI.OrbDisplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle
{
	public class PlayfieldMouseDetector : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IDragHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public delegate void MouseClicked();

		public delegate void MouseDragged();

		public delegate void MouseUp();

		public static bool IsMouseDown = false;

		public static MouseClicked OnMouseClicked = () =>
		{
		};

		public static MouseDragged OnMouseDragged = () =>
		{
		};

		public static MouseUp OnMouseUp = () =>
		{
		};

		public Action OnPlayfieldMouseDetectorToggled = () =>
		{
		};

		public static Action OnPlayfieldMouseHoverEnter = () =>
		{
		};

		public static Action OnPlayfieldMouseHoverExit = () =>
		{
		};

		private bool _isInFocus = true;

		private bool _discardBlock;

		private float _discardBlockTimer;

		private void Update()
		{
			if (_discardBlock && _discardBlockTimer < 1.5f)
			{
				_discardBlockTimer += Time.deltaTime;
			}
			else if (_discardBlock && _discardBlockTimer >= 1.5f)
			{
				_discardBlock = false;
				_discardBlockTimer = 0f;
			}
		}

		private void OnEnable()
		{
			_isInFocus = Application.isFocused;
			OnPlayfieldMouseDetectorToggled?.Invoke();
			PlayerHealthController.OnHealthDepleted = (PlayerHealthController.HealthEvent)Delegate.Combine(PlayerHealthController.OnHealthDepleted, new PlayerHealthController.HealthEvent(DisableSelf));
			BattleController.OnOrbDiscarded = (Action)Delegate.Combine(BattleController.OnOrbDiscarded, new Action(StartDiscardBlock));
			OrbDiscardButton.OnDiscardFinished = (OrbDiscardButton.OrbDiscardEvent)Delegate.Combine(OrbDiscardButton.OnDiscardFinished, new OrbDiscardButton.OrbDiscardEvent(EndDiscardBlock));
		}

		private void OnDisable()
		{
			OnPlayfieldMouseDetectorToggled?.Invoke();
			PlayerHealthController.OnHealthDepleted = (PlayerHealthController.HealthEvent)Delegate.Remove(PlayerHealthController.OnHealthDepleted, new PlayerHealthController.HealthEvent(DisableSelf));
			OrbDiscardButton.OnOrbDiscardButtonClicked = (OrbDiscardButton.OrbDiscardEvent)Delegate.Remove(OrbDiscardButton.OnOrbDiscardButtonClicked, new OrbDiscardButton.OrbDiscardEvent(StartDiscardBlock));
			OrbDiscardButton.OnDiscardFinished = (OrbDiscardButton.OrbDiscardEvent)Delegate.Remove(OrbDiscardButton.OnDiscardFinished, new OrbDiscardButton.OrbDiscardEvent(EndDiscardBlock));
		}

		private void DisableSelf()
		{
			base.gameObject.SetActive(value: false);
		}

		private void StartDiscardBlock()
		{
			_discardBlock = true;
		}

		private void EndDiscardBlock()
		{
			_discardBlock = false;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left && _isInFocus && !_discardBlock)
			{
				OnMouseClicked();
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left && _isInFocus && !_discardBlock)
			{
				OnMouseDragged();
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left && _isInFocus && !_discardBlock)
			{
				OnMouseUp();
			}
		}

		private IEnumerator OnApplicationFocus(bool focus)
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			_isInFocus = focus;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			OnPlayfieldMouseHoverEnter();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			OnPlayfieldMouseHoverExit();
		}
	}
}
