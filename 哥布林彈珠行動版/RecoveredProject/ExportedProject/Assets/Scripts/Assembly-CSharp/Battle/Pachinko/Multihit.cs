using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko
{
	public class Multihit : MonoBehaviour
	{
		public int multihitLevel = 2;

		private void OnValidate()
		{
			if (TryGetComponent<LocalizationParamsManager>(out var component))
			{
				component.SetParameterValue(LocParamKeys.MULTIHIT_LEVEL, (multihitLevel + 1).ToString());
			}
		}
	}
}
