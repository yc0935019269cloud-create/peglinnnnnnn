using System;
using Battle.Enemies.Attacks;
using DG.Tweening;
using UnityEngine;

public class SlowDownTimeForAttack : MonoBehaviour
{
	public float slowedTimeScale = 0.5f;

	public float goToSlowedTimeScaleLength = 0.2f;

	public float midSlowdownDelay = 0.05f;

	public float returnToNormalTimeScaleLength = 0.2f;

	private Projectile projectile;

	private void Awake()
	{
		projectile = GetComponent<Projectile>();
		if (projectile != null)
		{
			Projectile obj = projectile;
			obj.OnProjectileFired = (Projectile.ProjectileFired)Delegate.Combine(obj.OnProjectileFired, new Projectile.ProjectileFired(OnProjectileFired));
		}
	}

	private void OnProjectileFired(float fireLength)
	{
		float delay = 0.1f;
		if (fireLength != -1f)
		{
			delay = fireLength - goToSlowedTimeScaleLength;
		}
		float num = 1f;
		if (TimescaleManager.Instance.isSpedUp)
		{
			num = SettingsManager.Instance.SpeedupLevel;
		}
		Sequence sequence = DOTween.Sequence();
		sequence.Append(DOTween.To(() => Time.timeScale, (float x) =>
		{
			Time.timeScale = x;
		}, slowedTimeScale, goToSlowedTimeScaleLength).From(num).SetEase(Ease.OutQuad)
			.SetDelay(delay));
		sequence.Append(DOTween.To(() => Time.timeScale, (float x) =>
		{
			Time.timeScale = x;
		}, num, returnToNormalTimeScaleLength).SetEase(Ease.InQuad).SetDelay(midSlowdownDelay));
		sequence.AppendCallback(ResetTimeScale);
		sequence.Play();
	}

	private void ResetTimeScale()
	{
		if (TimescaleManager.Instance.isSpedUp)
		{
			Time.timeScale = SettingsManager.Instance.SpeedupLevel;
		}
		else
		{
			Time.timeScale = 1f;
		}
	}
}
