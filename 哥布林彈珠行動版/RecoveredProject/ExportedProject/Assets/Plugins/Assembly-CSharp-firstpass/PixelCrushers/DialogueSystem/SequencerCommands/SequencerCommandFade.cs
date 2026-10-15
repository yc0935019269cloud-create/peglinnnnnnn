using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
	[AddComponentMenu("")]
	public class SequencerCommandFade : SequencerCommand
	{
		private const float SmoothMoveCutoff = 0.05f;

		private const int FaderCanvasSortOrder = 32760;

		private string direction;

		private float duration;

		private Color color;

		private bool fadeIn;

		private bool stay;

		private bool unstay;

		private float startTime;

		private float endTime;

		private static Canvas faderCanvas;

		private static Image faderImage;

		public void Awake()
		{
			direction = GetParameter(0);
			duration = GetParameterAsFloat(1, 1f);
			color = Tools.WebColor(GetParameter(2, "#000000"));
			if (DialogueDebug.logInfo)
			{
				Debug.Log(string.Format(CultureInfo.InvariantCulture, "{0}: Sequencer: Fade({1}, {2}, {3})", "Dialogue System", direction, duration, color));
			}
			stay = string.Equals(direction, "stay", StringComparison.OrdinalIgnoreCase);
			unstay = string.Equals(direction, "unstay", StringComparison.OrdinalIgnoreCase);
			fadeIn = unstay || string.Equals(direction, "in", StringComparison.OrdinalIgnoreCase);
			if (unstay && faderImage != null && Mathf.Approximately(0f, faderImage.color.a))
			{
				Stop();
			}
			else if (duration > 0.05f)
			{
				if (faderCanvas == null)
				{
					faderCanvas = new GameObject("Canvas (Fader)", typeof(Canvas)).GetComponent<Canvas>();
					faderCanvas.transform.SetParent(DialogueManager.instance.transform);
					faderCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
					faderCanvas.sortingOrder = 32760;
				}
				if (faderImage == null)
				{
					faderImage = new GameObject("Fader Image", typeof(Image)).GetComponent<Image>();
					faderImage.transform.SetParent(faderCanvas.transform, worldPositionStays: false);
					faderImage.rectTransform.anchorMin = Vector2.zero;
					faderImage.rectTransform.anchorMax = Vector2.one;
					faderImage.sprite = null;
				}
				faderCanvas.gameObject.SetActive(value: true);
				faderImage.gameObject.SetActive(value: true);
				startTime = DialogueTime.time;
				endTime = startTime + duration;
				faderImage.color = new Color(color.r, color.g, color.b, fadeIn ? 1 : 0);
			}
			else
			{
				Stop();
			}
		}

		public void Update()
		{
			if (DialogueTime.time < endTime && faderImage != null)
			{
				float num = (DialogueTime.time - startTime) / duration;
				float a = (fadeIn ? (1f - num) : num);
				faderImage.color = new Color(color.r, color.g, color.b, a);
			}
			else
			{
				Stop();
			}
		}

		public void OnDestroy()
		{
			if (faderCanvas != null)
			{
				faderCanvas.gameObject.SetActive(stay);
			}
			if (faderImage != null)
			{
				faderImage.gameObject.SetActive(stay);
				faderImage.color = new Color(color.r, color.g, color.b, (!fadeIn) ? 1 : 0);
			}
		}
	}
}
