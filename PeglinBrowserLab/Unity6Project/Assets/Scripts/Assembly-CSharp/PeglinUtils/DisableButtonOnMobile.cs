using UnityEngine;
using UnityEngine.UI;

namespace PeglinUtils
{
	public class DisableButtonOnMobile : MonoBehaviour
	{
		private void Start()
		{
			if (Application.isMobilePlatform)
			{
				GetComponent<Button>().enabled = false;
			}
		}
	}
}
