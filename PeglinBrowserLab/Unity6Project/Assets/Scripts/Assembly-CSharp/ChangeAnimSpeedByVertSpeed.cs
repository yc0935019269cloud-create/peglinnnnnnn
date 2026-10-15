using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ChangeAnimSpeedByVertSpeed : MonoBehaviour
{
	private Animator _animator;

	public float noMoveThreshold = 0.08f;

	public float minSpeed = 0.6f;

	public float maxSpeed = 2f;

	private float _prevY = float.MaxValue;

	private void Start()
	{
		_animator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (_prevY > 100000f)
		{
			_prevY = base.transform.position.y;
			return;
		}
		float num = (_prevY - base.transform.position.y) / Time.deltaTime;
		if (Mathf.Abs(num) < noMoveThreshold)
		{
			num = 0f;
		}
		else if (Mathf.Abs(num) < minSpeed)
		{
			num = minSpeed * Mathf.Sign(num);
		}
		else if (Mathf.Abs(num) > maxSpeed)
		{
			num = maxSpeed * Mathf.Sign(num);
		}
		_animator.SetFloat("walkspeed", num);
		_prevY = base.transform.position.y;
	}
}
