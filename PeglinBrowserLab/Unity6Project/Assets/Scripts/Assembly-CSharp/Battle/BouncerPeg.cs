using System;
using System.Collections.Generic;
using Battle.Pachinko.BallBehaviours;
using Battle.PegBehaviour;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using MoreMountains.Feedbacks;
using Peglin.OdinSerializer.Utilities;
using Relics;
using TMPro;
using UnityEngine;

namespace Battle
{
	public class BouncerPeg : Peg, IDummyPeg
	{
		private const int MAX_HITS_PER_SHOT = 50;

		[SerializeField]
		public Sprite _regularSprite;

		[SerializeField]
		protected Sprite _bonusSprite;

		[Header("Bouncer Peg Properties")]
		private const float BOMB_RELIC_FLING_FORCE = 450f;

		private float BouncerForce = 250f;

		private Vector3 _initialScale = Vector3.one * 3f;

		public Vector3 _bounceScaleOffset = Vector3.one * 0.5f;

		public bool removeDuringNav = true;

		private RegularPegSlimeBehaviour _slimeBehaviour;

		private PhysicsMaterial2D _defaultPhysicsMat;

		[HideInInspector]
		public string id = string.Empty;

		private SpriteRenderer _renderer;

		protected MMFeedbacks _feedbacks;

		private Dictionary<GameObject, int> _numTimesHitThisShot = new Dictionary<GameObject, int>();

		private bool _isDummy;

		private EffectPeg _effectPeg;

		private bool _isEffectPeg;

		protected override void Awake()
		{
			base.Awake();
			_feedbacks = GetComponent<MMFeedbacks>();
			_renderer = GetComponentInChildren<SpriteRenderer>();
			_slimeBehaviour = GetComponentInChildren<RegularPegSlimeBehaviour>(includeInactive: true);
			_effectPeg = GetComponentInChildren<EffectPeg>();
			_isEffectPeg = _effectPeg != null;
			if (_collider != null)
			{
				_defaultPhysicsMat = _collider.sharedMaterial;
			}
		}

		protected override void OnEnable()
		{
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Combine(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(ResetNumTimesHitThisShot));
			BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Combine(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(ConvertToBonusPeg));
			BattleController.onCriticalHitDeactivated = (BattleController.CriticalHitActivated)Delegate.Combine(BattleController.onCriticalHitDeactivated, new BattleController.CriticalHitActivated(ConvertToRegularPeg));
			base.OnEnable();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Remove(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(ResetNumTimesHitThisShot));
			BattleController.onCriticalHitActivated = (BattleController.CriticalHitActivated)Delegate.Remove(BattleController.onCriticalHitActivated, new BattleController.CriticalHitActivated(ConvertToBonusPeg));
			BattleController.onCriticalHitDeactivated = (BattleController.CriticalHitActivated)Delegate.Remove(BattleController.onCriticalHitDeactivated, new BattleController.CriticalHitActivated(ConvertToRegularPeg));
		}

		protected override void Start()
		{
			base.Start();
			_initialScale = base.transform.localScale;
			if (slimeType != SlimeType.None)
			{
				_slimeBehaviour.gameObject.SetActive(value: true);
				SpriteRenderer component = _slimeBehaviour.GetComponent<SpriteRenderer>();
				if (component != null)
				{
					component.color = _slimeBehaviour.GetColorForSlimeType(slimeType);
				}
			}
		}

		private void Update()
		{
			base.transform.rotation = Quaternion.identity;
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			DoPegCollision(collision.collider.GetComponent<PachinkoBall>());
		}

