using TMPro;
using UnityEngine;

namespace PeglinUI.SettingsMenu
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class SetAudioPercentage : MonoBehaviour
	{
		public int multiplier = 2;

		private TextMeshProUGUI _text;

		private void OnEnable()
		{
			if (_text == null)
			{
				_text = GetComponent<TextMeshProUGUI>();
			}
		}

		public void SetPercentageText(float value)
		{
			if (_text == null)
			{
				_text = GetComponent<TextMeshProUGUI>();
			}
			int num = Mathf.RoundToInt((value - 0.001f) / 0.999f * 100f) * multiplier;
			_text.text = $"{num}%";
		}
	}
}
