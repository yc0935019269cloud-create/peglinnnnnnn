using PeglinUI.UIUtils;
using TMPro;
using UnityEngine;

namespace PeglinUI
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class TextReactToButtonPressDepress : MonoBehaviour
	{
		public float yAmount = 5f;

		private TextMeshProUGUI _text;

		private ButtonDownUpHandler _handler;

		private void Awake()
		{
			_text = GetComponent<TextMeshProUGUI>();
			_handler = GetComponentInParent<ButtonDownUpHandler>();
			_handler.onPointerDown.AddListener(Press);
			_handler.onPointerUp.AddListener(Depress);
		}

		private void OnDestroy()
		{
			_handler.onPointerDown.RemoveAllListeners();
			_handler.onPointerUp.RemoveAllListeners();
		}

		public void Press()
		{
			_text.margin = new Vector4(_text.margin.x, _text.margin.y + yAmount, _text.margin.z, _text.margin.w);
		}

		public void Depress()
		{
			_text.margin = new Vector4(_text.margin.x, _text.margin.y - yAmount, _text.margin.z, _text.margin.w);
		}
	}
}
