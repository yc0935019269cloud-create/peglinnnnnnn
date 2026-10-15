using Battle;
using Relics;
using UnityEngine;

public class PachinkoFireButton : MonoBehaviour
{
	public delegate void FireButtonPressed();

	public static FireButtonPressed OnFireButtonPressed = () =>
	{
	};

	[SerializeField]
	private RelicManager _relicManger;

	private bool _alreadyFired;

	public GameObject fireText;

	public GameObject endShotText;

	public void FireButtonDepressed()
	{
		if (_relicManger != null && !BattleController.PreBattlePause && (_relicManger.RelicEffectActive(RelicEffect.AIM_LIMITER) || _relicManger.RelicEffectActive(RelicEffect.AIM_LIMITER_MULTIBALL)))
		{
			_alreadyFired = true;
			OnFireButtonPressed();
		}
	}

	public void FireButton()
	{
		if (!BattleController.PreBattlePause)
		{
			if (_alreadyFired)
			{
				_alreadyFired = false;
			}
			else
			{
				OnFireButtonPressed();
			}
		}
	}

	public void SetToFireMode()
	{
		fireText.SetActive(value: true);
		endShotText.SetActive(value: false);
	}

	public void SetToEndShotMode()
	{
		fireText.SetActive(value: false);
		endShotText.SetActive(value: true);
	}
}
