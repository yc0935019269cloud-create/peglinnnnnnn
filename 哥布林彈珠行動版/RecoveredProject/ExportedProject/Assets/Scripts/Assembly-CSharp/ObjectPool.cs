using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
	public List<GameObject> PooledObjects = new List<GameObject>();

	public GameObject Prefab;

	public bool instanceOnStart = true;

	public int poolAmount = 10;

	private bool instantiated;

	private void Start()
	{
		if (!instantiated && instanceOnStart)
		{
			CreatePool(poolAmount, Prefab);
		}
	}

	public void CreatePool(int poolAmount, GameObject prefab)
	{
		if (PooledObjects.Count > 0)
		{
			instantiated = true;
			poolAmount = PooledObjects.Count;
			return;
		}
		this.poolAmount = poolAmount;
		Prefab = prefab;
		for (int i = 0; i < this.poolAmount; i++)
		{
			GameObject gameObject = Object.Instantiate(prefab, base.transform);
			gameObject.SetActive(value: false);
			PooledObjects.Add(gameObject);
		}
		instantiated = true;
	}

	public T GetPooledObject<T>(bool instanceMoreIfNeeded = false) where T : MonoBehaviour
	{
		foreach (GameObject pooledObject in PooledObjects)
		{
			if (!pooledObject.activeInHierarchy)
			{
				return pooledObject.GetComponent<T>();
			}
		}
		if (instanceMoreIfNeeded)
		{
			GameObject gameObject = Object.Instantiate(Prefab, base.transform);
			gameObject.SetActive(value: false);
			PooledObjects.Add(gameObject);
			return gameObject.GetComponent<T>();
		}
		return null;
	}

	public GameObject GetPooledObject(bool instanceMoreIfNeeded = false)
	{
		foreach (GameObject pooledObject in PooledObjects)
		{
			if (!pooledObject.activeInHierarchy)
			{
				return pooledObject;
			}
		}
		if (instanceMoreIfNeeded)
		{
			GameObject gameObject = Object.Instantiate(Prefab, base.transform);
			gameObject.SetActive(value: false);
			PooledObjects.Add(gameObject);
			return gameObject;
		}
		return null;
	}

	public void DestroyAllPooledObjects()
	{
		base.transform.MMDestroyAllChildren();
	}
}
