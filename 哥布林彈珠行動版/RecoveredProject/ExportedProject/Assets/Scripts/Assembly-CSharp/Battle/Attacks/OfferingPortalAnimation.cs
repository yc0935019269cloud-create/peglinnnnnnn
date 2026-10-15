using UnityEngine;

namespace Battle.Attacks
{
	public class OfferingPortalAnimation : MonoBehaviour
	{
		public GameObject offeringPortalPrefab;

		private void OnEnable()
		{
			Object.Instantiate(offeringPortalPrefab);
		}
	}
}
