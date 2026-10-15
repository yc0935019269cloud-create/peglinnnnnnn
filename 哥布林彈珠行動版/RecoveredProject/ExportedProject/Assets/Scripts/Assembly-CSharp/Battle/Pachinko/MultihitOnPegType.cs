using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko
{
	public class MultihitOnPegType : MonoBehaviour
	{
		public int multihitLevel = 1;

		public Peg.PegType multihitPegType;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("MULTIHIT_PEG_TYPE_LEVEL", (multihitLevel + 1).ToString());
			}
			else
			{
				Debug.Log(base.gameObject.name + " does not have loc param manager   ");
			}
		}
	}
}
