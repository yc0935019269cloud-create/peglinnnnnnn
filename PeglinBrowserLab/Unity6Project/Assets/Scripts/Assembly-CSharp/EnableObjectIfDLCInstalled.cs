using UnityEngine;

public class EnableObjectIfDLCInstalled : MonoBehaviour
{
	public GameObject objectToToggle;

	private void Start()
	{
		objectToToggle.SetActive(IAPManager.IsSupporterDLCInstalled);
	}
}
