using System;
using System.Collections;
using Battle.Pachinko;
using Battle.Pachinko.BallBehaviours;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class SlimeOnlyPeg : RegularPeg
{
	[Header("Slime Only Component Refs")]
	[SerializeField]
	private SpriteRenderer _slimeRenderer;

	[SerializeField]
	private SpriteRenderer _slimeShineRenderer;

	[SerializeField]
	private Animator _slimeAnimator;

	[SerializeField]
	private Animator _slimeShineAnimator;

	[Header("Slime Only Properties")]
	[SerializeField]
	private float _pachinkoBallVelocityReduction = 0.65f;

	private static readonly int SlimeOffAnimKey = Animator.StringToHash("SlimeOff");

	private static readonly int CritActiveAnimKey = Animator.StringToHash("CritActive");

	private static readonly int ResetAnimKey = Animator.StringToHash("Reset");

	private static readonly int SlimeMinimalAnimHash = Animator.StringToHash("ShineMinimal");

	private static readonly int SlimeShineRegularHash = Animator.StringToHash("ShineRegular");

	private void OnTriggerEnter2D(Collider2D collider)
	{
		if (!_revivedPegThisFrame)
		{
			DoPegCollision(collider.gameObject.GetComponent<PachinkoBall>());
		}
	}

	public override void Reset(bool criticalHitActive = false)
	{
		if (pegType != PegType.DESTROYED)
		{
			pegType = PegType.SLIME_ONLY;
			SwitchToRegularColliders();
			if (_pegText != null)
			{
				_pegText.text = buffAmount.ToString();
				_pegText.enabled = buffAmount != 0;
			}
			if (!(_slimeRenderer == null))
			{
				base.transform.localScale = _initialVector;
				_slimeShineRenderer.enabled = true;
				_slimeAnimator.SetBool(CritActiveAnimKey, criticalHitActive);
				_slimeAnimator.SetTrigger(ResetAnimKey);
				SetSlimeShine(buffAmount);
			}
		}
	}

	public override void ConvertToBonusPeg()
	{
		if (_slimeRenderer != null && pegType == PegType.SLIME_ONLY && !IsDisabled())
		{
			_slimeAnimator.SetBool(CritActiveAnimKey, value: true);
		}
	}

	public override void ConvertToRegularPeg()
	{
		if (_slimeRenderer != null && pegType == PegType.SLIME_ONLY && !IsDisabled())
		{
			_slimeAnimator.SetBool(CritActiveAnimKey, value: false);
		}
	}

	public override GameObject ConvertPegToType(PegType type)
	{
		GameObject result = base.gameObject;
		if (!SupportsPegType(type))
		{
			return result;
		}
		if (type == PegType.BOMB)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_bombPrefab, base.transform);
			result = gameObject;
			pegType = PegType.DESTROYED;
			HidePeg();
			Peg.OnPegAdded?.Invoke(gameObject, this);
		}
		return result;
	}

	public override void RemoveIfCleared()
	{
		if (pegType != PegType.DESTROYED && !(_slimeRenderer == null) && _cleared && NumCoins() <= 0)
		{
			TweenerCore<Color, Color, ColorOptions> tweenerCore = _slimeRenderer.DOFade(0f, _endBattleFadeTime);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(base.Disable));
			_slimeShineRenderer.DOFade(0f, _endBattleFadeTime - 0.01f);
			if (_pegText != null && _pegText.enabled)
			{
				_pegText.DOFade(0f, _endBattleFadeTime - 0.01f);
			}
			if ((bool)PegCoinOverlayInstance)
			{
				PegCoinOverlayInstance.EndBattleFadeOut(_endBattleFadeTime);
			}
		}
	}

	private new void HidePeg()
	{
		if ((bool)PegCoinOverlayInstance)
		{
			PegCoinOverlayInstance.EndBattleFadeOut();
		}
		_collider.enabled = false;
		if (_slimeRenderer != null)
		{
			_slimeRenderer.DOKill();
			_slimeRenderer.enabled = false;
			if (_slimeShineRenderer != null)
			{
				_slimeShineRenderer.enabled = false;
			}
		}
		if (_pegText != null && _pegText.enabled)
		{
			_pegText.enabled = false;
		}
	}

	public override bool SupportsPegType(PegType pType, SlimeType sType = SlimeType.None)
	{
		switch (pType)
		{
		case PegType.REGULAR:
		case PegType.BOMB:
			return true;
		case PegType.CRIT:
		case PegType.RESET:
		case PegType.SLIME_ONLY:
		case PegType.GOLD:
		case PegType.VINE:
		case PegType.SPINFECTION:
			return false;
		default:
			return false;
		}
	}

	public override void DoPegCollision(PachinkoBall pachinko, CollisionType cType = CollisionType.COLLIDER)
	{
		if (pachinko == null)
		{
			return;
		}
		ApplySlimeVelocityReduction(pachinko, cType);
		DontPopSpecialPegsOnHit component = pachinko.GetComponent<DontPopSpecialPegsOnHit>();
		if (component != null && component.completelyIgnore && (component.dontPopTheseTypes.HasFlag(pegType) || (component.ignorePegsWithCoins && NumCoins() > 0)))
		{
			return;
		}
		if (IsDisabled() && (bool)pachinko.GetComponent<ResetPegOnCollision>())
		{
			_revivedPegThisFrame = true;
		}
		if (!pachinko.IsDummy)
		{
			if (cType == CollisionType.COLLIDER)
			{
				Peg.OnPegCollisionHit(pegType, this);
			}
			PegActivated();
			Multihit component2 = pachinko.gameObject.GetComponent<Multihit>();
			if (component2 != null)
			{
				Peg.OnPegMultihit(pegType, this, component2.multihitLevel);
			}
			if (!(_slimeRenderer == null) && cType == CollisionType.COLLIDER)
			{
				CheckForPachinkoBuff(pachinko);
			}
		}
	}

	public void ApplySlimeVelocityReduction(PachinkoBall pachinko, CollisionType cType = CollisionType.COLLIDER)
	{
		PierceBehavior component = pachinko.GetComponent<PierceBehavior>();
		if (cType == CollisionType.COLLIDER && (component == null || !component.CanPierce))
		{
			pachinko.GetComponent<Rigidbody2D>().linearVelocity *= _pachinkoBallVelocityReduction;
		}
	}

	public override bool ShouldEndPierce()
	{
		return false;
	}

	public override void PegActivated(bool playAudio = true, bool forcePop = false)
	{
		_cleared = true;
		Peg.OnPegActivated(pegType, this);
		if (playAudio)
		{
			Peg.OnPegAudioRequest?.Invoke(pegType);
		}
		if (_feedbacks != null)
		{
			_feedbacks.PlayFeedbacks();
		}
		if (!IsDisabled())
		{
			DisableRegularColliders();
			StartCoroutine(WaitAndEnablePoppedColliders());
			if (!_revivedPegThisFrame)
			{
				PlayDestructionAnimation();
			}
		}
		CollectCoins();
		CheckForRelicBuff();
	}

	private void PlayDestructionAnimation()
	{
		_slimeAnimator.SetTrigger(SlimeOffAnimKey);
		_slimeShineAnimator.SetTrigger(SlimeOffAnimKey);
	}

	private IEnumerator WaitAndEnablePoppedColliders()
	{
		yield return new WaitForSeconds(0.25f);
		EnablePoppedColliders();
	}

	public override bool AddCoin(bool doParticleEffects = true)
	{
		bool result = base.AddCoin(doParticleEffects);
		if ((bool)PegCoinOverlayInstance)
		{
			float a = ((PegCoinOverlayInstance.NumCoins > 1) ? 0.78f : 1f);
			PegCoinOverlayInstance.UpdateAlpha(a);
		}
		return result;
	}

	protected override void CheckPegSlimeShineEnabled()
	{
		SetSlimeShine(buffAmount);
	}

	private void SetSlimeShine(int pegBuffAmount)
	{
		if (IsDisabled())
		{
			_slimeShineAnimator.ResetTrigger(SlimeMinimalAnimHash);
			if (!_slimeShineAnimator.GetCurrentAnimatorStateInfo(0).IsName("Cleared") && !_slimeShineAnimator.GetCurrentAnimatorStateInfo(0).IsName("SlimeOff"))
			{
				_slimeShineAnimator.SetTrigger(ResetAnimKey);
				_slimeShineAnimator.SetTrigger(SlimeOffAnimKey);
			}
		}
		else if (pegBuffAmount == 0)
		{
			_slimeShineAnimator.ResetTrigger(SlimeMinimalAnimHash);
			_slimeShineAnimator.SetTrigger(ResetAnimKey);
		}
		else
		{
			_slimeShineAnimator.ResetTrigger(ResetAnimKey);
			_slimeShineAnimator.SetTrigger(SlimeMinimalAnimHash);
		}
	}
}
