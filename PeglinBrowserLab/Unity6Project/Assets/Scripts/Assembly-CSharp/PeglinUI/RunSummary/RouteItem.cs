using Stats;
using UnityEngine;
using UnityEngine.UI;
using Worldmap;

namespace PeglinUI.RunSummary
{
	public class RouteItem : MonoBehaviour
	{
		public Sprite battleNodeSprite;

		public Sprite eliteNodeSprite;

		public Sprite bossNodeSprite;

		public Sprite qMarkSprite;

		public Sprite shopSprite;

		public Sprite treasureChestSprite;

		public Sprite[] bossSprites;

		[Header("Scene References")]
		public Image icon;

		public Image arrow;

		public void Initialize(RoomType roomType, bool isLast)
		{
			switch (roomType)
			{
			case RoomType.NONE:
			case RoomType.SCENARIO:
			case RoomType.UNKNOWN:
			case RoomType.PEG_MINIGAME:
				icon.sprite = qMarkSprite;
				break;
			case RoomType.STORE:
				icon.sprite = shopSprite;
				break;
			case RoomType.TREASURE:
				icon.sprite = treasureChestSprite;
				break;
			case RoomType.BATTLE:
				icon.sprite = battleNodeSprite;
				break;
			case RoomType.MINI_BOSS:
				icon.sprite = eliteNodeSprite;
				break;
			case RoomType.BOSS:
				icon.sprite = bossNodeSprite;
				break;
			}
			if (isLast)
			{
				arrow.gameObject.SetActive(value: false);
			}
		}

		public void InitializeBoss(RunStats.BossType bossType, bool isLast)
		{
			int num = (int)(bossType - 1);
			if (bossSprites.Length - 1 < num || num < 0)
			{
				Debug.LogWarning("Did not find boss sprite!");
				icon.sprite = bossNodeSprite;
			}
			else
			{
				icon.sprite = bossSprites[num];
			}
			icon.color = Color.white;
			if (isLast)
			{
				arrow.gameObject.SetActive(value: false);
			}
		}
	}
}
