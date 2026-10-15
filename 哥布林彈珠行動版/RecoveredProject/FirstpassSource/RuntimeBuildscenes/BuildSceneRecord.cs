using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuntimeBuildscenes
{
	[Serializable]
	public class BuildSceneRecord
	{
		[SerializeField]
		private string name;

		[SerializeField]
		private int buildIndex;

		[SerializeField]
		private string path;

		public int BuildIndex => buildIndex;

		public string Path => path;

		public string Name => name;

		public bool IsLoaded => SceneManager.GetSceneByBuildIndex(buildIndex).isLoaded;

		public BuildSceneRecord(string path, int buildIndex)
		{
			this.path = path;
			name = path[(path.LastIndexOf('/') + 1)..path.LastIndexOf('.')];
			this.buildIndex = buildIndex;
		}

		public override string ToString()
		{
			return $"[{buildIndex}] \"{name}\" ({path})";
		}
	}
}
