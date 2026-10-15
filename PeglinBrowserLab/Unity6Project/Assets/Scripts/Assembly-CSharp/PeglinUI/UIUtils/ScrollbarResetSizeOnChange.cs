using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.UIUtils
{
	[RequireComponent(typeof(Scrollbar))]
	public class ScrollbarResetSizeOnChange : MonoBehaviour
	{
		private Scrollbar scrollbar;

		private void Awake()
		{
			scrollbar = GetComponent<Scrollbar>();
		}

		public void ResetSizeToZero()
		{
			scrollbar.size = 0f;
		}
	}
}
