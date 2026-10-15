using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.PostBattle
{
	public class UpgradeDetailsDamageIcon : UpgradeDetails
	{
		public Image dmgImage;

		public Sprite regularPeg;

		public Sprite critPeg;

		public void InitWithIcon(bool isCrit, string before, string after, float beforeBaseToModRatio = 1f, float afterBaseToModRatio = 1f)
		{
			dmgImage.sprite = (isCrit ? critPeg : regularPeg);
			if (string.IsNullOrEmpty(before))
			{
				_beforeText.gameObject.SetActive(value: false);
			}
			if (string.IsNullOrEmpty(after) || before.Equals(after))
			{
				_afterText.gameObject.SetActive(value: false);
			}
			Initialize("", before, after, beforeBaseToModRatio, afterBaseToModRatio);
		}
	}
}
