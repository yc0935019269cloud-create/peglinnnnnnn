using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class FallingChandelier : MonoBehaviour
{
	public GameObject explosion;

	public float fallToYPos = 5.797f;

	public float fallTime = 0.3f;

	public float landTime = 1f;

	public float fadeTime = 0.4f;

	private Action _onLandAction;

	private void Start()
	{
		StartCoroutine(FallAnimation());
	}

	private IEnumerator FallAnimation()
	{
		base.transform.DOMoveY(fallToYPos, fallTime).SetEase(Ease.InQuad);
		yield return new WaitForSeconds(fallTime);
		explosion.SetActive(value: true);
		_onLandAction();
		yield return new WaitForSeconds(landTime);
		GetComponent<SpriteRenderer>().DOFade(0f, fadeTime);
		yield return new WaitForSeconds(fadeTime);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void SetOnLandAction(Action action)
	{
		_onLandAction = action;
	}
}
