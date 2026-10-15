using System.Collections;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.UnityGUI
{
	[AddComponentMenu("")]
	public class FadeEffect : GUIEffect
	{
		public float fadeInDuration = 0.5f;

		public float duration = 1f;

		public float fadeOutDuration = 0.5f;

		private GUIVisibleControl control;

		public void SetFadeDurations(float fadeInDuration, float duration, float fadeOutDuration)
		{
			this.fadeInDuration = fadeInDuration;
			this.duration = duration;
			this.fadeOutDuration = fadeOutDuration;
		}

		public override IEnumerator Play()
		{
			control = GetComponent<GUIVisibleControl>();
			if (control == null)
			{
				yield break;
			}
			float startTime2 = DialogueTime.time;
			float endTime2 = startTime2 + fadeInDuration;
			while (DialogueTime.time < endTime2)
			{
				float num = DialogueTime.time - startTime2;
				control.Alpha = num / fadeInDuration;
				yield return null;
			}
			control.Alpha = 1f;
			if (!Tools.ApproximatelyZero(fadeOutDuration))
			{
				yield return StartCoroutine(DialogueTime.WaitForSeconds(duration));
				startTime2 = DialogueTime.time;
				endTime2 = startTime2 + fadeOutDuration;
				while (DialogueTime.time < endTime2)
				{
					float num2 = DialogueTime.time - startTime2;
					control.Alpha = 1f - num2 / fadeOutDuration;
					yield return null;
				}
				control.Alpha = 0f;
			}
		}
	}
}
