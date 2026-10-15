using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Serialization;

namespace PeglinDebug
{
	public class DragonTrailerScene : MonoBehaviour
	{
		public GameObject peglin;

		public Animator peglinAnimator;

		[FormerlySerializedAs("camera")]
		public Camera cam;

		public Transform peglinEndPos;

		public Transform cameraEndPos;

		public float peglinMoveTime = 2f;

		public float cameraMoveTime = 4f;

		private static readonly int MovingAnimParam = Animator.StringToHash("moving");

		private void OnEnable()
		{
			peglinAnimator.SetBool(MovingAnimParam, value: true);
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = peglin.transform.DOMoveX(peglinEndPos.position.x, peglinMoveTime);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(PlayIdleAndMoveCamera));
		}

		private void PlayIdleAndMoveCamera()
		{
			peglinAnimator.SetBool(MovingAnimParam, value: false);
			cam.transform.DOMoveX(cameraEndPos.position.x, cameraMoveTime);
		}
	}
}
