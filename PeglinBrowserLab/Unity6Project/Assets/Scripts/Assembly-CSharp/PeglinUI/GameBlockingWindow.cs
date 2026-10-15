using Map.MinimapView;
using PeglinUI.PostBattle;
using Scenarios;
using Scenarios.Shop;
using Tutorial;

namespace PeglinUI
{
	public static class GameBlockingWindow
	{
		public static bool windowOpen
		{
			get
			{
				if (!InventoryViewController.isOpen && !MapPreviewOpenClose.isOpen && !TextScenarioInteractions.isOpen && !TutorialWindow.isOpen && !UpgradeConfirmationPanel.isOpen)
				{
					return UpgradeConfirmationBase.isOpen;
				}
				return true;
			}
		}

		public static bool cursorChangeWindowOpen
		{
			get
			{
				if (!InventoryViewController.isOpen && !MapPreviewOpenClose.isOpen && !TextScenarioInteractions.isOpen && !TutorialWindow.isOpen && !UpgradeConfirmationPanel.isOpen && !UpgradeConfirmationBase.isOpen && !ShopManager.isOpen)
				{
					return BattleUpgradeCanvas.isOpen;
				}
				return true;
			}
		}

		public static bool wasOpenThisFrame
		{
			get
			{
				if (!InventoryViewController.wasOpenThisFrame && !MapPreviewOpenClose.wasOpenThisFrame && !TextScenarioInteractions.isOpen && !TutorialWindow.wasOpenThisFrame && !UpgradeConfirmationPanel.isOpen)
				{
					return UpgradeConfirmationBase.isOpen;
				}
				return true;
			}
		}
	}
}
