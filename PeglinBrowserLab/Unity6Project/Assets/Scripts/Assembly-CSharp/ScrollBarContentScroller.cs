using PeglinUI.ControllerSupport;
using PeglinUtils;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.UI;

public class ScrollBarContentScroller : MonoBehaviour
{
	public RectTransform Viewport;

	public RectTransform ContentPanel;

	public RectTransform ScrollRectTransform;

	private Scrollbar _scrollbar;

	private RewiredEventSystem _eventSystem;

	private GameObject prevSelected;

	private void OnEnable()
	{
		if (_scrollbar == null)
		{
			_scrollbar = GetComponentInChildren<Scrollbar>();
		}
		if (_eventSystem == null)
		{
			_eventSystem = GameObject.FindWithTag("InputSystem").GetComponent<RewiredEventSystem>();
		}
		if (_scrollbar != null)
		{
			_scrollbar.value = 1f;
		}
	}

	private void Update()
	{
		if (ControllerInputManager.IsPointerBased)
		{
			return;
		}
		GameObject currentSelectedGameObject = _eventSystem.currentSelectedGameObject;
		if (currentSelectedGameObject != null && currentSelectedGameObject != prevSelected && ObjectUtils.IsTransformAChildOfTransformRecursive(currentSelectedGameObject.transform, base.transform))
		{
			RectTransform component = currentSelectedGameObject.GetComponent<RectTransform>();
			float num = Mathf.Abs(component.anchoredPosition.y) + component.rect.height;
			float y = ContentPanel.anchoredPosition.y;
			float num2 = ContentPanel.anchoredPosition.y + ScrollRectTransform.rect.height;
			if (num > num2)
			{
				float y2 = num - ScrollRectTransform.rect.height;
				ContentPanel.anchoredPosition = new Vector2(ContentPanel.anchoredPosition.x, y2);
			}
			else if (Mathf.Abs(component.anchoredPosition.y) < y)
			{
				ContentPanel.anchoredPosition = new Vector2(ContentPanel.anchoredPosition.x, Mathf.Abs(component.anchoredPosition.y));
			}
		}
		prevSelected = currentSelectedGameObject;
	}
}
