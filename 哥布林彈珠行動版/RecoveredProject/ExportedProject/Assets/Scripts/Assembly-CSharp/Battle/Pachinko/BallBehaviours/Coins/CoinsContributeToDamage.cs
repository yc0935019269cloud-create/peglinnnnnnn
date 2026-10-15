using System;
using System.Collections;
using System.Globalization;
using Battle.Attacks;
using Battle.Enemies;
using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours.Coins
{
	public class CoinsContributeToDamage : MonoBehaviour
	{
		public delegate void CoinDirectDamageDealt(Vector3 spawnPos, Vector3 scale, GameObject effectObject);

		public static CoinDirectDamageDealt OnCoinDirectDamageDealt = (Vector3 spawnPos, Vector3 scale, GameObject effectObject) =>
		{
		};

		private PachinkoBall _pachinkoBall;

		private Attack _attack;

		public int coinDmgMod = 1;

		private TargetingManager _targetingManager;

		public GameObject attackEffectObject;

		public float attackMadeContactWaitTime = 0.2f;

		public Vector3 spawnOffset;

		public float spawnRandomXMin = -0.2f;

		public float spawnRandomXMax = 0.2f;

		public float spawnRandomYMin = -0.8f;

		public float spawnRandomYMax = 0.8f;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("COIN_DMG", coinDmgMod.ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				Debug.Log(base.gameObject.name + " does not have loc param manager.");
			}
		}

		private void Awake()
		{
			_pachinkoBall = GetComponent<PachinkoBall>();
			_attack = GetComponent<Attack>();
			_targetingManager = UnityEngine.Object.FindObjectOfType<TargetingManager>();
		}

		private void OnEnable()
		{
			PachinkoBall pachinkoBall = _pachinkoBall;
			pachinkoBall.OnPachinkoBallPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Combine(pachinkoBall.OnPachinkoBallPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
			PachinkoBall pachinkoBall2 = _pachinkoBall;
			pachinkoBall2.OnPachinkoBallSlimeOnlyPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Combine(pachinkoBall2.OnPachinkoBallSlimeOnlyPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
		}

		private void OnDisable()
		{
			PachinkoBall pachinkoBall = _pachinkoBall;
			pachinkoBall.OnPachinkoBallPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Remove(pachinkoBall.OnPachinkoBallPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
			PachinkoBall pachinkoBall2 = _pachinkoBall;
			pachinkoBall2.OnPachinkoBallSlimeOnlyPegHit = (PachinkoBall.PachinkoBallPegHit)Delegate.Remove(pachinkoBall2.OnPachinkoBallSlimeOnlyPegHit, new PachinkoBall.PachinkoBallPegHit(PegHit));
		}

		private void PegHit(Peg peg, PachinkoBall pBall)
		{
			if (!_pachinkoBall.IsDummy)
			{
				int num = peg.NumCoins();
				int numCoins = (peg.ShouldPopPegOnHit() ? num : Mathf.Min(1, num));
				StartCoroutine(AddDamageFromCollectedCoins(numCoins, peg.GetCenterOfPeg()));
			}
		}

		private IEnumerator AddDamageFromCollectedCoins(int numCoins, Vector3 pegPos)
		{
			WaitForSeconds waitForSecons = new WaitForSeconds(0.25f);
			for (int i = 0; i < numCoins; i++)
			{
				if (_targetingManager.currentTarget != null)
				{
					OnCoinDirectDamageDealt?.Invoke(_targetingManager.currentTarget.GetCenterPosition() + spawnOffset + new Vector3(UnityEngine.Random.Range(spawnRandomXMin, spawnRandomXMax), UnityEngine.Random.Range(spawnRandomYMin, spawnRandomYMax)), new Vector3(1f, 1f, 1f), attackEffectObject);
				}
				StartCoroutine(WaitAndDealDirectDamage(attackMadeContactWaitTime, _attack.GetModifiedDamagePerPeg(BattleController.criticalHitCount, calculateStaticDamageModificersInPlace: false) * coinDmgMod));
				yield return waitForSecons;
			}
		}

		private IEnumerator WaitAndDealDirectDamage(float waitTime, int damage)
		{
			yield return new WaitForSeconds(waitTime);
			TargetingManager.OnDamageTargetedEnemy?.Invoke(damage, Enemy.EnemyDamageSource.RelicChip);
		}
	}
}
