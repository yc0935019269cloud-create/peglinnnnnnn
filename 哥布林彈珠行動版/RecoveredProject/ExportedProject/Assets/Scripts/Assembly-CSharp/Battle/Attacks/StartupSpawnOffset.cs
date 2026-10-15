using UnityEngine;

namespace Battle.Attacks
{
	public class StartupSpawnOffset : MonoBehaviour
	{
		public Vector3 peglinPositionStartupOffset = new Vector3(1f, 0f, 0f);

		private void Awake()
		{
			GameObject gameObject = GameObject.FindWithTag("Player");
			base.gameObject.transform.position = gameObject.transform.position + peglinPositionStartupOffset;
		}
	}
}
