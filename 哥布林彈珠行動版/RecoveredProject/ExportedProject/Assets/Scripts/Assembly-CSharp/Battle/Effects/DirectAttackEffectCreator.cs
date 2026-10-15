using System;
using Battle.Attacks;
using Battle.Pachinko.BallBehaviours.Coins;
using UnityEngine;

namespace Battle.Effects
{
	public class DirectAttackEffectCreator : MonoBehaviour
	{
		private ObjectPool _effectPool;

		private void Awake()
		{
			_effectPool = GetComponent<ObjectPool>();
		}

		private void OnEnable()
		{
			DealsDamageImmediatlyOnPegHit.OnDirectDamageDealt = (DealsDamageImmediatlyOnPegHit.DirectDamageDealt)Delegate.Combine(DealsDamageImmediatlyOnPegHit.OnDirectDamageDealt, new DealsDamageImmediatlyOnPegHit.DirectDamageDealt(OnDirectDamageDealt));
			CoinsContributeToDamage.OnCoinDirectDamageDealt = (CoinsContributeToDamage.CoinDirectDamageDealt)Delegate.Combine(CoinsContributeToDamage.OnCoinDirectDamageDealt, new CoinsContributeToDamage.CoinDirectDamageDealt(OnDirectDamageDealt));
		}

		private void OnDisable()
		{
			DealsDamageImmediatlyOnPegHit.OnDirectDamageDealt = (DealsDamageImmediatlyOnPegHit.DirectDamageDealt)Delegate.Remove(DealsDamageImmediatlyOnPegHit.OnDirectDamageDealt, new DealsDamageImmediatlyOnPegHit.DirectDamageDealt(OnDirectDamageDealt));
			CoinsContributeToDamage.OnCoinDirectDamageDealt = (CoinsContributeToDamage.CoinDirectDamageDealt)Delegate.Remove(CoinsContributeToDamage.OnCoinDirectDamageDealt, new CoinsContributeToDamage.CoinDirectDamageDealt(OnDirectDamageDealt));
		}

		private void OnDirectDamageDealt(Vector3 spawnPos, Vector3 scale, GameObject effectObject)
		{
			if (_effectPool.Prefab == null || _effectPool.Prefab != effectObject)
			{
				_effectPool.DestroyAllPooledObjects();
				_effectPool.CreatePool(_effectPool.poolAmount, effectObject);
			}
			GameObject pooledObject = _effectPool.GetPooledObject();
			if (pooledObject != null)
			{
				pooledObject.transform.position = spawnPos;
				pooledObject.transform.localScale = scale;
				float pitch = 1f;
				if (pooledObject.TryGetComponent<AudioSource>(out var component))
				{
					pitch = component.pitch;
					component.pitch += UnityEngine.Random.Range(-0.1f, 0.1f);
				}
				pooledObject.SetActive(value: true);
				if (component != null)
				{
					component.pitch = pitch;
				}
			}
		}
	}
}
