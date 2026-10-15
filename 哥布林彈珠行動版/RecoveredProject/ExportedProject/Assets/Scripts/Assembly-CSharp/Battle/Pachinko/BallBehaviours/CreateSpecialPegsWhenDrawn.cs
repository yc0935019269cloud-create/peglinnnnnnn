using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class CreateSpecialPegsWhenDrawn : MonoBehaviour
	{
		public Peg.PegType specialPegToCreate;

		public int numToCreate = 1;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("SPECIAL_PEGS", numToCreate.ToString());
			}
		}
	}
}
