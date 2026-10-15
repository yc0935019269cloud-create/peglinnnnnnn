using Battle.StatusEffects;
using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class ApplyStatusEffectsToEnemiesWhenDrawn : MonoBehaviour
	{
		public StatusEffect effectToApply;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null && effectToApply != null)
			{
				component.SetParameterValue("AMOUNT_ON_DRAWN", effectToApply.Intensity.ToString());
			}
		}
	}
}
