using System;
using System.Collections.Generic;
using Battle.PegBehaviour;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using I2.Loc;
using UnityEngine;

public class ThunderOrbPachinko : MonoBehaviour
{
	public int numZaps = 1;

	public float zapDistance = 2f;

	public float zapFadeTime = 0.2f;

	public LineRenderer _line;

	private List<Peg> _hitPegs = new List<Peg>();

	private PachinkoBall _pachinkoBall;

	private void Awake()
	{
		_pachinkoBall = GetComponent<PachinkoBall>();
	}

	private void Start()
	{
		_hitPegs.Clear();
		_line.enabled = false;
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		if (_pachinkoBall.IsDummy)
		{
			return;
		}
		Peg component = other.collider.GetComponent<Peg>();
		if (component != null)
		{
			if ((component is LongPeg && (component as LongPeg).hit) || _hitPegs.Contains(component))
			{
				return;
			}
			_hitPegs.Add(component);
			Peg peg = component;
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < numZaps; i++)
			{
				Collider2D[] array = Physics2D.OverlapCircleAll(peg.GetCenterOfPeg(), zapDistance);
				float num = float.PositiveInfinity;
				Peg peg2 = null;
				Collider2D[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					if (!array2[j].TryGetComponent<Peg>(out var component2) || _hitPegs.Contains(component2) || (component2 is LongPeg && (component2 as LongPeg).hit))
					{
						continue;
					}
					PegGridObscurer componentInParent = component2.GetComponentInParent<PegGridObscurer>();
					if ((!(componentInParent != null) || componentInParent.isRevealed) && component2 != component && !component2.IsDisabled() && (!component2.IsDelayedDeath() || !component2.IsWaitingForDeath()))
					{
						float num2 = Vector2.Distance(component2.GetCenterOfPeg(), base.transform.position);
						if (num2 < num)
						{
							peg2 = component2;
							num = num2;
						}
					}
				}
				if (!(peg2 != null))
				{
					break;
				}
				peg2.PegActivated();
				list.Add(peg.GetCenterOfPeg());
				list.Add(peg2.GetCenterOfPeg());
				_hitPegs.Add(peg2);
				peg = peg2;
			}
			if (list.Count > 0)
			{
				_line.enabled = true;
				_line.positionCount = list.Count;
				_line.SetPositions(list.ToArray());
				_line.material.DOKill();
				TweenerCore<Color, Color, ColorOptions> tweenerCore = _line.material.DOFade(0f, zapFadeTime).From(1f);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(DisableRenderer));
			}
		}
		_hitPegs.Clear();
	}

	private void DisableRenderer()
	{
		_line.enabled = false;
	}

	private void OnValidate()
	{
		LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
		if (component != null)
		{
			component.SetParameterValue("NUM_ZAPS", numZaps.ToString());
		}
		else
		{
			Debug.Log(base.gameObject.name + " does not have loc param manager   ");
		}
	}
}