		public override void PegActivated(bool playAudio = true, bool forcePop = false)
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (slimeType == SlimeType.HealSlime && !_slimedThisHit && !IsDisabled())
			{
				float num = 1f;
				if (buffAmount > 0)
				{
					num += Mathf.Floor(buffAmount / Peg.BUFF_PER_HEAL_INCREASE);
				}
				else if (buffAmount < 0)
				{
					num += Mathf.Floor(Mathf.Abs(buffAmount) / Peg.DEBUFF_PER_HEAL_INCREASE);
				}
				PlayerHealthController.OnExternalHealRequested?.Invoke(num);
				Peg.OnSlimedPegAudioRequest?.Invoke(slimeType);
			}
			else if (slimeType == SlimeType.LightningSlime && !_slimedThisHit && !IsDisabled())
			{
				StartCoroutine(ActivateLightningSlime());
			}
			Peg.OnPegHit(PegType.BOUNCER, this);
			Peg.OnPegActivated(PegType.BOUNCER, this);
			if (playAudio && !_isDummy)
			{
				Peg.OnPegAudioRequest?.Invoke(pegType);
			}
			CollectCoin();
			if (_feedbacks != null)
			{
				_feedbacks.PlayFeedbacks();
			}
			CheckForRelicBuff();
		}

		public override void Reset(bool criticalHitActive = false)
		{
			if (criticalHitActive)
			{
				_renderer.sprite = _bonusSprite;
			}
			else
			{
				_renderer.sprite = _regularSprite;
			}
		}

		public override GameObject ConvertPegToType(PegType type)
		{
			return base.gameObject;
		}

		public override void ApplySlimeToPeg(SlimeType sType)
		{
			base.ApplySlimeToPeg(sType);
			if (SupportsPegType(PegType.SLIME_ONLY, sType) && sType != slimeType)
			{
				if (relicManager.AttemptUseRelic(RelicEffect.SLIME_BUFFS_PEGS))
				{
					AddBuff(5);
				}
				if (slimeType == SlimeType.None && Peg.IsSlimeTypeBuffable(sType) && relicManager.AttemptUseRelic(RelicEffect.DEBUFF_SLIME_PEGS_HARDER))
				{
					AddBuff(-20);
				}
				_numBounces = 0;
				_slimeBehaviour.gameObject.SetActive(value: true);
				slimeType = sType;
				_slimeBehaviour.ApplySlime(sType, buffAmount);
				FlickeringPeg component = GetComponent<FlickeringPeg>();
				if (component != null)
				{
					component.StopFlicker();
					ResetRendererStatus();
				}
			}
		}

		public virtual void ConvertToBonusPeg()
		{
			if (_renderer != null && !IsDisabled())
			{
				_renderer.sprite = _bonusSprite;
			}
		}

		public virtual void ConvertToRegularPeg()
		{
			if (_renderer != null && !IsDisabled())
			{
				_renderer.sprite = _regularSprite;
			}
		}

		private void ResetRendererStatus()
		{
			if (_renderer != null && (GetComponent<FlickeringPeg>() == null || slimeType != SlimeType.None))
			{
				_renderer.color = new Color(1f, 1f, 1f, 1f);
			}
			base.transform.localScale = _initialScale;
		}

