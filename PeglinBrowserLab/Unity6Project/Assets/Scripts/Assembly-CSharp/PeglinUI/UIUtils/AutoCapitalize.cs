using I2.Loc;
using TMPro;
using UnityEngine;

namespace PeglinUI.UIUtils
{
	[RequireComponent(typeof(Localize))]
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class AutoCapitalize : MonoBehaviour
	{
		private Localize loc;

		private void OnEnable()
		{
			Uppercase();
			loc = GetComponent<Localize>();
			loc.LocalizeEvent.AddListener(Uppercase);
		}

		private void OnDisable()
		{
			loc.LocalizeEvent.RemoveListener(Uppercase);
		}

		private void Uppercase()
		{
			TextMeshProUGUI component = GetComponent<TextMeshProUGUI>();
			component.text = component.text.ToUpper();
		}
	}
}
