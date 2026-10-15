using UnityEngine;

namespace Scenarios.Shop
{
	public interface IPurchasableItem
	{
		void PurchaseItem();

		Sprite GetPurchasableImage();

		Vector3 GetSpriteLocalScale();

		Color GetSpriteColor();

		int GetCost();

		void ShowTooltip(Vector3 position, Vector3 offset, bool followMousePosition = false);

		bool GetUnlocked();
	}
}
