using DG.Tweening;
using UnityEngine;

namespace Battle.Effects
{
	public class ShakeCamAct4 : MonoBehaviour
	{
		private Camera _cam;

		private void OnEnable()
		{
			_cam = Camera.main;
		}

		public void DoShake()
		{
			_cam.DOShakePosition(0.05f, 1f, 100, 20f, fadeOut: false);
		}
	}
}
