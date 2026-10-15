using System;
using System.Collections;
using System.Diagnostics;
using Battle;
using Battle.Pachinko;
using Battle.Pachinko.BallBehaviours;
using Battle.PegBehaviour;
using Currency;
using Peglin.OdinSerializer.Utilities;
using Relics;
using UnityEngine;

public class Bomb : Peg, IDummyPeg
{
	[Header("Bomb Attributes")]
	[SerializeField]
	private PhysicsMaterial2D _explodeMaterial;

	[SerializeField]
	private AudioClip _bombDetonationSfx;

	[SerializeField]
	private string _animHitsKey = "NumHits";

	[SerializeField]
	private PegSfx _pegSfx;

	private float _bombFlingForce = 1100f;

	public bool isRigged;

	public RuntimeAnimatorController riggedAnim;

	public RuntimeAnimatorController regularAnim;

	public int HitCount;

	private PhysicsMaterial2D _untouchedMaterial;

	private Animator _animator;

	private AudioSource _audioSource;

	private bool _detonated;

	private bool _detonatedThisTurn;

	private bool _inited;

	private bool _started;

	[HideInInspector]
	public string id = string.Empty;

	private Stopwatch _stopwatch = new Stopwatch();

	public float bombFlingForce => _bombFlingForce;

	public bool detonated => _detonated;

	public bool detonatedThisTurn => _detonatedThisTurn;

