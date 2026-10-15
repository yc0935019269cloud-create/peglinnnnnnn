using UnityEngine;
using UnityEngine.UI;

public class ToggleOptionBackgroundImage : MonoBehaviour
{
	private void Start()
	{
		int num = 1;
		foreach (Transform item in base.transform)
		{
			if (item.gameObject.activeInHierarchy)
			{
				Image component = item.GetComponent<Image>();
				if (component != null)
				{
					component.enabled = num == 1;
					num = 1 - num;
				}
			}
		}
	}

	private void Update()
	{
	}
}
