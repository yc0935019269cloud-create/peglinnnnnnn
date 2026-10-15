using UnityEngine;
using UnityEngine.UI;

namespace PeglinUtils
{
	public class LoggerButtonHookup : MonoBehaviour
	{
		public Button copyButton;

		public Button closeButton;

		private ShowLogInLogger logger;

		private void Awake()
		{
			logger = Object.FindObjectOfType<ShowLogInLogger>();
		}

		private void OnEnable()
		{
			if (!(logger == null))
			{
				copyButton.onClick.AddListener(logger.CopyLogToClipboard);
				GetComponentInParent<GraphicRaycaster>().enabled = true;
				closeButton.onClick.AddListener(() =>
				{
					logger.CloseLog();
					base.gameObject.SetActive(value: false);
					GetComponentInParent<GraphicRaycaster>().enabled = false;
				});
			}
		}

		private void OnDisable()
		{
			if (!(logger == null))
			{
				copyButton.onClick.RemoveAllListeners();
				closeButton.onClick.RemoveAllListeners();
			}
		}
	}
}
