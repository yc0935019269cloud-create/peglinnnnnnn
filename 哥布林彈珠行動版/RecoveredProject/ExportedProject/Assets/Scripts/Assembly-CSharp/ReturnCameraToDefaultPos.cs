using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class ReturnCameraToDefaultPos : MonoBehaviour
{
	public delegate void CameraFinishLerp();

	public CameraFinishLerp onCameraFinishedLerp = () =>
	{
	};

	public bool freeCameraMoving;

	private Vector3 _initialPosition = Vector3.zero;

	public Vector3 initialPosition => _initialPosition;

	private void Start()
	{
		_initialPosition = base.transform.position;
	}

	public void SetNewDefaultPosition()
	{
		_initialPosition = base.transform.position;
	}

	public void ReturnToDefault(float duration = 1f, bool force = false)
	{
		if (force)
		{
			freeCameraMoving = false;
		}
		Vector3 vector = initialPosition;
		vector.z = base.transform.position.z;
		if (Mathf.Approximately(duration, 0f))
		{
			base.transform.position = vector;
			LerpComplete();
			return;
		}
		if (duration < 0f)
		{
			duration = Mathf.Min(1f, (base.transform.position - vector).magnitude / 2f);
		}
		if (!freeCameraMoving)
		{
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = base.transform.DOMove(vector, duration);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(LerpComplete));
		}
	}

	private void LerpComplete()
	{
		onCameraFinishedLerp?.Invoke();
	}
}
