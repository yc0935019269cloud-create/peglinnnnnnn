using System;
using Map.MinimapView;
using PeglinUI;
using UnityEngine;

namespace UI
{
	public class GameBlockingWindowHide : MonoBehaviour
	{
		public GameObject toControl;

		public MonoBehaviour altControl;

		private void OnEnable()
		{
			PauseMenu.OnPauseToggled = (PauseMenu.PauseToggle)Delegate.Combine(PauseMenu.OnPauseToggled, new PauseMenu.PauseToggle(ToggleActive));
			MapPreviewOpenClose.onMapPreviewToggled = (MapPreviewOpenClose.MapPreviewToggle)Delegate.Combine(MapPreviewOpenClose.onMapPreviewToggled, new MapPreviewOpenClose.MapPreviewToggle(ToggleActive));
			InventoryViewController.onInventoryViewToggled = (InventoryViewController.InventoryViewToggle)Delegate.Combine(InventoryViewController.onInventoryViewToggled, new InventoryViewController.InventoryViewToggle(ToggleActive));
		}

		private void OnDisable()
		{
			PauseMenu.OnPauseToggled = (PauseMenu.PauseToggle)Delegate.Remove(PauseMenu.OnPauseToggled, new PauseMenu.PauseToggle(ToggleActive));
			MapPreviewOpenClose.onMapPreviewToggled = (MapPreviewOpenClose.MapPreviewToggle)Delegate.Remove(MapPreviewOpenClose.onMapPreviewToggled, new MapPreviewOpenClose.MapPreviewToggle(ToggleActive));
			InventoryViewController.onInventoryViewToggled = (InventoryViewController.InventoryViewToggle)Delegate.Remove(InventoryViewController.onInventoryViewToggled, new InventoryViewController.InventoryViewToggle(ToggleActive));
		}

		private void ToggleActive(bool windowVisible)
		{
			bool flag = GameBlockingWindow.windowOpen || PauseMenu.Paused;
			toControl?.SetActive(!flag);
			if (altControl != null)
			{
				altControl.enabled = !flag;
			}
		}
	}
}
