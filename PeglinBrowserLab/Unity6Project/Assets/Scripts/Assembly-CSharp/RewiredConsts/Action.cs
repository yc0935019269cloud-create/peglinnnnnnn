using Rewired.Dev;

namespace RewiredConsts
{
	public static class Action
	{
		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Confirm")]
		public const int Confirm = 0;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Back")]
		public const int Back = 1;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Horizontal")]
		public const int Horizontal = 2;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "HorizontalSlow")]
		public const int HorizontalSlow = 19;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Vertical")]
		public const int Vertical = 3;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "VerticalSlow")]
		public const int VerticalSlow = 20;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Pause")]
		public const int Pause = 4;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "ViewRelics")]
		public const int RelicInfo = 5;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "View the active orb details")]
		public const int OrbInfo = 6;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Menu - Horizontal")]
		public const int UIHorizontal = 7;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Menu - Vertical")]
		public const int UIVertical = 8;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Menu Confirm")]
		public const int UISubmit = 9;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Menu Cancel")]
		public const int UICancel = 10;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Left Mouse Click")]
		public const int MouseClick = 11;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Pause Menu")]
		public const int PauseMenu = 12;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Map Button")]
		public const int MapButton = 13;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Deck Button")]
		public const int DeckButton = 14;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Action0")]
		public const int MouseMove = 15;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Change Target Left")]
		public const int TargetLeft = 16;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Change Target Right")]
		public const int TargetRight = 17;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Swap Tab Left")]
		public const int MenuTabLeft = 21;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Swap Tab Right")]
		public const int MenuTabRight = 22;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "CruciballToggle")]
		public const int CruciballToggle = 23;

		[ActionIdFieldInfo(categoryName = "Default", friendlyName = "Speedup")]
		public const int Speedup = 24;
	}
}
