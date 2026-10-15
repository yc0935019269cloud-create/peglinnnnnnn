using UnityEngine;

public class sineWobble : MonoBehaviour
{
	public float speed = 1f;

	public float xAmplitude = 1f;

	public float yAmplitude = 1f;

	public float zAmplitude = 1f;

	private Quaternion _initial;

	private Quaternion newAngle;

	private void Start()
	{
		_initial.x = base.transform.rotation.x;
		_initial.y = base.transform.rotation.y;
		_initial.z = base.transform.rotation.y;
	}

	private void Update()
	{
		newAngle = base.transform.rotation;
		newAngle.x = _initial.x + xAmplitude * Mathf.Sin(Time.time * speed);
		newAngle.y = _initial.y + yAmplitude * Mathf.Sin(Time.time * speed);
		newAngle.z = _initial.z + zAmplitude * Mathf.Sin(Time.time * speed);
		base.transform.rotation = newAngle;
	}

	private void onDisable()
	{
		base.transform.rotation = default;
	}
}
