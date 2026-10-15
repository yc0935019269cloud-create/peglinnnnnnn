using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Battle
{
	public class SlotManager : MonoBehaviour
	{
		public SpriteRenderer icon;

		public SpriteRenderer iconOne;

		public SpriteRenderer highlightHalf;

		public SpriteRenderer iconTwo;

		public SpriteRenderer highlightFull;

		public GameObject damageSpriteContainerHalf;

		public GameObject damageSpriteContainerFull;

		public void ConfigureForBattle()
		{
			ToggleMultiplierVisibility(show: true);
			if (damageSpriteContainerHalf != null)
			{
				damageSpriteContainerHalf.SetActive(value: false);
			}
			if (damageSpriteContainerFull != null)
			{
				damageSpriteContainerFull.SetActive(value: false);
			}
			icon.gameObject.SetActive(value: false);
			iconOne.gameObject.SetActive(value: false);
			if (iconTwo != null)
			{
				iconTwo.gameObject.SetActive(value: false);
			}
		}

		public void ConfigureForNavigation(Sprite newIcon, Color highlightColor, float delay = 0f, float duration = 0f)
		{
			ToggleMultiplierVisibility(show: false);
			if (damageSpriteContainerHalf != null)
			{
				damageSpriteContainerHalf.SetActive(value: false);
			}
			if (damageSpriteContainerFull != null)
			{
				damageSpriteContainerFull.SetActive(value: false);
			}
			icon.gameObject.SetActive(value: true);
			icon.sprite = newIcon;
			Color color = icon.color;
			color.a = 0f;
			Sequence sequence = DOTween.Sequence();
			sequence.Append(icon.DOColor(icon.color, duration).From(color));
			highlightHalf.gameObject.SetActive(value: true);
			color = highlightColor;
			color.a = 0f;
			sequence.Join(highlightHalf.DOColor(highlightColor, duration).From(color));
			iconOne.gameObject.SetActive(value: false);
			if (iconTwo != null)
			{
				iconTwo.gameObject.SetActive(value: false);
				highlightFull.gameObject.SetActive(value: true);
				sequence.Join(highlightFull.DOColor(highlightColor, duration).From(color));
			}
			sequence.PrependInterval(delay);
			sequence.Play();
		}

		public void ConfigureForIconOnly(Sprite newIcon, float fadeDuration = 0f)
		{
			ToggleNavigationIconVisibility(visible: true);
			ToggleColliders(enabled: false);
			if (highlightHalf != null)
			{
				highlightHalf.gameObject.SetActive(value: false);
			}
			if (highlightFull != null)
			{
				highlightFull.gameObject.SetActive(value: false);
			}
			icon.gameObject.SetActive(value: true);
			icon.sprite = newIcon;
			Color color = icon.color;
			color.a = 0f;
			icon.DOColor(icon.color, fadeDuration).From(color);
		}

		public void ConfigureDudNavigation()
		{
			ToggleMultiplierVisibility(show: false);
			if (damageSpriteContainerHalf != null)
			{
				damageSpriteContainerHalf.SetActive(value: true);
			}
			if (damageSpriteContainerFull != null)
			{
				damageSpriteContainerFull.SetActive(value: true);
			}
			icon.gameObject.SetActive(value: false);
			iconOne.gameObject.SetActive(value: false);
			if (iconTwo != null)
			{
				iconTwo.gameObject.SetActive(value: false);
			}
			highlightHalf.gameObject.SetActive(value: false);
			if (highlightFull != null)
			{
				highlightFull.gameObject.SetActive(value: false);
			}
		}

		public void ConfigureHalfNavigation(Color highlightColor, float delay = 0f, float duration = 0f)
		{
			ToggleMultiplierVisibility(show: false);
			if (damageSpriteContainerHalf != null)
			{
				damageSpriteContainerHalf.SetActive(value: false);
			}
			if (damageSpriteContainerFull != null)
			{
				damageSpriteContainerFull.SetActive(value: true);
			}
			icon.gameObject.SetActive(value: false);
			iconOne.gameObject.SetActive(value: false);
			if (iconTwo != null)
			{
				iconTwo.gameObject.SetActive(value: false);
			}
			highlightHalf.gameObject.SetActive(value: true);
			Color fromValue = highlightColor;
			fromValue.a = 0f;
			highlightHalf.DOColor(highlightColor, duration).From(fromValue);
			if (highlightFull != null)
			{
				highlightFull.gameObject.SetActive(value: false);
			}
		}

		private void ToggleMultiplierVisibility(bool show)
		{
			TMP_Text[] componentsInChildren = GetComponentsInChildren<TMP_Text>(includeInactive: true);
			if (componentsInChildren == null || componentsInChildren.Length == 0)
			{
				return;
			}
			TMP_Text[] array = componentsInChildren;
			foreach (TMP_Text tMP_Text in array)
			{
				if (show)
				{
					tMP_Text.gameObject.SetActive(value: true);
					tMP_Text.enabled = true;
					tMP_Text.DOFade(1f, 0.25f);
				}
				else
				{
					tMP_Text.DOFade(0f, 0.25f);
				}
			}
		}

		public void ToggleNavigationIconVisibility(bool visible)
		{
			icon.gameObject.SetActive(visible);
			if (iconTwo != null)
			{
				iconTwo.gameObject.SetActive(visible);
			}
		}

		public void ToggleColliders(bool enabled)
		{
			Collider2D[] componentsInChildren = base.gameObject.GetComponentsInChildren<Collider2D>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = enabled;
			}
		}
	}
}
