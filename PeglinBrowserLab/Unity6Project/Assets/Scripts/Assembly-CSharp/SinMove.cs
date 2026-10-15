using UnityEngine;

public class SinMove : MonoBehaviour
{
	public float speed = 1f;

	public float height = 0.3f;

	private float _initialY;

	private void Start()
	{
		_initialY = base.transform.position.y;
	}

	private void Update()
	{
		Vector3 position = base.transform.position;
		position.y = _initialY + height * Mathf.Sin(Time.time * speed);
		base.transform.position = position;
	}
}
