using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolerDict : MonoBehaviour
{
	private Dictionary<GameObject, GameObject> _pooledPrefabsDict = new Dictionary<GameObject, GameObject>();

	public GameObject InstantiateObject(GameObject prefab)
	{
		if (_pooledPrefabsDict.ContainsKey(prefab))
		{
			return _pooledPrefabsDict[prefab];
		}
		_pooledPrefabsDict.Add(prefab, Object.Instantiate(prefab));
		return _pooledPrefabsDict[prefab];
	}

	public void OnDisable()
	{
		_pooledPrefabsDict.Clear();
	}
}
