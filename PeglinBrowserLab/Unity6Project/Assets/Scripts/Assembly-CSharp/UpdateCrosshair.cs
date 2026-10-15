using UnityEngine;

public class UpdateCrosshair : MonoBehaviour
{
	private float _initialDistance = 2.5f;

	private PachinkoBall _fireball;

	private SpriteRenderer _renderer;

	private void Start()
	{
		_initialDistance = base.transform.localPosition.x;
		_fireball = base.transform.parent.GetComponent<PachinkoBall>();
		_renderer = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		if (_fireball.IsAiming())
		{
			_renderer.enabled = true;
		}
		else
		{
			_renderer.enabled = false;
		}
	}
}
