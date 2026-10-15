using System.Linq;
using RuntimeBuildscenes.BuildSceneAssets;
using UnityEngine;

namespace RuntimeBuildscenes
{
	public static class BuildScenes
	{
		private static BuildScenesAsset s_instance;

		public static BuildSceneRecord[] Records => Instance.BuildSceneRecords;

		private static BuildScenesAsset Instance
		{
			get
			{
				if (s_instance == null)
				{
					LoadBuildScenesAsset();
				}
				return s_instance;
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void LoadBuildScenesAsset()
		{
			s_instance = Resources.Load<BuildScenesAsset>(typeof(BuildScenesAsset).Name);
			if (s_instance == null)
			{
				Debug.LogWarning("Asset named 'BuildScenes' not found, trying brute force...");
				Resources.LoadAll<BuildScenesAsset>(string.Empty);
				s_instance = Resources.FindObjectsOfTypeAll<BuildScenesAsset>().FirstOrDefault();
			}
		}
	}
}
