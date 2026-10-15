using DG.Tweening;
using UnityEngine;

public class CanvasFadeIn : MonoBehaviour
{
	public float fadeTime = 0.7f;

	private void OnEnable()
	{
		GetComponent<CanvasGroup>().DOFade(1f, fadeTime).From(0f);
	}
}
