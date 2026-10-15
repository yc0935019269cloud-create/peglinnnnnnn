using UnityEngine;

namespace Saving
{
	public class DeleteSaveOnStart : MonoBehaviour
	{
		private void Start()
		{
			SaveManager.RequestDelete();
		}
	}
}
