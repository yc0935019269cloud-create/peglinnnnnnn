using UnityEngine;

public class BlackHoleNavigation : MonoBehaviour
{
	public int GetNavigationDirection()
	{
		if (base.transform.position.x < -0.5f)
		{
			return -1;
		}
		if (base.transform.position.x >= -0.5f && base.transform.position.x <= 0.5f)
		{
			return 0;
		}
		return 1;
	}
}
