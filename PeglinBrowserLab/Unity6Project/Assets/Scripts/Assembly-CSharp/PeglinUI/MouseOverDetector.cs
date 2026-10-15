using UnityEngine;

namespace PeglinUI
{
	public class MouseOverDetector : MonoBehaviour
	{
		public delegate void MouseEvent(GameObject gameObject);

		public LayerMask UiLayerMask;

		public MouseEvent OnMouseEntered;

		public MouseEvent OnMouseExited;

		private bool mouseEnteredCalled;

		private void OnMouseOver()
		{
			if (!mouseEnteredCalled)
			{
				OnMouseEntered?.Invoke(base.gameObject);
				mouseEnteredCalled = true;
			}
		}

		private void OnMouseExit()
		{
			OnMouseExited?.Invoke(base.gameObject);
			mouseEnteredCalled = false;
		}
	}
}
