using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class RandomVerticalMovement : MonoBehaviour
{
	private enum TweenType
	{
		NO_HORIZONTAL_MOV = 0,
		HORIZONTAL_MOV = 1,
		STRETCH = 2
	}

	[Header("Parameters")]
	public int minNumberOfTweensPerSequence = 2;

	public int maxNumberOfTweensPerSequence = 5;

	public float startY;

	public float endY;

	[Tooltip("Cumulative with chance of stretch, add up to 1. Rest will be chance of it just slowly moving vertically")]
	public float chanceOfXMovement = 0.5f;

	public float chanceOfStretch = 0.1f;

	public float maxXMovement = 0.5f;

	public float minXMovement = 0.2f;

	[Header("External References")]
	public SpriteRenderer[] sprites;

	private void Awake()
	{
	}

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(2f);
		SpriteRenderer[] array = sprites;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.transform.position = new Vector3(spriteRenderer.transform.position.x, startY);
			CreateSequenceForPlay(spriteRenderer.transform).Play();
		}
	}

	private void OnEnable()
	{
	}

	private Sequence CreateSequenceForPlay(Transform target)
	{
		int num = Mathf.Min(UnityEngine.Random.Range(minNumberOfTweensPerSequence, maxNumberOfTweensPerSequence), 16);
		Sequence sequence = DOTween.Sequence();
		Span<TweenType> span = stackalloc TweenType[16].Slice(0, num);
		float num2 = endY - startY;
		_ = num2 / (float)num;
		Span<float> span2 = stackalloc float[16].Slice(0, num);
		Span<float> span3 = stackalloc float[16].Slice(0, num);
		float num3 = num2 / (float)num;
		for (int i = 0; i < span2.Length; i++)
		{
			if (i == 0)
			{
				span2[i] = startY + num3;
			}
			else
			{
				span2[i] = span2[i - 1] + num3;
			}
			span3[i] = UnityEngine.Random.Range(1f, 3f);
		}
		float num4 = 0f;
		float num5 = target.position.x;
		for (int j = 0; j < span.Length; j++)
		{
			if (j > 0)
			{
				num4 += span3[j - 1];
			}
			float num6 = UnityEngine.Random.Range(0f, 1f);
			if (num6 < chanceOfStretch && j < span.Length - 2)
			{
				float num7 = ((j > 0) ? span2[j - 1] : target.position.y);
				float num8 = span2[j];
				float num9 = Mathf.Abs(num7 - num8) + 1f;
				Debug.Log($"Target Scale = {num9}, movY1 = {num8}, movY2 = {span2[j + 1]}");
				sequence.Append(target.DOScaleY(num9, span3[j]));
				sequence.Insert(num4, target.DOMoveY(num8, span3[j]));
				j++;
				num4 += span3[j - 1];
				sequence.Append(target.DOScaleY(1f, span3[j]));
				sequence.Insert(num4, target.DOMoveY(span2[j], span3[j]));
			}
			else if (num6 < chanceOfXMovement && j < span.Length - 2)
			{
				float num10 = UnityEngine.Random.Range(minXMovement, maxXMovement);
				num10 = ((UnityEngine.Random.Range(0, 2) == 0) ? (num10 * -1f) : num10);
				sequence.Append(target.DOMoveX(num10 + num5, span3[j]).SetEase(Ease.Linear));
				num5 += num10;
				sequence.Insert(num4, target.DOMoveY(span2[j], span3[j]).SetEase(Ease.Linear));
			}
			else
			{
				sequence.Append(target.DOMoveY(span2[j], span3[j]).SetEase(Ease.Linear));
			}
		}
		return sequence;
	}
}
