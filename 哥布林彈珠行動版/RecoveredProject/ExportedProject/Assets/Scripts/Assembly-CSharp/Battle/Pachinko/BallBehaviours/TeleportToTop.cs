using I2.Loc;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class TeleportToTop : MonoBehaviour
	{
		public int teleportLimit = 1;

		private int _teleportCount;

		public bool CanTeleport()
		{
			if (_teleportCount < teleportLimit)
			{
				_teleportCount++;
				return true;
			}
			return false;
		}

		private void SetLocParams()
		{
			GetComponent<LocalizationParamsManager>().SetParameterValue("PORTAL_COUNT", teleportLimit.ToString());
		}
	}
}
