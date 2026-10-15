using System;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
	[SerializeField]
	private Vector3 _velocity = new Vector2(0f, 1f);

	public float timeToFade = 1f;

	protected TextMeshProUGUI _text;

	protected Localize _localize;

	private Vector3 _startingPosition = Vector3.zero;

	private bool _shouldDie;

	private Tween _moveTween;

	private void Awake()
	{
		_text = GetComponentInChildren<TextMeshProUGUI>();
		_localize = GetComponentInChildren<Localize>();
		_text.enabled = false;
		base.enabled = false;
		base.gameObject.SetActive(value: false);
	}

	public void Activate(bool shouldDie = false)
	{
		_startingPosition = base.transform.localPosition;
		if (_text.enabled)
		{
			_moveTween.Kill();
			_moveTween = null;
			Move();
		}
		else
		{
			_text.enabled = true;
			base.gameObject.SetActive(value: true);
			base.enabled = true;
			_shouldDie = shouldDie;
			Move();
		}
	}

	public void Activate(string text, bool shouldDie = false)
	{
		if (_text != null)
		{
			Activate(shouldDie);
			_text.text = text;
		}
	}

	public void ActivateLocalized(string locKey, bool shouldDie = false)
	{
		if (_localize != null && _text != null)
		{
			_localize.Term = locKey;
			Activate(shouldDie);
		}
	}

	public void SetColor(Color color)
	{
		if (_text != null)
		{
			_text.color = color;
		}
	}

	protected virtual void Move()
	{
		_moveTween = base.transform.DOLocalMove(_startingPosition + _velocity, timeToFade);
		Tween moveTween = _moveTween;
		moveTween.onComplete = (TweenCallback)Delegate.Combine(moveTween.onComplete, new TweenCallback(DieOrDeactivate));
	}

	protected void DieOrDeactivate()
	{
		if (_shouldDie)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		base.enabled = false;
		base.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		_text.enabled = false;
		base.transform.localPosition = _startingPosition;
		base.gameObject.SetActive(value: false);
	}
}
