using Relics;
using UnityEngine;

namespace Peglin.PegMinigame
{
	public class RelicReward : Reward
	{
		public delegate void RelicTooltipWorldPos(Relic relic, Vector2 pos, Vector2 anchorPoint);

		public delegate void HideRelicTooltip(bool onDisable);

		public static RelicTooltipWorldPos ShowTooltipWorldPos = delegate
		{
		};

		public static HideRelicTooltip HideTooltip = delegate
		{
		};

		private Relic _relic;

		private RelicManager _relicManager;

		private GameObject _infoWidgetPrefab;

		private RelicTooltip _infoWidgetInstance;

		public Relic Relic
		{
			get
			{
				return _relic;
			}
			set
			{
				_relic = value;
			}
		}

		public RelicReward(Relic relic, RelicManager relicManager, GameObject relicInfoWidget)
		{
			_relic = relic;
			_relicManager = relicManager;
			_infoWidgetPrefab = relicInfoWidget;
		}

		public override Sprite GetSprite()
		{
			return _relic.sprite;
		}

		public override void Grant()
		{
			_relicManager.AddRelic(_relic);
		}

		public override Color GetSpriteColor()
		{
			return Color.white;
		}

		public override Vector3 GetSpriteLocalScale()
		{
			return Vector3.one;
		}

		public override void DrawTooltip(Canvas canvas, Vector3 position)
		{
			TooltipManager.Instance.ShowTooltipRelic(_relic, position, new Vector3(1f, 1f), followMousePos: true);
		}

		public override void DisableTooltip()
		{
			TooltipManager.Instance.HideTooltip();
		}
	}
}
