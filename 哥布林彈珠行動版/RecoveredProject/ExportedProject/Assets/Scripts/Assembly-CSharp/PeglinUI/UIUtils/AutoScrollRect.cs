using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.UIUtils
{
	[RequireComponent(typeof(ScrollRect))]
	public class AutoScrollRect : MonoBehaviour
	{
		public ScrollRect m_templateScrollRect;

		private RectTransform _selectedRectTransform;

		private GameObject _lastSelected;

		private RectTransform _scrollRectTransform;

		private void Awake()
		{
			_scrollRectTransform = m_templateScrollRect.GetComponent<RectTransform>();
		}

		private void Update()
		{
			UpdateScrollToSelected(m_templateScrollRect.content, _scrollRectTransform);
		}

		private void UpdateScrollToSelected(RectTransform contentRectTransform, RectTransform scrollRectTransform)
		{
			GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			if (!(currentSelectedGameObject == null) && !(currentSelectedGameObject.transform.parent != contentRectTransform.transform) && !(currentSelectedGameObject == _lastSelected))
			{
				_selectedRectTransform = currentSelectedGameObject.GetComponent<RectTransform>();
				float num = Mathf.Abs(_selectedRectTransform.anchoredPosition.y) + _selectedRectTransform.rect.height;
				float y = contentRectTransform.anchoredPosition.y;
				float num2 = contentRectTransform.anchoredPosition.y + scrollRectTransform.rect.height;
				if (num > num2)
				{
					float y2 = num - scrollRectTransform.rect.height + _selectedRectTransform.rect.height;
					contentRectTransform.anchoredPosition = new Vector2(contentRectTransform.anchoredPosition.x, y2);
				}
				else if (Mathf.Abs(_selectedRectTransform.anchoredPosition.y) < y)
				{
					contentRectTransform.anchoredPosition = new Vector2(contentRectTransform.anchoredPosition.x, Mathf.Abs(_selectedRectTransform.anchoredPosition.y) - _selectedRectTransform.rect.height);
				}
				_lastSelected = currentSelectedGameObject;
			}
		}
	}
}
