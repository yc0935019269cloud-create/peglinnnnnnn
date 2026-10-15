using UnityEngine;

public class SummoningCircleAimingSpriteRotater : MonoBehaviour
{
	public float rotationPerFrame = 0.1f;

	private void Update()
	{
		base.transform.Rotate(0f, 0f, rotationPerFrame * Time.deltaTime);
	}
}
