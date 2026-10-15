using System;
using PeglinUI;
using TMPro;
using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(TMP_Text))]
	public class VersionDisplay : MonoBehaviour
	{
		private static bool shouldDisplay;

		private TMP_Text _text;

		private string _versionString = "";

		private void Start()
		{
			_text = GetComponent<TMP_Text>();
			shouldDisplay = _text.enabled;
			_versionString = "v" + Application.version;
			_text.text = _versionString;
		}

		private void OnEnable()
		{
			ReleaseDemoManager.ReleaseDemoToggled = (Action)Delegate.Combine(ReleaseDemoManager.ReleaseDemoToggled, new Action(ToggleDemoText));
		}

		private void OnDisable()
		{
			ReleaseDemoManager.ReleaseDemoToggled = (Action)Delegate.Remove(ReleaseDemoManager.ReleaseDemoToggled, new Action(ToggleDemoText));
		}

		private void ToggleDemoText()
		{
			if (ReleaseDemoManager.ReleaseDemoActive)
			{
				_text.text = _versionString + " Demo";
			}
			else
			{
				_text.text = _versionString;
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F6))
			{
				shouldDisplay = !shouldDisplay;
				_text.enabled = shouldDisplay;
			}
		}
	}
}
