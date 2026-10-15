using Cruciball;
using Saving;
using UnityEngine;

namespace PeglinUI.RunSummary
{
	public class ClassUnlockAlert : MonoBehaviour
	{
		public GameObject[] silhouettes;

		public CruciballManager cruciballManager;

		public void SetClass()
		{
			if (silhouettes == null)
			{
				Debug.LogError("Missing Silhouettes in ClassUnlockAlert. Bailing.");
			}
			else if (silhouettes != null && silhouettes.Length <= (int)cruciballManager.currentClass)
			{
				Debug.LogError($"Missing Silhouette for {cruciballManager.currentClass}. Bailing.");
			}
			else if (PersistentPlayerData.Instance.QueuedClassUnlocks.Count > 0)
			{
				GameObject[] array = silhouettes;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: false);
				}
				silhouettes[(int)PersistentPlayerData.Instance.QueuedClassUnlocks[0]].SetActive(value: true);
			}
		}
	}
}
