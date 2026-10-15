using System.Collections;
using PeglinUI.ControllerSupport;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PeglinUI
{
	[RequireComponent(typeof(Button))]
	public class ButtonOffsetItem : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public float XOffsetHover;

		public float YOffsetHover;

		public float XOffsetPress;

		public float YOffsetPress;

		private Button _button;

		private RectTransform _rectTransform;

		private UnityAction _pressAction;

		private bool _isHoverOffset;

		private bool _isPressOffset;

		private bool _canHover;

		private Vector3 _previousMousePosition;

		private void OnEnable()
		{
			if (_button == null)
			{
				_button = GetComponent<Button>();
			}
			_pressAction = () =>
			{
				if (!ControllerInputManager.IsPointerBased && base.gameObject.activeInHierarchy)
				{
					StartCoroutine(PressDepressController());
				}
			};
			_rectTransform = GetComponent<RectTransform>();
			_button.onClick.AddListener(_pressAction);
		}

		private void Update()
		{
			_canHover = Input.mousePosition != _previousMousePosition;
			_previousMousePosition = Input.mousePosition;
		}

		private void OnDisable()
		{
			_button.onClick.RemoveListener(_pressAction);
			if (_isHoverOffset)
			{
				Offset(0f - XOffsetHover, 0f - YOffsetHover);
				_isHoverOffset = false;
			}
			if (_isPressOffset)
			{
				Offset(0f - XOffsetPress, 0f - YOffsetPress);
				_isPressOffset = false;
			}
		}

		private void Offset(float x, float y)
		{
			Vector3 localPosition = _rectTransform.localPosition;
			_rectTransform.localPosition = new Vector3(localPosition.x + x, localPosition.y + y, localPosition.z);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (_button.interactable)
			{
				Offset(XOffsetPress, YOffsetPress);
				_isPressOffset = true;
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (_button.interactable)
			{
				Offset(0f - XOffsetPress, 0f - YOffsetPress);
				_isPressOffset = false;
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			DoHoverOffset();
		}

		public void OnDeselect(BaseEventData eventData)
		{
			DoUnhoverOffset();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (_canHover)
			{
				DoHoverOffset();
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			DoUnhoverOffset();
		}

		private void DoHoverOffset()
		{
			if (_button.interactable && !_isHoverOffset)
			{
				Offset(XOffsetHover, YOffsetHover);
				_isHoverOffset = true;
			}
		}

		private void DoUnhoverOffset()
		{
			if (_button.interactable && _isHoverOffset)
			{
				Offset(0f - XOffsetHover, 0f - YOffsetHover);
				_isHoverOffset = false;
			}
		}

		private IEnumerator PressDepressController()
		{
			Offset(XOffsetPress, YOffsetPress);
			yield return new WaitForSecondsRealtime(0.1f);
			Offset(0f - XOffsetPress, 0f - YOffsetPress);
		}
	}
}
