using UnityEngine;

namespace PeglinUI.UIUtils
{
	public class RectTransformReactToButtonPressDepress : MonoBehaviour
	{
		private RectTransform rectTransform;

		private ButtonDownUpHandler _handler;

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
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
			Vector3 position = rectTransform.position;
			rectTransform.position = new Vector3(position.x, position.y - 0.1f, position.z);
		}

		public void Depress()
		{
			Vector3 position = rectTransform.position;
			rectTransform.position = new Vector3(position.x, position.y + 0.1f, position.z);
		}
	}
}
