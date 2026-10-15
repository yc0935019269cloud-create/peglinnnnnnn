using System;
using PeglinUI;
using UnityEngine;

namespace UI
{
	public class DeckButton : MonoBehaviour
	{
		private Animator _anim;

		private static readonly int InventoryOpen = Animator.StringToHash("InventoryOpen");

		private void Awake()
		{
			_anim = GetComponentInChildren<Animator>();
		}

		private void OnEnable()
		{
			InventoryViewController.onInventoryViewToggled = (InventoryViewController.InventoryViewToggle)Delegate.Combine(InventoryViewController.onInventoryViewToggled, new InventoryViewController.InventoryViewToggle(InventoryViewToggled));
		}

		private void OnDisable()
		{
			InventoryViewController.onInventoryViewToggled = (InventoryViewController.InventoryViewToggle)Delegate.Remove(InventoryViewController.onInventoryViewToggled, new InventoryViewController.InventoryViewToggle(InventoryViewToggled));
		}

		private void InventoryViewToggled(bool open)
		{
			_anim.SetBool(InventoryOpen, open);
		}
	}
}
