using System;
using Battle.Pachinko.BallBehaviours;
using UnityEngine;

namespace Battle.Effects
{
	public class SpinDoctorbPegEffectCreator : MonoBehaviour
	{
		private ObjectPool _effectPool;

		private void Awake()
		{
			_effectPool = GetComponent<ObjectPool>();
		}

		private void OnEnable()
		{
			FlipDowngradesAndHeal.OnSpinDoctorbFlippedPeg = (FlipDowngradesAndHeal.SpinDoctorbFlippedPeg)Delegate.Combine(FlipDowngradesAndHeal.OnSpinDoctorbFlippedPeg, new FlipDowngradesAndHeal.SpinDoctorbFlippedPeg(OnSpinDoctorbFlippedPeg));
		}

		private void OnDisable()
		{
			FlipDowngradesAndHeal.OnSpinDoctorbFlippedPeg = (FlipDowngradesAndHeal.SpinDoctorbFlippedPeg)Delegate.Remove(FlipDowngradesAndHeal.OnSpinDoctorbFlippedPeg, new FlipDowngradesAndHeal.SpinDoctorbFlippedPeg(OnSpinDoctorbFlippedPeg));
		}

		private void OnSpinDoctorbFlippedPeg(Peg peg, bool isFlippingFromNegativeToPositive)
		{
			if (!(peg != null))
			{
				return;
			}
			GameObject pooledObject = _effectPool.GetPooledObject();
			if (pooledObject != null)
			{
				pooledObject.SetActive(value: true);
				SpinDoctorbPegEffect component = pooledObject.GetComponent<SpinDoctorbPegEffect>();
				if (component != null)
				{
					component.PlayEffect(peg.GetCenterOfPeg(), isFlippingFromNegativeToPositive);
				}
			}
		}
	}
}
