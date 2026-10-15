using System.Collections;
using UnityEngine;

public class UpdateBar : MonoBehaviour
{
	public float BarLerpTime = 0.5f;

	public float BarFlashTime = 0.2f;

	private float _initialScale = 1f;

	private float _targetScale = 1f;

	private float _currentScale = 1f;

	private float _scaleMoveVel;

	private Color _initialColour = Color.red;

	private SpriteRenderer _renderer;

	private void OnEnable()
	{
		_initialScale = base.transform.localScale.x;
		_renderer = GetComponent<SpriteRenderer>();
		_initialColour = _renderer.color;
	}

	private void Update()
	{
		if (_currentScale != _targetScale)
		{
			_currentScale = Mathf.SmoothDamp(_currentScale, _targetScale, ref _scaleMoveVel, BarLerpTime);
			Vector3 localScale = base.transform.localScale;
			localScale.x = _currentScale;
			base.transform.localScale = localScale;
		}
	}

	public void UpdateSize(float percent)
	{
		_targetScale = _initialScale * percent;
		StartCoroutine(FlashWhite());
	}

	private IEnumerator FlashWhite()
	{
		_renderer.color = Color.white;
		yield return new WaitForSeconds(BarFlashTime);
		_renderer.color = _initialColour;
	}
}
