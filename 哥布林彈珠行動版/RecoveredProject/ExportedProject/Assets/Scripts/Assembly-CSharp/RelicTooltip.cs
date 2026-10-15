using I2.Loc;
using PeglinUI;
using PeglinUI.OrbDisplay;
using Relics;
using TMPro;
using UnityEngine;

public class RelicTooltip : MonoBehaviour
{
	public RelicManager _relicManager;

	public TMP_Text headerText;

	public TMP_Text descriptionText;

	public void SetupTooltip(Relic relic)
	{
		headerText.text = LocalizationManager.GetTranslation(relic.nameKey);
		descriptionText.GetComponent<Localize>().Term = relic.descKey;
		TooltipKeywordDescriptions componentInChildren = GetComponentInChildren<TooltipKeywordDescriptions>();
		if (componentInChildren != null)
		{
			string translation = LocalizationManager.GetTranslation(relic.descKey, FixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: true, null, "English");
			componentInChildren.UpdateString(translation);
		}
	}

	public void SetupTooltipLocked(Relic relic)
	{
		headerText.text = "???";
		string translation = LocalizationManager.GetTranslation(relic.lockedInfoKey);
		if (string.IsNullOrEmpty(translation))
		{
			string term = Tooltip.LockedDescPresetToKeyMap[relic.lockedDescriptionPreset];
			if (relic.lockedDescriptionPreset == Tooltip.LockedDescriptionPresets.RELIC_DEFAULT && _relicManager.ShopRelicPool.Contains(relic))
			{
				term = Tooltip.SHOP_ONLY_KEY;
			}
			translation = LocalizationManager.GetTranslation(term);
		}
		descriptionText.text = translation;
		TooltipKeywordDescriptions componentInChildren = GetComponentInChildren<TooltipKeywordDescriptions>();
		if (componentInChildren != null)
		{
			componentInChildren.UpdateString("none");
		}
	}

	public void ShowTooltip(Relic relic, RectTransform iconRect, Vector2 anchorPoint, bool isUnlocked = true)
	{
		base.gameObject.SetActive(value: true);
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
			zero.y = 0f - (rect3.height * pivot.y + rect2.height * component.pivot.y);
		}
		else if (anchorPoint.y > 0.5f)
		{
			zero.y = rect3.height * pivot.y + rect2.height * component.pivot.y;
		}
		component.anchoredPosition += zero;
		float num = rect2.width * component.pivot.x;
		float num2 = rect.width - component.sizeDelta.x * (1f - component.pivot.x);
		float num3 = -1f * (rect.height - component.sizeDelta.y * component.pivot.y);
		float num4 = -1f * component.sizeDelta.y * (1f - component.pivot.y);
		Vector2 anchoredPosition2 = component.anchoredPosition;
		Vector2 vector = zero;
		if (anchoredPosition2.x < num)
		{
			vector.x = 0f - (rect3.width * pivot.x + rect2.width * component.pivot.x);
		}
		else if (anchoredPosition2.x > num2)
		{
			vector.x = rect3.width * pivot.x + rect2.width * component.pivot.x;
		}
		if (anchoredPosition2.y < num3)
		{
			vector.y = 0f - (rect3.height * pivot.y + rect2.height * component.pivot.y);
		}
		else if (anchoredPosition2.y > num4)
		{
			vector.y = rect3.height * pivot.y + rect2.height * component.pivot.y;
		}
		if (vector != zero)
		{
			component.anchoredPosition = anchoredPosition + zero;
		}
		anchoredPosition = component.anchoredPosition;
		component.anchoredPosition = new Vector2(Mathf.Clamp(anchoredPosition.x, num, num2), Mathf.Clamp(anchoredPosition.y, num3, num4));
		if (isUnlocked)
		{
			SetupTooltip(relic);
		}
		else
		{
			SetupTooltipLocked(relic);
		}
	}
}
