using TMPro;
using UnityEngine;

namespace PeglinUI
{
	public class ResizeRectBasedOnText : MonoBehaviour
	{
		private TMP_Text tmp;

		private void Awake()
		{
			tmp = GetComponent<TextMeshProUGUI>();
		}

		private void Update()
		{
			Vector2 preferredValues = tmp.GetPreferredValues(tmp.text);
			tmp.rectTransform.sizeDelta = preferredValues;
		}
	}
}
