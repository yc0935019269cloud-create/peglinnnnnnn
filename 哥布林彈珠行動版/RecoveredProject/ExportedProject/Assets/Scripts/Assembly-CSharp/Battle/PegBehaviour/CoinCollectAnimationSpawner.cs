using System;
using DG.Tweening;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class CoinCollectAnimationSpawner : MonoBehaviour
	{
		private ObjectPool _animObjectPool;

		private void Awake()
		{
			_animObjectPool = GetComponent<ObjectPool>();
		}

		private void OnEnable()
		{
			PegCoinOverlay.OnCoinCollected = (PegCoinOverlay.CoinOverlayEvent)Delegate.Combine(PegCoinOverlay.OnCoinCollected, new PegCoinOverlay.CoinOverlayEvent(SpawnCoinCollectAnim));
		}

		private void OnDisable()
		{
			PegCoinOverlay.OnCoinCollected = (PegCoinOverlay.CoinOverlayEvent)Delegate.Remove(PegCoinOverlay.OnCoinCollected, new PegCoinOverlay.CoinOverlayEvent(SpawnCoinCollectAnim));
		}

		public void SpawnCoinCollectAnim(Vector3 pos)
		{
			GameObject pooledObject = _animObjectPool.GetPooledObject();
			if (pooledObject != null)
			{
				pooledObject.transform.DOKill();
				pooledObject.transform.position = pos;
				pooledObject.SetActive(value: true);
				Vector3 normalized = (new Vector3(14f, 10f) - pos).normalized;
				pooledObject.transform.DOJump(pos + normalized, 0.8f, 1, 0.65f);
			}
		}
	}
}
