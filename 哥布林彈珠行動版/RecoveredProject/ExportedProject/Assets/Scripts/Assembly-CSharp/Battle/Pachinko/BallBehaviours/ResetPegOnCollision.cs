using I2.Loc;
using Relics;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class ResetPegOnCollision : MonoBehaviour
	{
		private PachinkoBall _pachinkoBall;

		public RelicManager relicManager;

		public int buffAmount;

		private void OnEnable()
		{
			_pachinkoBall = GetComponent<PachinkoBall>();
		}

		private void OnValidate()
		{
			GetComponent<LocalizationParamsManager>()?.SetParameterValue(LocParamKeys.BUFF_AMOUNT, buffAmount.ToString());
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (_pachinkoBall.IsDummy)
			{
				return;
			}
			Peg componentInParent = collision.collider.GetComponentInParent<Peg>();
			if (componentInParent != null && componentInParent.pegType != Peg.PegType.BOMB)
			{
				componentInParent.DoPegCollision(GetComponent<PachinkoBall>());
				if (!(componentInParent is LongPeg))
				{
					componentInParent.Reset(BattleController.criticalActive);
				}
				int num = buffAmount + (relicManager.AttemptUseRelic(RelicEffect.REFRESH_UPGRADES_PEGS) ? 1 : 0);
				if (num != 0)
				{
					componentInParent.AddBuff(num);
				}
			}
		}

		private void OnTriggerEnter2D(Collider2D collider)
		{
			SlimeOnlyPeg componentInParent = collider.GetComponentInParent<SlimeOnlyPeg>();
			if (_pachinkoBall.IsDummy)
			{
				if (componentInParent != null)
				{
					componentInParent.ApplySlimeVelocityReduction(_pachinkoBall);
				}
			}
			else if (componentInParent != null)
			{
				componentInParent.DoPegCollision(_pachinkoBall);
				componentInParent.Reset(BattleController.criticalActive);
				if (buffAmount > 0)
				{
					componentInParent.AddBuff(buffAmount);
				}
			}
		}
	}
}
