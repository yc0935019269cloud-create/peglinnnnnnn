using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class StandardUITimer : MonoBehaviour
	{
		private Slider slider;

		private float m_startTime;

		public virtual void Awake()
		{
			slider = GetComponent<Slider>();
		}

		public virtual void StartCountdown(float duration, Action timeoutHandler)
		{
			StartCoroutine(Countdown(duration, timeoutHandler));
		}

		private IEnumerator Countdown(float duration, Action timeoutHandler)
		{
			m_startTime = DialogueTime.time;
			float endTime = m_startTime + duration;
			while (DialogueTime.time < endTime)
			{
				float num = DialogueTime.time - m_startTime;
				UpdateTimeLeft(Mathf.Clamp(1f - num / duration, 0f, 1f));
				yield return null;
			}
			timeoutHandler?.Invoke();
		}

		public virtual void StopCountdown()
		{
			StopAllCoroutines();
		}

		public void SkipTime(float amountToSkip)
		{
			m_startTime -= amountToSkip;
		}

		public virtual void UpdateTimeLeft(float normalizedTimeLeft)
		{
			if (!(slider == null))
			{
				slider.value = normalizedTimeLeft;
			}
		}
	}
}
