using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers
{
	[AddComponentMenu("")]
	public class SpawnedObjectManager : Saver
	{
		[Serializable]
		public class SpawnedObjectData
		{
			public string prefabName;

			public Vector3 position;

			public Quaternion rotation;

			public string guid;

			public SpawnedObjectData()
			{
			}

			public SpawnedObjectData(string prefabName, Vector3 position, Quaternion rotation, string guid = null)
			{
				this.prefabName = prefabName;
				this.position = position;
				this.rotation = rotation;
				this.guid = guid;
			}
		}

		[Serializable]
		public class SpawnedObjectDataList
		{
			public List<SpawnedObjectData> list = new List<SpawnedObjectData>();
		}

		[Tooltip("Prefabs for all spawnable objects. If your spawnable object isn't in this list, Spawned Object Manager won't be able to respawn it when restoring a scene.")]
		[SerializeField]
		private List<SpawnedObject> m_spawnedObjectPrefabs = new List<SpawnedObject>();

		[Tooltip("Objects that have currently been spawned.")]
		[SerializeField]
		private List<SpawnedObject> m_spawnedObjects = new List<SpawnedObject>();

		[Tooltip("When restoring this Spawned Object Manager, tell respawned objects to restore their saved data also.")]
		[SerializeField]
		private bool m_applySaveDataToSpawnedObjectsOnRestore;

		private static SpawnedObjectManager m_instance;

		public List<SpawnedObject> spawnedObjectPrefabs
		{
			get
			{
				return m_spawnedObjectPrefabs;
			}
			set
			{
				m_spawnedObjectPrefabs = value;
			}
		}

		public List<SpawnedObject> spawnedObjects
		{
			get
			{
				return m_spawnedObjects;
			}
			set
			{
				m_spawnedObjects = value;
			}
		}

		public bool applySaveDataToSpawnedObjectsOnRestore
		{
			get
			{
				return m_applySaveDataToSpawnedObjectsOnRestore;
			}
			set
			{
				m_applySaveDataToSpawnedObjectsOnRestore = value;
			}
		}

		public override string key
		{
			get
			{
				string text = base.key;
				if (!string.Equals(text, base.name))
				{
					return text;
				}
				return base.name + " Scene " + SaveSystem.currentSceneIndex;
			}
			set
			{
				base.key = value;
			}
		}

		public override void Reset()
		{
			base.Reset();
			saveAcrossSceneChanges = true;
		}

		public override void Awake()
		{
			base.Awake();
			m_instance = this;
		}

		public override string RecordData()
		{
			SpawnedObjectDataList spawnedObjectDataList = new SpawnedObjectDataList();
			for (int i = 0; i < m_spawnedObjects.Count; i++)
			{
				SpawnedObject spawnedObject = m_spawnedObjects[i];
				if (!(spawnedObject == null))
				{
					spawnedObjectDataList.list.Add(new SpawnedObjectData(spawnedObject.name.Replace("(Clone)", string.Empty), spawnedObject.transform.position, spawnedObject.transform.rotation, spawnedObject.guid));
				}
			}
			return SaveSystem.Serialize(spawnedObjectDataList);
		}

		public override void ApplyData(string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				return;
			}
			SpawnedObjectDataList spawnedObjectDataList = SaveSystem.Deserialize<SpawnedObjectDataList>(data);
			if (spawnedObjectDataList == null || spawnedObjectDataList.list == null)
			{
				return;
			}
			m_spawnedObjects.Clear();
			for (int i = 0; i < spawnedObjectDataList.list.Count; i++)
			{
				SpawnedObjectData spawnedObjectData = spawnedObjectDataList.list[i];
				if (spawnedObjectData != null)
				{
					SpawnedObject spawnedObjectPrefab = GetSpawnedObjectPrefab(spawnedObjectData.prefabName);
					if (!(spawnedObjectPrefab == null))
					{
						UnityEngine.Object.Instantiate(spawnedObjectPrefab, spawnedObjectData.position, spawnedObjectData.rotation).guid = spawnedObjectData.guid;
					}
				}
			}
			if (m_applySaveDataToSpawnedObjectsOnRestore)
			{
				if (SaveSystem.framesToWaitBeforeApplyData == 0)
				{
					ApplyDataToRespawnedObjects();
				}
				else
				{
					StartCoroutine(ApplyDataToRespawnedObjectsAfterFrames(SaveSystem.framesToWaitBeforeApplyData));
				}
			}
		}

		protected virtual void ApplyDataToRespawnedObjects()
		{
			for (int i = 0; i < m_spawnedObjects.Count; i++)
			{
				Saver[] componentsInChildren = m_spawnedObjects[i].GetComponentsInChildren<Saver>();
				foreach (Saver saver in componentsInChildren)
				{
					saver.ApplyData(SaveSystem.currentSavedGameData.GetData(saver.key));
				}
			}
		}

		protected IEnumerator ApplyDataToRespawnedObjectsAfterFrames(int numFrames)
		{
			for (int i = 0; i < numFrames; i++)
			{
				yield return null;
			}
			ApplyDataToRespawnedObjects();
		}

		protected virtual SpawnedObject GetSpawnedObjectPrefab(string prefabName)
		{
			return m_spawnedObjectPrefabs.Find((SpawnedObject x) => x != null && string.Equals(x.name, prefabName));
		}

		public static void AddSpawnedObjectData(SpawnedObject spawnedObject)
		{
			if (!(m_instance == null) && !(spawnedObject == null))
			{
				m_instance.m_spawnedObjects.Add(spawnedObject);
			}
		}

		public static void RemoveSpawnedObjectData(SpawnedObject spawnedObject)
		{
			if (!(m_instance == null) && !(spawnedObject == null))
			{
				m_instance.m_spawnedObjects.Remove(spawnedObject);
			}
		}
	}
}
