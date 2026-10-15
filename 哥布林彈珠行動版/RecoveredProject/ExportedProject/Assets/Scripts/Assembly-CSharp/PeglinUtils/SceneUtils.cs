using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeglinUtils
{
	public static class SceneUtils
	{
		public static List<T> Find<T>()
		{
			List<T> list = new List<T>();
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				T[] componentsInChildren = rootGameObjects[i].GetComponentsInChildren<T>(includeInactive: true);
				list.AddRange(componentsInChildren);
			}
			return list;
		}
	}
}
