using System;
using System.Collections.Generic;
using Battle.PegBehaviour;
using Peglin.OdinSerializer.Utilities;
using UnityEngine;

namespace Battle
{
	public class EffectPeg : MonoBehaviour, IDummyPeg
	{
		public delegate void EffectPegCreated(EffectPeg effectPeg);

		public delegate void EffectPegToggled(bool enabled);

		public static EffectPegCreated OnEffectPegCreated = delegate
		{
		};

		public EffectPegToggled OnEffectPegToggled = delegate
		{
		};

		public string effectName = "";

		[SerializeField]
		private Sprite fire_sprite;

		[SerializeField]
		private Sprite black_hole_sprite;

		[SerializeField]
		private Sprite lifelink_sprite;

		[SerializeField]
		private Sprite resistance_sprite;

		[SerializeField]
		private Sprite max_hp_attacks_sprite;

		[SerializeField]
		private Sprite muscircle_sprite;

		public SpriteRenderer crackingSpriteRenderer;

		public SpriteRenderer respawningIndicatorRenderer;

		public BouncerPeg bouncerPeg;

		public List<Sprite> crackingAnimationFrames;

		public List<Sprite> respawningAnimationFrames;

		public GameObject slimeOverlay;

		public int turnsToRecharge = 3;

		private bool _effectActive = true;

		private int _numTimesHit;

		private int _rechargeCount;

		private SpriteRenderer _runeSpriteRenderer;

		private bool _isDummy;

		[HideInInspector]
		public string id = string.Empty;

		public bool pegActive => _effectActive;

		private void Start()
		{
			if (!_isDummy)
			{
				_runeSpriteRenderer = GetComponent<SpriteRenderer>();
				OnEffectPegCreated?.Invoke(this);
				crackingSpriteRenderer.sortingOrder = _runeSpriteRenderer.sortingOrder + 1;
			}
		}

		private void OnEnable()
		{
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegActivated, new Peg.PegHitEvent(OnPegActivated));
			BattleController.OnTurnComplete = (Action)Delegate.Combine(BattleController.OnTurnComplete, new Action(OnTurnComplete));
		}

		private void OnDisable()
		{
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegActivated, new Peg.PegHitEvent(OnPegActivated));
			BattleController.OnTurnComplete = (Action)Delegate.Remove(BattleController.OnTurnComplete, new Action(OnTurnComplete));
		}

		private void OnTurnComplete()
		{
			if (_effectActive)
			{
				return;
			}
			_rechargeCount++;
			if (_rechargeCount >= turnsToRecharge)
			{
				respawningIndicatorRenderer.enabled = false;
				_numTimesHit = 0;
				_rechargeCount = 0;
				_effectActive = true;
				crackingSpriteRenderer.sprite = crackingAnimationFrames[_numTimesHit];
				bouncerPeg.SetPegStatus(status: true);
				bouncerPeg.gameObject.SetActive(value: true);
				_runeSpriteRenderer.enabled = true;
				crackingSpriteRenderer.enabled = true;
				if (bouncerPeg.buffAmount != 0)
				{
					bouncerPeg.pegText.enabled = true;
				}
				if (bouncerPeg.slimeType != Peg.SlimeType.None)
				{
					slimeOverlay.SetActive(value: true);
				}
				OnEffectPegToggled?.Invoke(enabled: true);
			}
			UpdateRespawningAnimation();
		}

		private void OnPegActivated(Peg.PegType pegType, Peg peg)
		{
			if (!(peg == bouncerPeg))
			{
				return;
			}
			_numTimesHit++;
			if (_numTimesHit >= crackingAnimationFrames.Count)
			{
				_numTimesHit = 0;
				_rechargeCount = 0;
				_effectActive = false;
				bouncerPeg.SetPegStatus(status: false);
				_runeSpriteRenderer.enabled = false;
				crackingSpriteRenderer.enabled = false;
				bouncerPeg.pegText.enabled = false;
				if (bouncerPeg.slimeType != Peg.SlimeType.None)
				{
					slimeOverlay.SetActive(value: false);
				}
				OnEffectPegToggled?.Invoke(enabled: false);
				UpdateRespawningAnimation();
			}
			else
			{
				crackingSpriteRenderer.sprite = crackingAnimationFrames[_numTimesHit];
			}
		}

		private void UpdateRespawningAnimation()
		{
			if (_effectActive)
			{
				respawningIndicatorRenderer.enabled = false;
				return;
			}
			respawningIndicatorRenderer.enabled = true;
			int num = respawningAnimationFrames.Count - turnsToRecharge;
			respawningIndicatorRenderer.sprite = respawningAnimationFrames[Mathf.Max(0, _rechargeCount + num)];
		}

		public Sprite GetSpriteForEffectName(string effectString)
		{
			return effectString switch
			{
				"add_fire" => fire_sprite, 
				"black_hole" => black_hole_sprite, 
				"lifelink" => lifelink_sprite, 
				"resistance" => resistance_sprite, 
				"max_hp_attacks" => max_hp_attacks_sprite, 
				"muscircle" => muscircle_sprite, 
				_ => null, 
			};
		}

		private void OnValidate()
		{
			GetComponent<SpriteRenderer>().sprite = GetSpriteForEffectName(effectName);
		}

		public void SetDummyStatus(bool status)
		{
			_isDummy = status;
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

		public bool IsSpecialPeg()
		{
			return false;
		}

		Peg.PegType IDummyPeg.GetPegType()
		{
			return Peg.PegType.BOUNCER;
		}

		public void SetPegType(Peg.PegType pegType)
		{
		}

		public void SetSpecialCollider(bool value)
		{
		}

		public void SetPegStatus(bool status)
		{
		}

		public bool GetPegStatus()
		{
			if (bouncerPeg.pegType == Peg.PegType.DESTROYED)
			{
				return false;
			}
			return !bouncerPeg.IsDisabled();
		}
	}
}
