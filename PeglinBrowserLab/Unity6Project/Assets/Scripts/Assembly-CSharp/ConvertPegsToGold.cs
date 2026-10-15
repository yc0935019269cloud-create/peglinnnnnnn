using System;
using I2.Loc;
using UnityEngine;

public class ConvertPegsToGold : MonoBehaviour
{
	private PachinkoBall _pachinkoBall;

	public GameObject goldPegPrefab;

	[Header("Conversion Params")]
	public bool convertsFirstPeg;

	public int hitsUntilConversion = 20;

	public int extraCoinsOnConversion = 1;

	private int _currentHits;

	private bool _hasHitPeg;

	[HideInInspector]
	public Peg convertPeg;

	private void OnValidate()
	{
		LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
		if (component != null)
		{
			component.SetParameterValue("GOLD_PEG_CONVERT_COUNT", hitsUntilConversion.ToString());
			component.SetParameterValue("EXTRA_COINS_ON_CONVERT", extraCoinsOnConversion.ToString());
		}
		else
		{
			Debug.Log(base.gameObject.name + " does not have loc param manager   ");
		}
	}

	private void Awake()
	{
		_pachinkoBall = GetComponent<PachinkoBall>();
	}

	private void OnEnable()
	{
		PachinkoBall pachinkoBall = _pachinkoBall;
		pachinkoBall.OnPachinkoBallPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Combine(pachinkoBall.OnPachinkoBallPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
	}

	private void OnDisable()
	{
		PachinkoBall pachinkoBall = _pachinkoBall;
		pachinkoBall.OnPachinkoBallPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Remove(pachinkoBall.OnPachinkoBallPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
	}

	private void PegHit(Peg peg, PachinkoBall pBall)
	{
		if (!_pachinkoBall.IsDummy && peg.SupportsPegType(Peg.PegType.GOLD))
		{
			if (convertsFirstPeg && !_hasHitPeg)
			{
				convertPeg = peg;
				_hasHitPeg = true;
			}
			else if (++_currentHits >= hitsUntilConversion)
			{
				convertPeg = peg;
				_currentHits = 0;
			}
		}
	}
}
