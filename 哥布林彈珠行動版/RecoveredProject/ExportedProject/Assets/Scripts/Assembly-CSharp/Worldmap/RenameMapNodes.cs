using UnityEngine;
using UnityEngine.SceneManagement;

namespace Worldmap
{
	public class RenameMapNodes : MonoBehaviour
	{
		public void RenameNodes()
		{
			MapNode[] array = Object.FindObjectsOfType<MapNode>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.name = SceneManager.GetActiveScene().name + "-Node_" + (i + 1);
			}
		}
	}
}
