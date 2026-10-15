using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RuntimeBuildscenes
{
	public class ListSceneNamesInUiText : MonoBehaviour
	{
		private void Awake()
		{
			GetComponent<Text>().text = new string(BuildScenes.Records.SelectMany((BuildSceneRecord sr) => sr.ToString() + "\n").ToArray());
		}
	}
}
