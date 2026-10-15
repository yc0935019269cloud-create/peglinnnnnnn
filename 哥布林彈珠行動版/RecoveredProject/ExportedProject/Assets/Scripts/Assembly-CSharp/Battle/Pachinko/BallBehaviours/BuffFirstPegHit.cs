using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class BuffFirstPegHit : MonoBehaviour
	{
		private PachinkoBall _pachinkoBall;

		public int buffAmmount = 10;

		[HideInInspector]
		public bool hasBuffed;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("FIRST_PEG_HIT_BUFF_AMOUNT", buffAmmount.ToString());
			}
			else
			{
				Debug.Log(base.gameObject.name + " does not have loc param manager   ");
			}
		}
	}
}
