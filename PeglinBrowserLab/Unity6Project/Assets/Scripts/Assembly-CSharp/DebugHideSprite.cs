using UnityEngine;

public class DebugHideSprite : MonoBehaviour
{
	private void Start()
	{
		GetComponent<SpriteRenderer>().enabled = false;
	}
}
