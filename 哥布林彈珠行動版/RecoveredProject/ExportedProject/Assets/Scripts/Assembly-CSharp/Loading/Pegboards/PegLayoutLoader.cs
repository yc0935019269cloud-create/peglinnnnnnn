using System.Collections;
using System.Collections.Generic;
using System.IO;
using Peglin.OdinSerializer;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Loading.Pegboards
{
	public class PegLayoutLoader : MonoBehaviour
	{
		public delegate void PegboardPreInstancingFinished(PreInstancedPegboardData preInstancedPegboardData);

		public class PreInstancedPegboardData
		{
			public Queue<Transform> preInstancedObjectTransforms;

			public GameObject rootGameObject;

			public PegboardParser.PegboardData pegboardData;

			public PegLayoutLoader pegLayoutLoader;

			public PreInstancedPegboardData(Queue<Transform> preInstancedObjectTransforms, GameObject rootGameObject, PegboardParser.PegboardData pegboardData, PegLayoutLoader pegLayoutLoader)
			{
				this.preInstancedObjectTransforms = preInstancedObjectTransforms;
				this.rootGameObject = rootGameObject;
				this.pegboardData = pegboardData;
				this.pegLayoutLoader = pegLayoutLoader;
			}
		}

		private readonly Vector3 PRE_INSTANCED_OFFSET = new Vector3(1000f, 0f, 0f);

		public PegboardPreInstancingFinished OnPegboardPreInstancingFinished;

		public AssetReference DebugPegboardData;

		public bool debug;

		public static Dictionary<string, object> pegboardAssets = new Dictionary<string, object>();

		public static Dictionary<string, LongPegSnapData> snapDataAssets = new Dictionary<string, LongPegSnapData>();

		public static Dictionary<string, Sprite> pegboardSpriteAssets = new Dictionary<string, Sprite>();

		private List<IPostProcessAction> _postProcessComponents;

		private readonly List<string> PreinstancableComponents = new List<string>
		{
			PegboardParser.componentTypesMap[PegboardParser.ComponentType.RegularPeg],
			PegboardParser.componentTypesMap[PegboardParser.ComponentType.Bomb],
			PegboardParser.componentTypesMap[PegboardParser.ComponentType.BouncerPeg],
			PegboardParser.componentTypesMap[PegboardParser.ComponentType.IndestructiblePeg],
			PegboardParser.componentTypesMap[PegboardParser.ComponentType.SlimeOnlyPeg],
			PegboardParser.componentTypesMap[PegboardParser.ComponentType.BezierMeshGenerator]
		};

		private readonly List<string> PreinstancableComponentsToAlsoInstanceCoin = new List<string>
		{
			PegboardParser.componentTypesMap[PegboardParser.ComponentType.RegularPeg],
			PegboardParser.componentTypesMap[PegboardParser.ComponentType.SlimeOnlyPeg]
		};

		private float _scaledLoadingTimeElapsed = -1f;

		private float _loadingTimeWaited = -1f;

		public bool forceStopLoading;

		private WaitForSeconds _waitObject;

		private void Update()
		{
			if (_scaledLoadingTimeElapsed >= 0f)
			{
				_scaledLoadingTimeElapsed += Time.deltaTime * Time.timeScale;
			}
		}

		public static void CheckAndSavePegboardAsset(AssetReference reference)
		{
			if (!pegboardAssets.ContainsKey(reference.RuntimeKey.ToString()) || pegboardAssets[reference.RuntimeKey.ToString()] == null)
			{
				object obj = reference.LoadAssetAsync<object>().WaitForCompletion();
				if (obj == null)
				{
					Debug.LogWarning("LoadAssetAsync returned null object for asset: " + reference.RuntimeKey?.ToString() + ". Trying once more and then falling back to ignoring this object.");
					obj = reference.LoadAssetAsync<object>().WaitForCompletion();
				}
				pegboardAssets[reference.RuntimeKey.ToString()] = obj;
			}
		}

		public static void LoadLongPegSnapData()
		{
			foreach (KeyValuePair<string, AssetReference> snapDataRef in PegboardParser.LongPegData.SnapDataRefs)
			{
				if (!snapDataAssets.ContainsKey(snapDataRef.Value.RuntimeKey.ToString()))
				{
					snapDataAssets.Add(snapDataRef.Value.RuntimeKey.ToString(), snapDataRef.Value.LoadAssetAsync<LongPegSnapData>().WaitForCompletion());
				}
			}
		}

		public static Sprite LoadSpriteData(AssetReference assetReference)
		{
			if (pegboardSpriteAssets.TryGetValue(assetReference.RuntimeKey.ToString(), out var value))
			{
				return value;
			}
			value = assetReference.LoadAssetAsync<Sprite>().WaitForCompletion();
			pegboardSpriteAssets.Add(assetReference.RuntimeKey.ToString(), value);
			return value;
		}

		private void OnEnable()
		{
			if (debug)
			{
				AsyncOperationHandle<TextAsset> asyncOperationHandle = DebugPegboardData.LoadAssetAsync<TextAsset>();
				asyncOperationHandle.Completed += DataOnCompleted;
			}
		}

		private void DataOnCompleted(AsyncOperationHandle<TextAsset> obj)
		{
			PegboardParser.PegboardData pegboardData = SerializationUtility.DeserializeValue<PegboardParser.PegboardData>(new MemoryStream(obj.Result.bytes), DataFormat.Binary);
			GameObject parentGameObject = new GameObject(pegboardData.name);
			LoadPegboard(pegboardData, base.transform.position, parentGameObject);
		}

		public PreInstancedPegboardData StartPreInstancingPegboard(AssetReference pegLayoutData, string filename, float maxLoadTime = -1f)
		{
			forceStopLoading = false;
			if (!AssetLoading.Instance.PegLayouts.TryGetValue(filename, out var value))
			{
				value = SerializationUtility.DeserializeValue<PegboardParser.PegboardData>(new MemoryStream(Addressables.LoadAssetAsync<TextAsset>(pegLayoutData).WaitForCompletion().bytes), DataFormat.Binary);
			}
			GameObject rootGameObject = new GameObject("preInstancedObjectsFor-" + value.name);
			PreInstancedPegboardData preInstancedPegboardData = new PreInstancedPegboardData(new Queue<Transform>(), rootGameObject, value, this);
			StartCoroutine(PreInstancePegboard(preInstancedPegboardData, maxLoadTime));
			return preInstancedPegboardData;
		}

		public GameObject FinsihLoadingPegboardUsingPreInstances(PreInstancedPegboardData preInstancedPegboardData, Vector3 position)
		{
			SceneManager.MoveGameObjectToScene(preInstancedPegboardData.rootGameObject, SceneManager.GetActiveScene());
			preInstancedPegboardData.rootGameObject.name = preInstancedPegboardData.pegboardData.name;
			preInstancedPegboardData.rootGameObject.transform.position -= PRE_INSTANCED_OFFSET;
			LoadPegboard(preInstancedPegboardData.pegboardData, position, preInstancedPegboardData.rootGameObject, preInstancedPegboardData);
			return preInstancedPegboardData.rootGameObject;
		}

		public void ForceStopLoading(PreInstancedPegboardData preInstancedPegboardData)
		{
			forceStopLoading = true;
			if (_scaledLoadingTimeElapsed != -1f)
			{
				preInstancedPegboardData.rootGameObject.transform.position += PRE_INSTANCED_OFFSET;
				preInstancedPegboardData.rootGameObject.SetActive(value: true);
				_scaledLoadingTimeElapsed = -1f;
				OnPegboardPreInstancingFinished?.Invoke(preInstancedPegboardData);
			}
		}

		public GameObject TryLoadPegLayout(AssetReference pegLayoutData, string filename, Vector3 position)
		{
			if (!AssetLoading.Instance.PegLayouts.TryGetValue(filename, out var value))
			{
				value = SerializationUtility.DeserializeValue<PegboardParser.PegboardData>(new MemoryStream(Addressables.LoadAssetAsync<TextAsset>(pegLayoutData).WaitForCompletion().bytes), DataFormat.Binary);
			}
			return StartLoadingPegLayoutData(value, position);
		}

		public GameObject StartLoadingPegLayoutData(PegboardParser.PegboardData layoutData, Vector3 position)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = layoutData.name;
			LoadPegboard(layoutData, position, gameObject);
			return gameObject;
		}

		public void LoadPegboard(PegboardParser.PegboardData data, Vector3 position, GameObject parentGameObject, PreInstancedPegboardData preInstancedPegboardData = null)
		{
			Transform transform = parentGameObject.transform;
			transform.transform.position = new Vector3(data.posX, data.posY);
			foreach (PegboardParser.TransformData transform2 in data.transforms)
			{
				foreach (PegboardParser.Component component in transform2.components)
				{
					component.CreateInstance(transform2, transform);
					if (component is IPostProcessAction item)
					{
						if (_postProcessComponents == null)
						{
							_postProcessComponents = new List<IPostProcessAction>();
						}
						_postProcessComponents.Add(item);
					}
				}
				LoadPegboardRecursive(transform2, transform, preInstancedPegboardData, mustReparentChildren: false);
			}
			if (_postProcessComponents != null)
			{
				foreach (IPostProcessAction postProcessComponent in _postProcessComponents)
				{
					postProcessComponent.DoPostProcessAction(transform.transform);
				}
			}
			transform.transform.position += position;
		}

		public void LoadPegboardRecursive(PegboardParser.TransformData tData, Transform root, PreInstancedPegboardData preInstancedPegboardData, bool mustReparentChildren)
		{
			foreach (PegboardParser.TransformData item2 in tData.child)
			{
				if (item2 == null)
				{
					continue;
				}
				Transform transform;
				if (preInstancedPegboardData != null && CanObjectBePreInstanced(item2) && preInstancedPegboardData.preInstancedObjectTransforms.Count > 0)
				{
					transform = preInstancedPegboardData.preInstancedObjectTransforms.Dequeue().transform;
					if (mustReparentChildren)
					{
						transform.parent = root;
					}
				}
				else if (item2.prefab != null)
				{
					transform = item2.prefab.CreateInstance(item2, root);
					mustReparentChildren = true;
				}
				else
				{
					transform = new GameObject(item2.name).transform;
					transform.parent = root;
					transform.position = new Vector3(item2.posX, item2.posY);
					transform.localScale = new Vector3(item2.scaleX, item2.scaleY);
					transform.gameObject.SetActive(item2.enabled);
				}
				if (!item2.enabled)
				{
					mustReparentChildren = true;
				}
				foreach (PegboardParser.Component component in item2.components)
				{
					component.CreateInstance(item2, transform);
					if (component is IPostProcessAction item)
					{
						if (_postProcessComponents == null)
						{
							_postProcessComponents = new List<IPostProcessAction>();
						}
						_postProcessComponents.Add(item);
					}
					mustReparentChildren = true;
				}
				LoadPegboardRecursive(item2, transform, preInstancedPegboardData, mustReparentChildren);
			}
		}

		public IEnumerator PreInstancePegboard(PreInstancedPegboardData preInstancedPegboardData, float maxLoadTime)
		{
			_scaledLoadingTimeElapsed = 0f;
			_loadingTimeWaited = 0f;
			float waitTimePerInstance = maxLoadTime / (float)CountBasicPegInstancesInLayout(preInstancedPegboardData.pegboardData);
			_waitObject = new WaitForSeconds(waitTimePerInstance);
			Object.DontDestroyOnLoad(preInstancedPegboardData.rootGameObject);
			preInstancedPegboardData.rootGameObject.SetActive(value: false);
			preInstancedPegboardData.rootGameObject.transform.position = new Vector3(preInstancedPegboardData.pegboardData.posX, preInstancedPegboardData.pegboardData.posY);
			foreach (PegboardParser.TransformData transform in preInstancedPegboardData.pegboardData.transforms)
			{
				yield return StartCoroutine(PreInstancePegboardRecursive(transform, preInstancedPegboardData, waitTimePerInstance));
			}
			if (!forceStopLoading)
			{
				preInstancedPegboardData.rootGameObject.transform.position += PRE_INSTANCED_OFFSET;
				preInstancedPegboardData.rootGameObject.SetActive(value: true);
				_scaledLoadingTimeElapsed = -1f;
				OnPegboardPreInstancingFinished?.Invoke(preInstancedPegboardData);
			}
		}

		public IEnumerator PreInstancePegboardRecursive(PegboardParser.TransformData tData, PreInstancedPegboardData preInstancedPegboardData, float waitTimePerInstance)
		{
			foreach (PegboardParser.TransformData child in tData.child)
			{
				if (forceStopLoading)
				{
					yield break;
				}
				if (child == null)
				{
					continue;
				}
				if (CanObjectBePreInstanced(child))
				{
					Transform tInstance;
					if (child.prefab != null)
					{
						if (child.prefab.componentType == PegboardParser.componentTypesMap[PegboardParser.ComponentType.BezierMeshGenerator])
						{
							PegboardParser.LongPegData longPegData = child.prefab as PegboardParser.LongPegData;
							tInstance = longPegData.SetupCreateInstance(child, preInstancedPegboardData.rootGameObject.transform).transform;
							BezierCurveManager component = tInstance.GetComponent<BezierCurveManager>();
							BezierMeshGenerator component2 = tInstance.GetComponent<BezierMeshGenerator>();
							component.SetupPegGeneration();
							yield return StartCoroutine(GenerateLongPegsAsync(component.ObjectPositions.ToArray(), component.NormalPositions.ToArray(), component.polygon, tInstance, component2, waitTimePerInstance));
							LongPeg[] componentsInChildren = tInstance.gameObject.GetComponentsInChildren<LongPeg>();
							foreach (LongPeg obj in componentsInChildren)
							{
								obj.InitCoinPrefab();
								obj.InitPegText();
							}
						}
						else if (PreinstancableComponentsToAlsoInstanceCoin.Contains(child.prefab.componentType))
						{
							tInstance = child.prefab.CreateInstance(child, preInstancedPegboardData.rootGameObject.transform);
							Peg component3 = tInstance.GetComponent<Peg>();
							component3.InitCoinPrefab();
							component3.InitPegText();
						}
						else
						{
							tInstance = child.prefab.CreateInstance(child, preInstancedPegboardData.rootGameObject.transform);
						}
					}
					else
					{
						tInstance = new GameObject(child.name).transform;
						tInstance.parent = preInstancedPegboardData.rootGameObject.transform;
						tInstance.position = new Vector3(child.posX, child.posY);
						tInstance.localScale = new Vector3(child.scaleX, child.scaleY);
						tInstance.gameObject.SetActive(child.enabled);
					}
					if (!forceStopLoading)
					{
						preInstancedPegboardData.preInstancedObjectTransforms.Enqueue(tInstance);
					}
					_loadingTimeWaited += waitTimePerInstance;
					if (_loadingTimeWaited >= _scaledLoadingTimeElapsed)
					{
						yield return _waitObject;
					}
					if (forceStopLoading)
					{
						yield break;
					}
				}
				yield return StartCoroutine(PreInstancePegboardRecursive(child, preInstancedPegboardData, waitTimePerInstance));
			}
		}

		public IEnumerator GenerateLongPegsAsync(Vector3[] objectPositions, Vector3[] normalPositions, PolygonCollider2D polygon, Transform parent, BezierMeshGenerator bezierMeshGenerator, float waitTimePerInstance)
		{
			Vector3 position = bezierMeshGenerator.transform.position;
			for (int i = 0; i < objectPositions.Length - 1; i++)
			{
				Vector2 vector = (normalPositions[i] + normalPositions[i + 1]) / 2f;
				bezierMeshGenerator.GenerateMesh(position, objectPositions[i] + normalPositions[i] * -1f, objectPositions[i] + normalPositions[i], objectPositions[i + 1] + normalPositions[i + 1] * -1f, objectPositions[i + 1] + normalPositions[i + 1], i, vector, parent, polygon);
				_loadingTimeWaited += waitTimePerInstance;
				if (_loadingTimeWaited >= _scaledLoadingTimeElapsed)
				{
					yield return new WaitForSeconds(waitTimePerInstance);
				}
				if (forceStopLoading)
				{
					break;
				}
			}
		}

		private int CountBasicPegInstancesInLayout(PegboardParser.PegboardData data)
		{
			int num = 0;
			foreach (PegboardParser.TransformData transform in data.transforms)
			{
				num += CountBasicPegInstancesRecursive(transform);
			}
			return num;
		}

		private int CountBasicPegInstancesRecursive(PegboardParser.TransformData tData)
		{
			int num = 0;
			int num2 = 0;
			foreach (PegboardParser.TransformData item in tData.child)
			{
				if (item == null)
				{
					continue;
				}
				if (CanObjectBePreInstanced(item))
				{
					if (item.prefab != null && item.prefab.componentType == PegboardParser.componentTypesMap[PegboardParser.ComponentType.BezierMeshGenerator])
					{
						PegboardParser.LongPegData longPegData = item.prefab as PegboardParser.LongPegData;
						num += longPegData.numLongPegInstances;
					}
					else
					{
						num++;
					}
				}
				num2 += CountBasicPegInstancesRecursive(item);
			}
			return num + num2;
		}

		private bool CanObjectBePreInstanced(PegboardParser.TransformData tData)
		{
			if (tData.prefab != null)
			{
				return PreinstancableComponents.Contains(tData.prefab.componentType);
			}
			return true;
		}
	}
}
