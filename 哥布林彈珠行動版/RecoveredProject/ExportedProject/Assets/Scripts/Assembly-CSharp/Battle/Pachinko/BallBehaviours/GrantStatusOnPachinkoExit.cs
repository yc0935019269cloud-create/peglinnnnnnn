using System.Globalization;
using Battle.StatusEffects;
using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class GrantStatusOnPachinkoExit : MonoBehaviour
	{
		public StatusEffect statusToGrant;

		private void OnValidate()
		{
			if (statusToGrant != null)
			{
				GetComponent<LocalizationParamsManager>()?.SetParameterValue("STATUS_ON_EXIT", statusToGrant.Intensity.ToString(CultureInfo.InvariantCulture));
			}
		}
	}
}
