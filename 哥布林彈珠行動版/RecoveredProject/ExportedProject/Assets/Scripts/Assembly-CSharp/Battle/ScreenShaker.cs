using DG.Tweening;
using UnityEngine;

namespace Battle
{
	public class ScreenShaker : MonoBehaviour
	{
		public bool blocked;

		[Header("ScreenShake")]
		public float screenShakeDurationMin = 0.7f;

		public float screenShakeDurationMax = 1.1f;

		public float screenShakeIntensityBase = 1.65f;

		public float screenShakeIntensityMax = 3.2f;

		public float screenShakeIntensityReference = 275f;

		private Camera _camera;

		[HideInInspector]
		public Vector3 initialCameraPos;

		private void Awake()
		{
			_camera = GetComponent<Camera>();
			initialCameraPos = _camera.transform.position;
		}

		private void Start()
		{
			blocked = SettingsManager.Instance == null && SettingsManager.Instance.ScreenshakeInstensity <= 0f;
		}

		public void BulkDamageScreenShake(float damage)
		{
			if (!blocked)
			{
				_camera.DOKill();
				_camera.transform.position = initialCameraPos;
				float num = ((SettingsManager.Instance != null) ? SettingsManager.Instance.ScreenshakeInstensity : 1f);
				if (num > 0f)
				{
					float num2 = damage / screenShakeIntensityReference * num;
					float duration = Mathf.Lerp(screenShakeDurationMin, screenShakeDurationMax, num2);
					_camera.DOShakePosition(duration, Mathf.Min(screenShakeIntensityBase * num2, screenShakeIntensityMax));
				}
			}
		}
	}
}
