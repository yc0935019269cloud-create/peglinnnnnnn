using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.UIUtils
{
	public class ScrollbarSlider : MonoBehaviour
	{
		public ScrollRect scrollRect;

		public Slider slider;

		public RectTransform content;

		public RectTransform scrollRectContainer;

		private void OnEnable()
		{
			slider.onValueChanged.AddListener(UpdateScrollPosition);
			scrollRect.onValueChanged.AddListener(UpdateSliderValue);
		}

		private void OnDisable()
		{
			slider.onValueChanged.RemoveListener(UpdateScrollPosition);
			scrollRect.onValueChanged.RemoveListener(UpdateSliderValue);
		}

		private void UpdateScrollPosition(float value)
		{
			scrollRect.verticalNormalizedPosition = 1f - value;
		}

		private void UpdateSliderValue(Vector2 scrollPos)
		{
			if (content.rect.height < scrollRectContainer.rect.height)
			{
				slider.gameObject.SetActive(value: false);
			}
			else
			{
				slider.gameObject.SetActive(value: true);
			}
			slider.SetValueWithoutNotify(1f - scrollPos.y);
		}
	}
}