		public override void RemoveIfCleared()
		{
			if (removeDuringNav)
			{
				if (_collider != null)
				{
					_collider.enabled = false;
				}
				if (_specialPegCollider != null)
				{
					_specialPegCollider.enabled = false;
				}
				float num = 1f;
				TweenerCore<Color, Color, ColorOptions> tweenerCore = _renderer.DOFade(0f, num);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(Disable));
				if (_pegText != null && _pegText.enabled)
				{
					_pegText.DOFade(0f, num - 0.01f);
				}
				if ((bool)PegCoinOverlayInstance)
				{
					PegCoinOverlayInstance.EndBattleFadeOut(num);
				}
			}
		}

		protected override void HidePeg()
		{
		}

		public void Disable()
		{
			base.gameObject.SetActive(value: false);
		}

		public override bool IsDelayedDeath()
		{
			return false;
		}

		public override bool SupportsPegType(PegType pType, SlimeType sType = SlimeType.None)
		{
			if (pType == PegType.SLIME_ONLY)
			{
				return sType > slimeType;
			}
			return false;
		}

		protected override void CheckPegSlimeShineEnabled()
		{
			_slimeBehaviour.SetSlimeShine(buffAmount);
		}

		public override bool IsDisabled()
		{
			if (base.gameObject.activeInHierarchy && !(_collider == null))
			{
				return !_collider.enabled;
			}
			return true;
		}

		public override void DoPegCollision(PachinkoBall pachinko, CollisionType cType = CollisionType.COLLIDER)
		{
			if (pachinko == null)
			{
				return;
			}
			if (pachinko.IsDummy && cType == CollisionType.COLLIDER)
			{
				Vector2 vector = pachinko.transform.position - base.transform.position;
				if (relicManager.RelicEffectActive(RelicEffect.LEGACY_METEORITE))
				{
					pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector.normalized * 450f);
				}
				else if (pachinko.meteoriteEnabled && relicManager.GetRemainingCountdownForRelic(RelicEffect.BOMB_FORCE_ALWAYS) <= 1 && relicManager.RelicEffectActive(RelicEffect.BOMB_FORCE_ALWAYS))
				{
					pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector.normalized * 450f);
				}
				else if (slimeType == SlimeType.BouncySlime)
				{
					pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector.normalized * 450f);
				}
				else
				{
					pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector.normalized * BouncerForce);
				}
			}
			else
			{
				if (pachinko.IsDummy)
				{
					return;
				}
				CheckAndApplySlime(pachinko);
				_slimedThisHit = false;
				if (_lightningSlimeRenderer != null)
				{
					_lightningSlimeRenderer.chain = new List<Peg>();
				}
				if (cType == CollisionType.COLLIDER)
				{
					if (!_numTimesHitThisShot.ContainsKey(pachinko.gameObject))
					{
						_numTimesHitThisShot[pachinko.gameObject] = 1;
					}
					else if (_numTimesHitThisShot[pachinko.gameObject]++ > 50)
					{
						pachinko.StartDestroy();
						Debug.LogWarning($"{pachinko} destroyed after hitting {this} over {50} times.");
					}
				}
				if (cType == CollisionType.COLLIDER)
				{
					Peg.OnPegCollisionHit(pegType, this);
				}
				PegActivated();
				if (!pachinko.IsFiring())
				{
					return;
				}
				HandleMultihits(this, GetNumberOfMultihits(pachinko, cType, pegType), pegType);
				if (cType == CollisionType.COLLIDER)
				{
					Vector2 vector2 = pachinko.transform.position - base.transform.position;
					if (relicManager.AttemptUseRelic(RelicEffect.LEGACY_METEORITE))
					{
						pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector2.normalized * 450f);
					}
					else if (pachinko.meteoriteEnabled && relicManager.AttemptUseRelic(RelicEffect.BOMB_FORCE_ALWAYS))
					{
						pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector2.normalized * 450f);
					}
					else
					{
						Peg.OnPegAudioRequest?.Invoke(pegType);
						Sequence s = DOTween.Sequence();
						s.Append(_renderer.transform.DOScale(_initialScale + _bounceScaleOffset, 0.25f).From(_initialScale * 0.9f).SetEase(Ease.Linear));
						s.Append(_renderer.transform.DOScale(_initialScale, 0.15f).SetEase(Ease.OutElastic));
						pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector2.normalized * BouncerForce);
					}
				}
				if (!(_renderer == null) && cType == CollisionType.COLLIDER)
				{
					CheckForPachinkoBuff(pachinko);
				}
			}
		}

		private void CheckAndApplySlime(PachinkoBall pachinko)
		{
			_slimedThisHit = false;
			if (IsDisabled())
			{
				return;
			}
			PachinkoPegSlimer component = pachinko.GetComponent<PachinkoPegSlimer>();
			if (component != null && SupportsPegType(PegType.SLIME_ONLY, component.slimeType) && component.ShouldSlimeCurrentPeg(this))
			{
				_slimedThisHit = true;
				ApplySlimeToPeg(component.slimeType);
			}
			if (SupportsPegType(PegType.SLIME_ONLY, SlimeType.HealSlime) && relicManager.AttemptUseRelic(RelicEffect.APPLIES_HEALING_SLIME) && !_slimedThisHit)
			{
				_slimedThisHit = true;
				ApplySlimeToPeg(SlimeType.HealSlime);
			}
			if (slimeType < SlimeType.LightningSlime && relicManager.AttemptUseRelic(RelicEffect.RANDOM_SLIME_ON_PEGS_HIT) && !_slimedThisHit)
			{
				SlimeType randomPrimeSlimeType = RelicManager.GetRandomPrimeSlimeType();
				if (SupportsPegType(PegType.SLIME_ONLY, randomPrimeSlimeType))
				{
					_slimedThisHit = true;
					ApplySlimeToPeg(randomPrimeSlimeType);
				}
			}
		}

		public override void RemoveSlime(bool forceRemoval = false)
		{
			if (slimeType != SlimeType.None)
			{
				_slimeBehaviour.StartSlimeDestruction();
				if (!ShouldIgnoreSlimeOnPegPop(slimeType) | forceRemoval)
				{
					slimeType = SlimeType.None;
					_collider.sharedMaterial = _defaultPhysicsMat;
				}
			}
		}

		protected override void CheckPegTextEnabled()
		{
			TMP_Text tMP_Text = _pegText;
			int num;
			if (buffAmount != 0)
			{
				PegType pegType = base.pegType;
				if (pegType != PegType.DESTROYED && pegType != PegType.CRIT && pegType != PegType.RESET && pegType != PegType.BOMB && NumCoins() <= 0)
				{
					num = ((!_isEffectPeg || _effectPeg.pegActive) ? 1 : 0);
					goto IL_0048;
				}
			}
			num = 0;
			goto IL_0048;
			IL_0048:
			tMP_Text.enabled = (byte)num != 0;
		}

		public override float GetMagnetForce()
		{
			return 1.1f;
		}

		public override void ResetHitCount()
		{
		}

		private void ResetNumTimesHitThisShot(Vector2 aimVector)
		{
			_numTimesHitThisShot.Clear();
		}

		public void GenerateId(bool force = false)
		{
			if (id.IsNullOrWhitespace() | force)
			{
				id = Guid.NewGuid().ToString();
			}
		}

		public string GetId()
		{
			return id;
		}

		public void SetDummyStatus(bool status)
		{
			_isDummy = status;
		}

		public void SetPegStatus(bool status)
		{
			if (!status)
			{
				DisableRegularColliders();
			}
			else
			{
				SwitchToRegularColliders();
			}
		}

		protected void DisableRegularColliders()
		{
			if (!(base.gameObject == null))
			{
				if (_collider != null)
				{
					_collider.enabled = false;
				}
				if (_trigger != null)
				{
					_trigger.enabled = false;
				}
				if (_specialPegCollider != null)
				{
					_specialPegCollider.enabled = false;
				}
			}
		}

		protected void SwitchToRegularColliders()
		{
			if (_collider != null)
			{
				_collider.enabled = true;
				if (_trigger != null)
				{
					_trigger.enabled = true;
				}
				if (_poppedPegTrigger != null)
				{
					_poppedPegTrigger.enabled = false;
				}
				if (_poppedPegCollider != null)
				{
					_poppedPegCollider.enabled = false;
				}
				if (_specialPegCollider != null)
				{
					_specialPegCollider.enabled = false;
				}
			}
		}

		public bool GetPegStatus()
		{
			if (pegType == PegType.DESTROYED)
			{
				return false;
			}
			return !IsDisabled();
		}

		public void SetSpecialCollider(bool value)
		{
		}

		public PegType GetPegType()
		{
			return pegType;
		}

		public void SetPegType(PegType pegType)
		{
		}

		protected override void UpdateInvisibilityFromBlind(SpriteRenderer spriteRenderer = null)
		{
			base.UpdateInvisibilityFromBlind(_renderer);
		}
	}
}