	private new IEnumerator Start()
	{
		_started = true;
		base.Start();
		pegType = PegType.BOMB;
		yield return null;
		_collider = GetComponent<Collider2D>();
		_trigger = GetComponent<Collider2D>();
		_audioSource = GetComponent<AudioSource>();
		_untouchedMaterial = _collider.sharedMaterial;
		if (relicManager.RelicEffectActive(RelicEffect.BOMB_TO_DULL))
		{
			ConvertPegToType(PegType.DULL);
		}
		if (relicManager.RelicEffectActive(RelicEffect.ALL_BOMBS_RIGGED))
		{
			ConvertToRigged();
		}
		if (relicManager.RelicEffectActive(RelicEffect.BOMBS_ONE_HIT))
		{
			HitCount = 1;
			_animator.SetInteger(_animHitsKey, HitCount);
		}
		_inited = true;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_animator = GetComponent<Animator>();
		_animator.keepAnimatorStateOnDisable = true;
		RelicManager.OnRelicAdded = (RelicManager.RelicManagement)Delegate.Combine(RelicManager.OnRelicAdded, new RelicManager.RelicManagement(HandleRelicAddition));
		if (_started && !_inited)
		{
			StartCoroutine(Start());
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		RelicManager.OnRelicAdded = (RelicManager.RelicManagement)Delegate.Remove(RelicManager.OnRelicAdded, new RelicManager.RelicManagement(HandleRelicAddition));
	}

	private void Update()
	{
		base.transform.rotation = Quaternion.identity;
	}

	private void LateUpdate()
	{
		if (base.gameObject.activeSelf && _animator.GetCurrentAnimatorStateInfo(0).IsName("SetInvis") && _detonated)
		{
			base.transform.gameObject.SetActive(value: false);
		}
	}

	private void HandleRelicAddition(Relic relic)
	{
		if (relic.effect == RelicEffect.BOMBS_ONE_HIT && HitCount == 0)
		{
			HitCount = 1;
			_animator.SetInteger(_animHitsKey, HitCount);
		}
	}

	public bool ShouldExplode()
	{
		return HitCount > 1;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (_inited)
		{
			DoPegCollision(collision.collider.GetComponent<PachinkoBall>());
		}
	}

	public override void PegActivated(bool playAudio = true, bool forcePop = false)
	{
		if (!_inited)
		{
			return;
		}
		HitCount++;
		_animator.SetInteger(_animHitsKey, HitCount);
		Peg.OnPegHit(PegType.BOMB, this);
		if (HitCount > 1 && !_detonated)
		{
			relicManager.AttemptUseRelic(RelicEffect.BOMBS_ONE_HIT);
			_detonated = true;
			_detonatedThisTurn = true;
			if ((!BattleController.BattleActive || BattleController.CurrentBattleState == BattleController.BattleState.NAVIGATION) && relicManager.AttemptUseRelic(RelicEffect.BOMB_NAV_GOLD) && !relicManager.RelicEffectActive(RelicEffect.CONVERT_COIN_TO_DAMAGE))
			{
				int num = 5;
				if (relicManager.AttemptUseRelic(RelicEffect.INCREASED_NAV_GOLD))
				{
					num *= 4;
				}
				CurrencyManager.Instance.AddGold(num);
			}
			else if ((!BattleController.BattleActive || BattleController.CurrentBattleState == BattleController.BattleState.NAVIGATION) && relicManager.AttemptUseRelic(RelicEffect.BOMB_NAV_GOLD) && relicManager.RelicEffectActive(RelicEffect.CONVERT_COIN_TO_DAMAGE))
			{
				int num2 = 5;
				if (relicManager.AttemptUseRelic(RelicEffect.INCREASED_NAV_GOLD))
				{
					num2 *= 4;
				}
				relicManager.DoCoinsProvideHealingRelicsEffect(num2);
			}
			if (relicManager.AttemptUseRelic(RelicEffect.BOMB_SPLASH))
			{
				_audioSource.PlayOneShot(_bombDetonationSfx);
				Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, 1.8f);
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].TryGetComponent<Peg>(out var component))
					{
						PegGridObscurer componentInParent = component.GetComponentInParent<PegGridObscurer>();
						if (component != this && !component.IsDisabled() && (componentInParent == null || componentInParent.isRevealed))
						{
							component.PegActivated(playAudio: false);
						}
					}
				}
			}
			else
			{
				_audioSource.PlayOneShot(_pegSfx.BombExplosionSfx);
			}
			_collider.enabled = false;
			Peg.OnPegActivated(PegType.BOMB, this);
		}
		else if (HitCount == 1)
		{
			if (_audioSource != null && _pegSfx != null)
			{
				_audioSource.PlayOneShot(_pegSfx.BombBeforeExplosionSfx);
			}
			_collider.sharedMaterial = _explodeMaterial;
		}
	}

	public void CompletelyResetBomb()
	{
		Reset();
	}

	private void Reset()
	{
		_collider.enabled = true;
		_collider.sharedMaterial = _untouchedMaterial;
		HitCount = (relicManager.RelicEffectActive(RelicEffect.BOMBS_ONE_HIT) ? 1 : 0);
		if (isRigged && !relicManager.RelicEffectActive(RelicEffect.ALL_BOMBS_RIGGED))
		{
			ConvertFromRigged();
		}
		else
		{
			_animator.SetInteger(_animHitsKey, HitCount);
		}
		_detonated = false;
	}

	public void BombsThrown()
	{
		_detonatedThisTurn = false;
	}

	public override bool IsDelayedDeath()
	{
		return false;
	}

	public override GameObject ConvertPegToType(PegType type)
	{
		UnityEngine.Debug.LogWarning("Attempted to convert a bomb to " + type);
		return base.gameObject;
	}

	public override void Reset(bool criticalHitActive = false)
	{
		if (CanResetBomb())
		{
			base.gameObject.SetActive(value: true);
			Reset();
		}
	}

	public bool CanResetBomb()
	{
		if (_detonated)
		{
			return !_detonatedThisTurn;
		}
		return false;
	}

	public override bool SupportsPegType(PegType pType, SlimeType sType)
	{
		return false;
	}

	public override void RemoveIfCleared()
	{
		UnityEngine.Debug.LogWarning("Attempted to clear a bomb peg");
	}

	public override bool IsDisabled()
	{
		return !base.gameObject.activeInHierarchy;
	}

	private void ConvertToTrigger()
	{
		_collider.isTrigger = true;
	}

	private void ConvertToCollider()
	{
		_collider.isTrigger = false;
	}

	public override void DoPegCollision(PachinkoBall pachinko, CollisionType cType = CollisionType.COLLIDER)
	{
		_stopwatch.Restart();
		if (pachinko != null && !pachinko.IsDummy && pachinko.IsFiring())
		{
			if (HitCount == 0 && ((bool)pachinko.gameObject.GetComponent<DetonateBomb>() || (bool)pachinko.gameObject.GetComponent<Multihit>()))
			{
				HitCount = 1;
			}
			if (cType == CollisionType.COLLIDER)
			{
				Peg.OnPegCollisionHit(pegType, this);
			}
			PegActivated();
			if (ShouldExplode())
			{
				Vector2 vector = pachinko.transform.position - base.transform.position;
				pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector.normalized * _bombFlingForce);
			}
		}
		else if (pachinko != null && pachinko.IsDummy)
		{
			if (HitCount == 0 && ((bool)pachinko.gameObject.GetComponent<DetonateBomb>() || (bool)pachinko.gameObject.GetComponent<Multihit>()))
			{
				HitCount = 1;
			}
			if (HitCount + 1 > 1)
			{
				Vector2 vector2 = pachinko.transform.position - base.transform.position;
				pachinko.gameObject.GetComponent<Rigidbody2D>().AddForce(vector2.normalized * _bombFlingForce);
			}
		}
	}

	public override float GetMagnetForce()
	{
		return 1.1f;
	}

	public void ConvertToRigged()
	{
		isRigged = true;
		_animator.runtimeAnimatorController = riggedAnim;
		_animator.SetInteger(_animHitsKey, HitCount);
	}

	public void ConvertFromRigged()
	{
		isRigged = false;
		_animator.runtimeAnimatorController = regularAnim;
		_animator.SetInteger(_animHitsKey, HitCount);
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(base.transform.position, 1.8f);
	}

	public override void ResetHitCount()
	{
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
	}

	public void SetPegStatus(bool status)
	{
		base.gameObject.SetActive(status);
	}

	public bool GetPegStatus()
	{
		return base.gameObject.activeInHierarchy;
	}

	public void SetSpecialCollider(bool value)
	{
	}

	public void SetPegType(PegType pegType)
	{
		pegType = PegType.BOMB;
	}

	public PegType GetPegType()
	{
		return PegType.BOMB;
	}

	public void SetPhysicsMaterial(Bomb b)
	{
		_collider.sharedMaterial = b.GetActiveMaterial();
	}

	public PhysicsMaterial2D GetActiveMaterial()
	{
		return _collider.sharedMaterial;
	}

	private void OnDestroy()
	{
	}
}
