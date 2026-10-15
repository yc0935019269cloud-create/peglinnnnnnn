using System;
using System.Collections.Generic;
using PeglinUtils;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI
{
	public class ReleaseDemoManager : MonoBehaviour
	{
		public static Action ReleaseDemoToggled;

		public static bool ReleaseDemoActive;

		public List<GameObject> hideInDemo;

		private Player _player;

		private void Start()
		{
			_player = ReInput.players.GetPlayer(0);
			ToggleReleaseDemo(ReleaseDemoActive);
			LoggerButtonHookup loggerButtonHookup = UnityEngine.Object.FindObjectOfType<LoggerButtonHookup>();
			if (loggerButtonHookup != null)
			{
				Canvas component = loggerButtonHookup.transform.parent.GetComponent<Canvas>();
				component.worldCamera = Camera.main;
				component.gameObject.AddComponent<GraphicRaycaster>();
			}
		}

		private void Update()
		{
			if (((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.F7)) || (_player.GetButton("DeckButton") && _player.GetButtonDown("Speedup")))
			{
				ReleaseDemoActive = !ReleaseDemoActive;
				ToggleReleaseDemo(ReleaseDemoActive);
				ReleaseDemoToggled?.Invoke();
				Debug.Log("Debug mode toggled: " + ReleaseDemoActive);
			}
		}

		private void ToggleReleaseDemo(bool demoModeActive)
		{
			if (hideInDemo == null || hideInDemo.Count <= 0)
			{
				return;
			}
			foreach (GameObject item in hideInDemo)
			{
				item.SetActive(!demoModeActive);
			}
		}
	}
}
