using System;
using Battle.Enemies;
using UnityEngine;

namespace Battle.Attacks
{
	public class DealsDamageImmediatlyOnPegHit : Attack
	{
		public delegate void DirectDamageDealt(Vector3 spawnPos, Vector3 scale, GameObject effectObject);

		public static DirectDamageDealt OnDirectDamageDealt = (Vector3 spawnPos, Vector3 scale, GameObject effectObject) =>
		{
		};

		public GameObject attackEffectObject;

		public Vector3 spawnOffset;

		public float spawnRandomXMin = -0.2f;

		public float spawnRandomXMax = 0.2f;

		public float spawnRandomYMin = -0.8f;

		public float spawnRandomYMax = 0.8f;

		public bool flipYScaleForEveryOtherAttack = true;

		private PachinkoBall _pachinkoBall;

		private TargetingManager _targetingManager;

		private bool _isYScaleFlipped;

		private void Awake()
		{
			_pachinkoBall = GetComponent<PachinkoBall>();
			_targetingManager = UnityEngine.Object.FindObjectOfType<TargetingManager>();
		}

		public override void Fire(AttackManager attackManager, Enemy target, int pegMultiplierTally, int numPegsHit, float dmgMult, long dmgBonus, int critCount = 0)
		{
			_attackManager.AttackAnimationEnded();
		}

		public override long GetDamage(AttackManager attackManager, int pegMultipliersTally, float dmgMult, long bonus, int critCount = 0, bool displayNegative = false)
		{
			return 0L;
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
			if (!_pachinkoBall.IsDummy && _targetingManager.currentTarget != null)
			{
				OnDirectDamageDealt?.Invoke(_targetingManager.currentTarget.GetCenterPosition() + spawnOffset + new Vector3(UnityEngine.Random.Range(spawnRandomXMin, spawnRandomXMax), UnityEngine.Random.Range(spawnRandomYMin, spawnRandomYMax)), _isYScaleFlipped ? new Vector3(1f, -1f, 1f) : new Vector3(1f, 1f, 1f), attackEffectObject);
				if (flipYScaleForEveryOtherAttack)
				{
					_isYScaleFlipped = !_isYScaleFlipped;
				}
			}
		}
	}
}
