using System;
using UnityEngine;

namespace Battle.Effects
{
	public class BonusSpecialPegEffectCreator : MonoBehaviour
	{
		private ObjectPool _effectPool;

		private void Awake()
		{
			_effectPool = GetComponent<ObjectPool>();
		}

		private void OnEnable()
		{
			Peg.OnPegTypeConverted = (Peg.PegTypeConverted)Delegate.Combine(Peg.OnPegTypeConverted, new Peg.PegTypeConverted(HandlePegTypeConverted));
		}

		private void OnDisable()
		{
			Peg.OnPegTypeConverted = (Peg.PegTypeConverted)Delegate.Remove(Peg.OnPegTypeConverted, new Peg.PegTypeConverted(HandlePegTypeConverted));
		}

		private void HandlePegTypeConverted(Peg.PegType pegType, Vector3 pos, bool isBombRigged = false)
		{
			GameObject pooledObject = _effectPool.GetPooledObject();
			if (pooledObject != null)
			{
				pooledObject.SetActive(value: true);
				pooledObject.GetComponent<BonusSpecialPegEffect>().PlayEffect(pegType, pos, isBombRigged);
			}
		}
	}
}
