using DG.Tweening;
using PeglinUI.ControllerSupport;
using PeglinUtils;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ControllerSupport
{
	public class EnsureGridItemIsVisible : MonoBehaviour
	{
		public bool UseUnscaledTime;

		public bool useParent;

		private ScrollRect _scrollRect;

		private RewiredEventSystem _eventSystem;

		private GameObject prevSelected;

		private void Awake()
		{
			_scrollRect = GetComponentInParent<ScrollRect>();
			_eventSystem = GameObject.FindWithTag("InputSystem").GetComponent<RewiredEventSystem>();
		}

		private void OnEnable()
		{
			if (_scrollRect != null)
			{
				Vector2 normalizedPosition = _scrollRect.normalizedPosition;
				normalizedPosition.y = 1f;
				_scrollRect.normalizedPosition = normalizedPosition;
			}
		}

		private void Update()
		{
			GameObject currentSelectedGameObject = _eventSystem.currentSelectedGameObject;
			if (currentSelectedGameObject != null && currentSelectedGameObject != prevSelected && !ControllerInputManager.IsPointerBased && ObjectUtils.IsTransformAChildOfTransformRecursive(currentSelectedGameObject.transform, base.transform))
			{
				_scrollRect.DOKill();
				RectTransform rectTransform = ((!useParent) ? currentSelectedGameObject.GetComponent<RectTransform>() : currentSelectedGameObject.transform.parent.GetComponent<RectTransform>());
				float height = GetComponent<RectTransform>().rect.height;
				float num = Mathf.Abs(rectTransform.anchoredPosition.y);
				float height2 = rectTransform.rect.height;
				float endValue = 1f - num / height;
				if (num <= height2)
				{
					endValue = 1f;
				}
				else if (num >= height - height2)
				{
					endValue = 0f;
				}
				_scrollRect.DOVerticalNormalizedPos(endValue, 0.35f).SetUpdate(UseUnscaledTime);
			}
			prevSelected = currentSelectedGameObject;
		}
	}
}
