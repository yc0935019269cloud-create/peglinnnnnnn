using TMPro;
using UnityEngine;

namespace PeglinUI.RunSummary
{
	public class StatsLine : MonoBehaviour
	{
		public TextMeshProUGUI statsNameText;

		public TextMeshProUGUI statsValueText;

		public void Initialize(string title, string value)
		{
			statsNameText.text = title;
			statsValueText.text = value;
		}
	}
}
