using System;
using System.Collections.Generic;
using System.IO;
using Battle;
using Battle.Pachinko.Obstacles;
using Battle.PegBehaviour;
using Battle.PegBehaviour.Boids;
using Loading.Pegboards;
using MoreMountains.Feedbacks;
using Peglin.OdinSerializer;
using RNG.Scenarios;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.U2D;

public class PegboardParser
{
	public enum ComponentType
	{
		RegularPeg = 0,
		BezierMeshGenerator = 1,
		SlimeOnlyPeg = 2,
		Bomb = 3,
		IndestructiblePeg = 4,
		BouncerPeg = 5,
		EffectPeg = 6,
		InvisibleOnAwake = 7,
		FlickeringPeg = 8,
		LinearPegMovement = 9,
		MirroredPeg = 10,
		OffsetPositionOnStart = 11,
		PegMoveAndReturn = 12,
		PegBlock = 13,
		RandomPegField = 14,
		PegTeleport = 15,
		RotatingPegCircle = 16,
		FireworkMovement = 17,
		FlickeringPegLoopCreator = 18,
		PegSquareMovement = 19,
		ScenarioNavigationBonusChest = 20,
		PegSplineFollow = 21,
		PegboardBlackHole = 22,
		RandomObscuredPegGrid = 23,
		Bouncer = 24,
		BouncerMines = 25,
		DragonPegLayoutSimulator = 26,
		PegLayoutAlternator = 27,
		PegLayoutHolder = 28,
		DisappearForNavigation = 29,
		BoidsPegLayout = 30
	}

	[Serializable]
	public class PegboardData
	{
		public string name = "";

		public float posX;

		public float posY;

		public List<TransformData> transforms = new List<TransformData>();
	}

	[Serializable]
	public class TransformData
	{
		public string name;

		public float posX;

		public float posY;

		public float scaleX;

		public float scaleY;

		[SerializeField]
		public List<TransformData> child = new List<TransformData>();

		public List<Component> components = new List<Component>();

		public Component prefab;

		public bool enabled;

		public TransformData(Transform t)
		{
			name = t.name;
			posX = t.position.x;
			posY = t.position.y;
			scaleX = t.localScale.x;
			scaleY = t.localScale.y;
			enabled = t.gameObject.activeSelf;
		}
	}

	[Serializable]
	public class Component
	{
		public string componentType;

		public bool enabled = true;

		public virtual Transform CreateInstance(TransformData td, Transform parent)
		{
			return null;
		}

		protected static Transform CreateEmptyTransform(TransformData td)
		{
			Debug.LogError("We've fetched a null prefab for a regular or dull peg, creating an empty transform and skipping");
			return new GameObject
			{
				name = td.name + " empty"
			}.transform;
		}
	}

	[Serializable]
	public class RegularPegData : Component
	{
		public Peg.PegType pegType;

		public Peg.SlimeType slimeType;

		public bool specialPegBlocker;

		public bool coinBlocker;

		public int numCoinOverride;

		public Color color;

		public bool canBeDull;

		[FormerlySerializedAs("fakePeg")]
		public bool isFakePeg;

		private static AssetReference RegularPegAssetReference = new AssetReference("RegularPeg");

		private static AssetReference DullPegAssetReference = new AssetReference("DullPeg");

		public RegularPegData(RegularPeg peg, bool isFake = false)
		{
			componentType = componentTypesMap[ComponentType.RegularPeg];
			pegType = peg.pegType;
			slimeType = peg.slimeType;
			numCoinOverride = peg.numCoinOverride;
			specialPegBlocker = peg.specialPegBlocker;
			coinBlocker = peg.coinBlocker;
			canBeDull = peg.canBeDull;
			SpriteRenderer component = peg.GetComponent<SpriteRenderer>();
			color = ((component != null) ? component.color : Color.white);
			isFakePeg = isFake;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			GameObject gameObject;
			if (pegType == Peg.PegType.DULL)
			{
				PegLayoutLoader.CheckAndSavePegboardAsset(DullPegAssetReference);
				gameObject = (GameObject)PegLayoutLoader.pegboardAssets[DullPegAssetReference.RuntimeKey.ToString()];
			}
			else
			{
				PegLayoutLoader.CheckAndSavePegboardAsset(RegularPegAssetReference);
				gameObject = (GameObject)PegLayoutLoader.pegboardAssets[RegularPegAssetReference.RuntimeKey.ToString()];
			}
			if (gameObject == null)
			{
				Debug.LogError("PegPrefab null when trying to Create Instance of type: " + pegType);
				if (pegType == Peg.PegType.DULL)
				{
					PegLayoutLoader.CheckAndSavePegboardAsset(RegularPegAssetReference);
					gameObject = (GameObject)PegLayoutLoader.pegboardAssets[RegularPegAssetReference.RuntimeKey.ToString()];
				}
			}
			Vector3 position = new Vector3(td.posX, td.posY);
			Vector3 localScale = new Vector3(td.scaleX, td.scaleY);
			if (gameObject == null)
			{
				return Component.CreateEmptyTransform(td);
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, parent);
			gameObject2.transform.localScale = localScale;
			gameObject2.transform.name = td.name;
			gameObject2.SetActive(td.enabled);
			RegularPeg component = gameObject2.GetComponent<RegularPeg>();
			component.pegType = pegType;
			component.slimeType = slimeType;
			component.numCoinOverride = numCoinOverride;
			component.specialPegBlocker = specialPegBlocker;
			component.coinBlocker = coinBlocker;
			component.canBeDull = canBeDull;
			SpriteRenderer component2 = component.GetComponent<SpriteRenderer>();
			component2.color = color;
			if (isFakePeg)
			{
				component.enabled = false;
				gameObject2.GetComponent<Collider2D>().enabled = false;
				gameObject2.GetComponent<Animator>().enabled = false;
				gameObject2.GetComponent<MMFeedbacks>().enabled = false;
			}
			if (component.pegType == Peg.PegType.CRYSTAL)
			{
				component2.sprite = component.crystalPegSprite;
			}
			return component.transform;
		}
	}

	[Serializable]
	public class LongPegData : Component
	{
		public enum SnapData
		{
			Default = 0,
			Extra = 1,
			BothSides = 2,
			BothSidesExtraLong = 3
		}

		private const string DEFAULT_SNAP = "DefaultLongPegSnapData";

		private const string BOTH_SIDES_SNAP = "BothSidesLongPegSnapData";

		private const string EXTRA_SNAP = "ExtraSnapLongPegSnapData";

		private const string BOTH_SIDES_EXTRA_LONG = "BothSidesExtraLongPegSnapData";

		public SnapData snapData;

		public float timeToDisappear;

		public Spline spline;

		public int numLongPegInstances;

		private static AssetReference LongPegGeneratorAssetRef = new AssetReference("LongPegGenerator");

		public static Dictionary<string, AssetReference> SnapDataRefs = new Dictionary<string, AssetReference>
		{
			{
				"DefaultLongPegSnapData",
				new AssetReference("DefaultLongPegSnapData")
			},
			{
				"BothSidesLongPegSnapData",
				new AssetReference("BothSidesLongPegSnapData")
			},
			{
				"ExtraSnapLongPegSnapData",
				new AssetReference("ExtraSnapLongPegSnapData")
			},
			{
				"BothSidesExtraLongPegSnapData",
				new AssetReference("BothSidesExtraLongPegSnapData")
			}
		};

		public LongPegData(BezierMeshGenerator component)
		{
			componentType = componentTypesMap[ComponentType.BezierMeshGenerator];
			snapData = ObjectToSnapData(component.longPegSnapData);
			timeToDisappear = component.timeToDisappear;
			spline = component.GetComponent<SpriteShapeController>().spline;
			numLongPegInstances = CountLongPegInstances(component, spline);
		}

		private SnapData ObjectToSnapData(LongPegSnapData data)
		{
			switch (data.name)
			{
			case "DefaultLongPegSnapData":
				return SnapData.Default;
			case "BothSidesLongPegSnapData":
				return SnapData.BothSides;
			case "ExtraSnapLongPegSnapData":
				return SnapData.Extra;
			case "BothSidesExtraLongPegSnapData":
				return SnapData.BothSidesExtraLong;
			default:
				Debug.LogError("Did not find long peg snap data for " + data.name);
				return SnapData.Default;
			}
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			GameObject gameObject = SetupCreateInstance(td, parent);
			BezierCurveManager component = gameObject.GetComponent<BezierCurveManager>();
			if (component != null)
			{
				component.GenerateLongPegs();
			}
			return gameObject.transform;
		}

		public GameObject SetupCreateInstance(TransformData td, Transform parent)
		{
			PegLayoutLoader.CheckAndSavePegboardAsset(LongPegGeneratorAssetRef);
			PegLayoutLoader.LoadLongPegSnapData();
			GameObject gameObject = (GameObject)PegLayoutLoader.pegboardAssets[LongPegGeneratorAssetRef.RuntimeKey.ToString()];
			Vector3 position = new Vector3(td.posX, td.posY);
			Vector3 localScale = new Vector3(td.scaleX, td.scaleY);
			if (gameObject == null)
			{
				return Component.CreateEmptyTransform(td).gameObject;
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, parent);
			gameObject2.transform.localScale = localScale;
			gameObject2.transform.name = td.name;
			gameObject2.SetActive(td.enabled);
			BezierMeshGenerator component = gameObject2.GetComponent<BezierMeshGenerator>();
			switch (snapData)
			{
			case SnapData.Extra:
				component.longPegSnapData = PegLayoutLoader.snapDataAssets[SnapDataRefs["ExtraSnapLongPegSnapData"].RuntimeKey.ToString()];
				break;
			case SnapData.BothSides:
				component.longPegSnapData = PegLayoutLoader.snapDataAssets[SnapDataRefs["BothSidesLongPegSnapData"].RuntimeKey.ToString()];
				break;
			default:
				component.longPegSnapData = PegLayoutLoader.snapDataAssets[SnapDataRefs["DefaultLongPegSnapData"].RuntimeKey.ToString()];
				break;
			}
			component.timeToDisappear = timeToDisappear;
			SpriteShapeController component2 = gameObject2.GetComponent<SpriteShapeController>();
			component2.spline.Clear();
			component2.spline.isOpenEnded = spline.isOpenEnded;
			for (int i = 0; i < spline.GetPointCount(); i++)
			{
				component2.spline.InsertPointAt(i, spline.GetPosition(i));
				component2.spline.SetTangentMode(i, spline.GetTangentMode(i));
				component2.spline.SetCorner(i, spline.GetCorner(i));
				component2.spline.SetHeight(i, spline.GetHeight(i));
				component2.spline.SetLeftTangent(i, spline.GetLeftTangent(i));
				component2.spline.SetRightTangent(i, spline.GetRightTangent(i));
				component2.spline.SetSpriteIndex(i, spline.GetSpriteIndex(i));
			}
			return gameObject2;
		}

		private int CountLongPegInstances(BezierMeshGenerator component, Spline spline)
		{
			List<Vector3> list = new List<Vector3>();
			component.GetComponent<BezierCurveManager>().SampleSpline(spline, list, new List<Vector3>());
			return Mathf.Max(list.Count - 1, 0);
		}
	}

	[Serializable]
	public class BombData : Component
	{
		public bool isRigged;

		public static AssetReference BombAssetRef = new AssetReference("Bomb");

		public static AssetReference RiggedBombAssetRef = new AssetReference("BombRigged");

		public BombData(Bomb c)
		{
			componentType = componentTypesMap[ComponentType.Bomb];
			isRigged = c.isRigged;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			GameObject gameObject;
			if (isRigged)
			{
				PegLayoutLoader.CheckAndSavePegboardAsset(RiggedBombAssetRef);
				gameObject = (GameObject)PegLayoutLoader.pegboardAssets[RiggedBombAssetRef.RuntimeKey.ToString()];
			}
			else
			{
				PegLayoutLoader.CheckAndSavePegboardAsset(BombAssetRef);
				gameObject = (GameObject)PegLayoutLoader.pegboardAssets[BombAssetRef.RuntimeKey.ToString()];
			}
			if (gameObject == null)
			{
				return Component.CreateEmptyTransform(td);
			}
			Vector3 position = new Vector3(td.posX, td.posY);
			Vector3 localScale = new Vector3(td.scaleX, td.scaleY);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, parent);
			gameObject2.transform.localScale = localScale;
			gameObject2.transform.name = td.name;
			gameObject2.SetActive(td.enabled);
			Bomb component = gameObject2.GetComponent<Bomb>();
			component.isRigged = isRigged;
			return component.transform;
		}
	}

	[Serializable]
	public class SlimeOnlyPegData : Component
	{
		public static AssetReference AssetRef = new AssetReference("SlimeOnlyPeg");

		public SlimeOnlyPegData()
		{
			componentType = componentTypesMap[ComponentType.SlimeOnlyPeg];
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegLayoutLoader.CheckAndSavePegboardAsset(AssetRef);
			GameObject gameObject = (GameObject)PegLayoutLoader.pegboardAssets[AssetRef.RuntimeKey.ToString()];
			Vector3 position = new Vector3(td.posX, td.posY);
			Vector3 localScale = new Vector3(td.scaleX, td.scaleY);
			if (gameObject == null)
			{
				return Component.CreateEmptyTransform(td);
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, parent);
			gameObject2.transform.localScale = localScale;
			gameObject2.transform.name = td.name;
			gameObject2.SetActive(td.enabled);
			return gameObject2.transform;
		}
	}

	[Serializable]
	public class IndestructiblePegData : Component
	{
		public static AssetReference AssetRef = new AssetReference("IndestructiblePeg");

		public IndestructiblePegData(IndestructiblePeg peg)
		{
			componentType = componentTypesMap[ComponentType.IndestructiblePeg];
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegLayoutLoader.CheckAndSavePegboardAsset(AssetRef);
			GameObject gameObject = (GameObject)PegLayoutLoader.pegboardAssets[AssetRef.RuntimeKey.ToString()];
			Vector3 position = new Vector3(td.posX, td.posY);
			Vector3 localScale = new Vector3(td.scaleX, td.scaleY);
			if (gameObject == null)
			{
				return Component.CreateEmptyTransform(td);
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, parent);
			gameObject2.transform.localScale = localScale;
			gameObject2.transform.name = td.name;
			gameObject2.SetActive(td.enabled);
			return gameObject2.transform;
		}
	}

	[Serializable]
	public class InvisibleOnAwakeData : Component
	{
		public InvisibleOnAwakeData(InvisibleOnAwake c)
		{
			componentType = componentTypesMap[ComponentType.InvisibleOnAwake];
			enabled = c.enabled;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			parent.gameObject.AddComponent<InvisibleOnAwake>().enabled = enabled;
			return parent;
		}
	}

	[Serializable]
	public class FlickeringPegData : Component
	{
		public float initialDelay;

		public bool randomizeInitialDelay;

		public float minDelay;

		public float maxDelay;

		public float minDelayWhenInvisible;

		public float maxDelayWhenInvisible;

		public bool useDifferentMinAndMaxForInvisible;

		public float fadeTime;

		public bool fromInvis;

		public FlickeringPegData(FlickeringPeg component)
		{
			componentType = componentTypesMap[ComponentType.FlickeringPeg];
			initialDelay = component.initialDelay;
			randomizeInitialDelay = component.randomizeInitialDelay;
			minDelay = component.minDelay;
			maxDelay = component.maxDelay;
			minDelayWhenInvisible = component.minDelayWhenInvisible;
			maxDelayWhenInvisible = component.maxDelayWhenInvisible;
			useDifferentMinAndMaxForInvisible = component.useDifferentMinAndMaxForInvisible;
			fadeTime = component.fadeTime;
			fromInvis = component.fromInvis;
			enabled = component.enabled;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			FlickeringPeg flickeringPeg = parent.gameObject.AddComponent<FlickeringPeg>();
			flickeringPeg.initialDelay = initialDelay;
			flickeringPeg.randomizeInitialDelay = randomizeInitialDelay;
			flickeringPeg.minDelay = minDelay;
			flickeringPeg.maxDelay = maxDelay;
			flickeringPeg.minDelayWhenInvisible = minDelayWhenInvisible;
			flickeringPeg.maxDelayWhenInvisible = maxDelayWhenInvisible;
			flickeringPeg.useDifferentMinAndMaxForInvisible = useDifferentMinAndMaxForInvisible;
			flickeringPeg.fadeTime = fadeTime;
			flickeringPeg.fromInvis = fromInvis;
			flickeringPeg.enabled = enabled;
			return parent;
		}
	}

	[Serializable]
	public class LinearPegMovementData : Component
	{
		public float xMovement;

		public float yMovement;

		public float leftBoundary;

		public float rightBoundary;

		public bool isKinematic;

		public float mass;

		public float drag;

		public float gravityScale;

		public float angularDrag;

		public LinearPegMovementData(LinearPegMovement component)
		{
			xMovement = component.xMovement;
			yMovement = component.yMovement;
			leftBoundary = component.leftBoundary;
			rightBoundary = component.rightBoundary;
			Rigidbody2D component2 = component.GetComponent<Rigidbody2D>();
			if (component2 == null)
			{
				Debug.LogError("Rigidbody shouldn't be null with LinearPegMovement!");
			}
			else
			{
				isKinematic = component2.isKinematic;
				mass = component2.mass;
				drag = component2.drag;
				gravityScale = component2.gravityScale;
				angularDrag = component2.angularDrag;
			}
			enabled = component.enabled;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			Rigidbody2D rigidbody2D = parent.gameObject.AddComponent<Rigidbody2D>();
			rigidbody2D.isKinematic = isKinematic;
			rigidbody2D.mass = mass;
			rigidbody2D.gravityScale = gravityScale;
			rigidbody2D.drag = drag;
			rigidbody2D.angularDrag = angularDrag;
			LinearPegMovement linearPegMovement = parent.gameObject.AddComponent<LinearPegMovement>();
			linearPegMovement.xMovement = xMovement;
			linearPegMovement.yMovement = yMovement;
			linearPegMovement.leftBoundary = leftBoundary;
			linearPegMovement.rightBoundary = rightBoundary;
			linearPegMovement.enabled = enabled;
			return parent;
		}
	}

	[Serializable]
	public class MirroredPegData : Component
	{
		public float mirrorX;

		public float mirrorY;

		public MirroredPegData(MirroredPeg component)
		{
			componentType = componentTypesMap[ComponentType.MirroredPeg];
			mirrorX = component.mirrorVector.x;
			mirrorY = component.mirrorVector.y;
			enabled = component.enabled;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			MirroredPeg mirroredPeg = parent.gameObject.AddComponent<MirroredPeg>();
			mirroredPeg.mirrorVector = new Vector2(mirrorX, mirrorY);
			mirroredPeg.enabled = enabled;
			return parent;
		}
	}

	[Serializable]
	public class OffsetPositionOnStartData : Component
	{
		public OffsetPositionOnStartData(OffsetPositionOnStart c)
		{
			componentType = componentTypesMap[ComponentType.OffsetPositionOnStart];
			enabled = c.enabled;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			parent.gameObject.AddComponent<OffsetPositionOnStart>().enabled = enabled;
			return parent;
		}
	}

	[Serializable]
	public class PegMoveAndReturnData : Component
	{
		public float startingPositionX;

		public float startingPositionY;

		public float targetOffsetAX;

		public float targetOffsetAY;

		public float targetOffsetBX;

		public float targetOffsetBY;

		public float moveToASpeed;

		public float moveToBSpeed;

		public PegMoveAndReturnData(PegMoveAndReturn c)
		{
			componentType = componentTypesMap[ComponentType.PegMoveAndReturn];
			startingPositionX = c.startingPosition.x;
			startingPositionY = c.startingPosition.y;
			targetOffsetAX = c.targetOffsetA.x;
			targetOffsetAY = c.targetOffsetA.y;
			targetOffsetBX = c.targetOffsetB.x;
			targetOffsetBY = c.targetOffsetB.y;
			moveToASpeed = c.moveToASpeed;
			moveToBSpeed = c.moveToBSpeed;
			enabled = c.enabled;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegMoveAndReturn pegMoveAndReturn = parent.gameObject.AddComponent<PegMoveAndReturn>();
			pegMoveAndReturn.startingPosition = new Vector2(startingPositionX, startingPositionY);
			pegMoveAndReturn.targetOffsetA = new Vector2(targetOffsetAX, targetOffsetAY);
			pegMoveAndReturn.targetOffsetB = new Vector2(targetOffsetBX, targetOffsetBY);
			pegMoveAndReturn.moveToASpeed = moveToASpeed;
			pegMoveAndReturn.moveToBSpeed = moveToBSpeed;
			pegMoveAndReturn.enabled = enabled;
			return parent;
		}
	}

	[Serializable]
	public class PegBlockData : Component
	{
		public float width;

		public PegBlockData(PegBlock c)
		{
			width = c.width;
			enabled = c.enabled;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegBlock pegBlock = parent.gameObject.AddComponent<PegBlock>();
			pegBlock.width = width;
			pegBlock.enabled = enabled;
			return parent;
		}
	}

	[Serializable]
	public class RandomPegFieldData : Component
	{
		public string[] pegPrefabs;

		public float[] pegTypeSpawnRatio;

		public float spawnObjectRadius;

		public int samplesBeforeRejection;

		public bool moveEveryTurn;

		public int moveObjectsRetrySampleBoost;

		public float moveTransformTime;

		public static AssetReference SquareAssetReference = new AssetReference("Square");

		public RandomPegFieldData(RandomPegField c)
		{
			pegPrefabs = new string[c.pegPrefabs.Length];
			for (int i = 0; i < c.pegPrefabs.Length; i++)
			{
				pegPrefabs[i] = c.pegPrefabs[i]?.name;
			}
			pegTypeSpawnRatio = c.pegTypeSpawnRatio;
			spawnObjectRadius = c.spawnObjectRadius;
			samplesBeforeRejection = c.samplesBeforeRejection;
			moveEveryTurn = c.moveEveryTurn;
			moveObjectsRetrySampleBoost = c.moveObjectsRetrySampleBoost;
			moveTransformTime = c.moveTransformTime;
			enabled = c.enabled;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			RandomPegField randomPegField = parent.gameObject.AddComponent<RandomPegField>();
			Peg[] array = new Peg[pegPrefabs.Length];
			for (int i = 0; i < pegPrefabs.Length; i++)
			{
				if (pegPrefabs[i] != null)
				{
					AssetReference assetReference = new AssetReference(pegPrefabs[i]);
					PegLayoutLoader.CheckAndSavePegboardAsset(assetReference);
					GameObject gameObject = (GameObject)PegLayoutLoader.pegboardAssets[assetReference.RuntimeKey.ToString()];
					if (gameObject != null)
					{
						array[i] = gameObject.GetComponent<Peg>();
					}
				}
			}
			randomPegField.pegPrefabs = array;
			randomPegField.pegTypeSpawnRatio = pegTypeSpawnRatio;
			randomPegField.spawnObjectRadius = spawnObjectRadius;
			randomPegField.samplesBeforeRejection = samplesBeforeRejection;
			randomPegField.moveEveryTurn = moveEveryTurn;
			randomPegField.moveObjectsRetrySampleBoost = moveObjectsRetrySampleBoost;
			randomPegField.moveTransformTime = moveTransformTime;
			randomPegField.enabled = enabled;
			Sprite sprite = PegLayoutLoader.LoadSpriteData(SquareAssetReference);
			parent.gameObject.AddComponent<SpriteRenderer>().sprite = sprite;
			return parent;
		}
	}

	[Serializable]
	public class ScenarioNavigationBonusChestData : Component, IPostProcessAction
	{
		public static AssetReference uiPurchaseAudio = new AssetReference("UI_Purchase_Item");

		public static AssetReference mixerGroup = new AssetReference("SFX");

		private ScenarioNavigationBonusChest _componentInstance;

		public ScenarioNavigationBonusChestData(ScenarioNavigationBonusChest c)
		{
			enabled = c.enabled;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			_componentInstance = parent.gameObject.AddComponent<ScenarioNavigationBonusChest>();
			_componentInstance._bonusChestSound = Addressables.LoadAssetAsync<AudioClip>(uiPurchaseAudio).WaitForCompletion();
			AudioSource audioSource = _componentInstance.gameObject.AddComponent<AudioSource>();
			AudioMixerGroup[] array = Addressables.LoadAssetAsync<AudioMixer>(mixerGroup).WaitForCompletion().FindMatchingGroups("Master");
			if (array.Length != 0)
			{
				audioSource.outputAudioMixerGroup = array[0];
			}
			_componentInstance.enabled = enabled;
			return _componentInstance.transform;
		}

		public void DoPostProcessAction(Transform root)
		{
			int numberOfBombs = root.GetComponentsInChildren<Bomb>().Length;
			_componentInstance.SetNumberOfBombs(numberOfBombs);
		}
	}

	[Serializable]
	public class PegTeleportData : Component
	{
		public float percentMovement;

		public float disappearTime;

		public float disappearVectorX;

		public float disappearVectorY;

		public float offsetX;

		public float offsetY;

		public PegTeleportData(PegTeleport c)
		{
			percentMovement = c.percentMovement;
			disappearTime = c.disappearTime;
			disappearVectorX = c.disappearVector.x;
			disappearVectorY = c.disappearVector.y;
			enabled = c.enabled;
			offsetX = c.teleportOffset.x;
			offsetY = c.teleportOffset.y;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegTeleport pegTeleport = parent.gameObject.AddComponent<PegTeleport>();
			pegTeleport.percentMovement = percentMovement;
			pegTeleport.disappearTime = disappearTime;
			pegTeleport.disappearVector = new Vector2(disappearVectorX, disappearVectorY);
			pegTeleport.enabled = enabled;
			pegTeleport.teleportOffset = new Vector2(offsetX, offsetY);
			return parent;
		}
	}

	[Serializable]
	public class RotatingPegCircleData : Component
	{
		public string prefab;

		public int numPegs;

		public float radius;

		public float rotateSpeed;

		public RotatingPegCircleData(RotatingPegCircle c)
		{
			componentType = componentTypesMap[ComponentType.RotatingPegCircle];
			if (c.pegPrefab != null)
			{
				prefab = c.pegPrefab.name;
			}
			numPegs = c.numPegs;
			radius = c.radius;
			rotateSpeed = c.rotateSpeed;
			enabled = c.enabled;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			RotatingPegCircle rotatingPegCircle = parent.gameObject.AddComponent<RotatingPegCircle>();
			if (prefab != null)
			{
				AssetReference assetReference = new AssetReference(prefab);
				PegLayoutLoader.CheckAndSavePegboardAsset(assetReference);
				rotatingPegCircle.pegPrefab = (GameObject)PegLayoutLoader.pegboardAssets[assetReference.RuntimeKey.ToString()];
			}
			rotatingPegCircle.numPegs = numPegs;
			rotatingPegCircle.radius = radius;
			rotatingPegCircle.rotateSpeed = rotateSpeed;
			rotatingPegCircle.enabled = enabled;
			return parent;
		}
	}

	[Serializable]
	public class FireworkMovementData : Component
	{
		public float timeBetweenToggle;

		public static AssetReference FireworkAssetRef = new AssetReference("Firework");

		public FireworkMovementData(FireworkMovement c)
		{
			componentType = componentTypesMap[ComponentType.FireworkMovement];
			timeBetweenToggle = c.timeBetweenToggle;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegLayoutLoader.CheckAndSavePegboardAsset(FireworkAssetRef);
			GameObject gameObject = (GameObject)PegLayoutLoader.pegboardAssets[FireworkAssetRef.RuntimeKey.ToString()];
			Vector3 position = new Vector3(td.posX, td.posY);
			Vector3 localScale = new Vector3(td.scaleX, td.scaleY);
			if (gameObject == null)
			{
				return Component.CreateEmptyTransform(td);
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, parent);
			gameObject2.transform.localScale = localScale;
			gameObject2.transform.name = td.name;
			gameObject2.SetActive(td.enabled);
			gameObject2.GetComponent<FireworkMovement>().timeBetweenToggle = timeBetweenToggle;
			return gameObject2.transform;
		}
	}

	[Serializable]
	public class FlickeringPegLoopCreatorData : Component
	{
		public int numRunners;

		public float visibleTime;

		public FlickeringPegLoopCreatorData(FlickeringPegLoopCreator c)
		{
			componentType = componentTypesMap[ComponentType.FlickeringPegLoopCreator];
			numRunners = c.numRunners;
			visibleTime = c.visTime;
			enabled = c.enabled;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			FlickeringPegLoopCreator flickeringPegLoopCreator = parent.gameObject.AddComponent<FlickeringPegLoopCreator>();
			flickeringPegLoopCreator.numRunners = numRunners;
			flickeringPegLoopCreator.visTime = visibleTime;
			flickeringPegLoopCreator.enabled = enabled;
			return parent;
		}
	}

	[Serializable]
	public class PegboardBlackHoleData : Component
	{
		public float gravityRange;

		public float gravityStrength;

		public static AssetReference AssetRef = new AssetReference("BlackHole");

		public PegboardBlackHoleData(PegboardBlackHole c)
		{
			componentType = componentTypesMap[ComponentType.PegboardBlackHole];
			gravityRange = c.gravityRange;
			gravityStrength = c.gravityStrength;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegLayoutLoader.CheckAndSavePegboardAsset(AssetRef);
			GameObject gameObject = (GameObject)PegLayoutLoader.pegboardAssets[AssetRef.RuntimeKey.ToString()];
			Vector3 position = new Vector3(td.posX, td.posY);
			Vector3 localScale = new Vector3(td.scaleX, td.scaleY);
			if (gameObject == null)
			{
				return Component.CreateEmptyTransform(td);
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, parent);
			gameObject2.transform.localScale = localScale;
			gameObject2.transform.name = td.name;
			gameObject2.SetActive(td.enabled);
			PegboardBlackHole component = gameObject2.GetComponent<PegboardBlackHole>();
			component.gravityRange = gravityRange;
			component.gravityStrength = gravityStrength;
			return gameObject2.transform;
		}
	}

	[Serializable]
	public class RandomObscuredPegGridData : Component
	{
		public static AssetReference AssetRef = new AssetReference("SapperSweeperField");

		public static AssetReference resetSFX = new AssetReference("Assets/Audio/enemies/Combat_Beast_Attack.wav");

		public static AssetReference mixerGroup = new AssetReference("SFX");

		public RandomObscuredPegGridData()
		{
			componentType = componentTypesMap[ComponentType.RandomObscuredPegGrid];
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegLayoutLoader.CheckAndSavePegboardAsset(AssetRef);
			GameObject gameObject = (GameObject)PegLayoutLoader.pegboardAssets[AssetRef.RuntimeKey.ToString()];
			Vector3 position = new Vector3(td.posX, td.posY);
			Vector3 localScale = new Vector3(td.scaleX, td.scaleY);
			if (gameObject == null)
			{
				return Component.CreateEmptyTransform(td);
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, parent.parent);
			gameObject2.transform.localScale = localScale;
			gameObject2.transform.name = td.name;
			gameObject2.SetActive(td.enabled);
			gameObject2.GetComponent<RandomObscuredPegGrid>().resetingSound = Addressables.LoadAssetAsync<AudioClip>(resetSFX).WaitForCompletion();
			AudioSource audioSource = gameObject2.gameObject.AddComponent<AudioSource>();
			AudioMixerGroup[] array = Addressables.LoadAssetAsync<AudioMixer>(mixerGroup).WaitForCompletion().FindMatchingGroups("Master");
			if (array.Length != 0)
			{
				audioSource.outputAudioMixerGroup = array[0];
			}
			return gameObject2.transform;
		}
	}

	[Serializable]
	public class BoidsPegLayoutData : Component
	{
		public bool shouldDropAdditionalPegs;

		public int pegCount = 125;

		public static AssetReference AssetRef = new AssetReference("BoidsContainer");

		public BoidsPegLayoutData(BoidsPegLayout bpl)
		{
			componentType = componentTypesMap[ComponentType.BoidsPegLayout];
			shouldDropAdditionalPegs = bpl.shouldDropAdditionalPegs;
			pegCount = bpl.pegCount;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegLayoutLoader.CheckAndSavePegboardAsset(AssetRef);
			GameObject gameObject = (GameObject)PegLayoutLoader.pegboardAssets[AssetRef.RuntimeKey.ToString()];
			Vector3 position = new Vector3(td.posX, td.posY);
			Vector3 localScale = new Vector3(td.scaleX, td.scaleY);
			if (gameObject == null)
			{
				return Component.CreateEmptyTransform(td);
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, parent.parent);
			gameObject2.transform.localScale = localScale;
			gameObject2.transform.name = td.name;
			gameObject2.SetActive(td.enabled);
			BoidsPegLayout component = gameObject2.GetComponent<BoidsPegLayout>();
			if (component != null)
			{
				component.shouldDropAdditionalPegs = shouldDropAdditionalPegs;
				component.pegCount = pegCount;
			}
			return gameObject2.transform;
		}
	}

	[Serializable]
	public class BouncerData : Component
	{
		public static AssetReference BouncerAssetRef = new AssetReference("BouncerRound");

		public BouncerData(Bouncer c)
		{
			componentType = componentTypesMap[ComponentType.Bouncer];
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegLayoutLoader.CheckAndSavePegboardAsset(BouncerAssetRef);
			GameObject gameObject = (GameObject)PegLayoutLoader.pegboardAssets[BouncerAssetRef.RuntimeKey.ToString()];
			Vector3 position = new Vector3(td.posX, td.posY);
			Vector3 localScale = new Vector3(td.scaleX, td.scaleY);
			if (gameObject == null)
			{
				return Component.CreateEmptyTransform(td);
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, parent);
			gameObject2.transform.localScale = localScale;
			gameObject2.transform.name = td.name;
			gameObject2.SetActive(td.enabled);
			return gameObject2.transform;
		}
	}

	[Serializable]
	public class BouncerMinesData : Component
	{
		public static AssetReference BouncerAssetRef = new AssetReference("BouncerMines");

		public BouncerMinesData(Bouncer c)
		{
			componentType = componentTypesMap[ComponentType.BouncerMines];
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegLayoutLoader.CheckAndSavePegboardAsset(BouncerAssetRef);
			GameObject gameObject = (GameObject)PegLayoutLoader.pegboardAssets[BouncerAssetRef.RuntimeKey.ToString()];
			Vector3 position = new Vector3(td.posX, td.posY);
			Vector3 localScale = new Vector3(td.scaleX, td.scaleY);
			if (gameObject == null)
			{
				return Component.CreateEmptyTransform(td);
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, parent);
			gameObject2.transform.localScale = localScale;
			gameObject2.transform.name = td.name;
			gameObject2.SetActive(td.enabled);
			return gameObject2.transform;
		}
	}

	[Serializable]
	public class BouncerPegData : Component
	{
		public float bounceScaleX;

		public float bounceScaleY;

		public int numCoinOverride;

		public bool removeDuringNav;

		public static AssetReference BouncerPegAssetRef = new AssetReference("LargeBouncerPeg");

		public BouncerPegData(BouncerPeg c)
		{
			componentType = componentTypesMap[ComponentType.BouncerPeg];
			bounceScaleX = c._bounceScaleOffset.x;
			bounceScaleY = c._bounceScaleOffset.y;
			numCoinOverride = c.numCoinOverride;
			removeDuringNav = c.removeDuringNav;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegLayoutLoader.CheckAndSavePegboardAsset(BouncerPegAssetRef);
			GameObject gameObject = (GameObject)PegLayoutLoader.pegboardAssets[BouncerPegAssetRef.RuntimeKey.ToString()];
			Vector3 position = new Vector3(td.posX, td.posY);
			Vector3 localScale = new Vector3(td.scaleX, td.scaleY);
			if (gameObject == null)
			{
				return Component.CreateEmptyTransform(td);
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, parent);
			gameObject2.transform.localScale = localScale;
			gameObject2.transform.name = td.name;
			gameObject2.SetActive(td.enabled);
			BouncerPeg component = gameObject2.GetComponent<BouncerPeg>();
			component._bounceScaleOffset = new Vector3(bounceScaleX, bounceScaleY);
			component.numCoinOverride = numCoinOverride;
			component.removeDuringNav = removeDuringNav;
			return gameObject2.transform;
		}
	}

	[Serializable]
	public class EffectBouncerPegData : Component
	{
		public float bounceScaleX;

		public float bounceScaleY;

		public int numCoinOverride;

		public bool removeDuringNav;

		public string effectPegString;

		public int orderingInLayer;

		public static AssetReference EffectPegAssetRef = new AssetReference("baseLargeBouncerEffectPeg");

		public EffectBouncerPegData(EffectPeg effectPeg)
		{
			BouncerPeg componentInParent = effectPeg.GetComponentInParent<BouncerPeg>(includeInactive: true);
			componentType = componentTypesMap[ComponentType.EffectPeg];
			bounceScaleX = componentInParent._bounceScaleOffset.x;
			bounceScaleY = componentInParent._bounceScaleOffset.y;
			numCoinOverride = componentInParent.numCoinOverride;
			removeDuringNav = componentInParent.removeDuringNav;
			effectPegString = effectPeg.effectName;
			orderingInLayer = effectPeg.GetComponent<SpriteRenderer>().sortingOrder;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegLayoutLoader.CheckAndSavePegboardAsset(EffectPegAssetRef);
			GameObject gameObject = (GameObject)PegLayoutLoader.pegboardAssets[EffectPegAssetRef.RuntimeKey.ToString()];
			Vector3 position = new Vector3(td.posX, td.posY);
			Vector3 localScale = new Vector3(td.scaleX, td.scaleY);
			if (gameObject == null)
			{
				return Component.CreateEmptyTransform(td);
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, parent);
			gameObject2.transform.localScale = localScale;
			gameObject2.transform.name = td.name;
			gameObject2.SetActive(td.enabled);
			BouncerPeg component = gameObject2.GetComponent<BouncerPeg>();
			component._bounceScaleOffset = new Vector3(bounceScaleX, bounceScaleY);
			component.numCoinOverride = numCoinOverride;
			component.removeDuringNav = removeDuringNav;
			EffectPeg componentInChildren = gameObject2.GetComponentInChildren<EffectPeg>();
			componentInChildren.effectName = effectPegString;
			SpriteRenderer component2 = componentInChildren.GetComponent<SpriteRenderer>();
			component2.sprite = componentInChildren.GetSpriteForEffectName(effectPegString);
			component2.sortingOrder = orderingInLayer;
			SpriteRenderer component3 = component.GetComponentInChildren<RegularPegSlimeBehaviour>(includeInactive: true).GetComponent<SpriteRenderer>();
			component3.sortingOrder = orderingInLayer + 2;
			component3.GetComponentInChildren<SpriteRenderer>().sortingOrder = orderingInLayer + 3;
			return gameObject2.transform;
		}
	}

	[Serializable]
	public class DragonPegLayoutSimData : Component
	{
		public int totalPegCountAvg;

		public int totalPegCountVariance;

		public int simulationFrames;

		public float gridBottomLeftX;

		public float gridBottomLeftY;

		public int gridMaxSizeX;

		public int gridMaxSizeY;

		public float distanceBetweenPegs;

		public int extraSpawnsForPerGuaranteedSpawner;

		public int spawnAmountVarience;

		public int minExtraSpawners;

		public int maxExtraSpawners;

		public int minSpawerX;

		public int maxSpawerX;

		public int minXDistanceBetweenSpawners;

		public int minXDistanceFromGuarenteedSpawners;

		public int minSpawnerFramesPerSpawn;

		public int maxSpawnerFramesPerSpawn;

		public string pegPrefab;

		public DragonPegLayoutSimData(DragonPegLayoutSimulation c)
		{
			componentType = componentTypesMap[ComponentType.DragonPegLayoutSimulator];
			totalPegCountAvg = c.totalPegCountAvg;
			totalPegCountVariance = c.totalPegCountVarience;
			simulationFrames = c.simulationFrames;
			gridBottomLeftX = c.gridBottomLeft.x;
			gridBottomLeftY = c.gridBottomLeft.y;
			gridMaxSizeX = c.gridMaxSize.x;
			gridMaxSizeY = c.gridMaxSize.y;
			distanceBetweenPegs = c.distanceBetweenPegs;
			extraSpawnsForPerGuaranteedSpawner = c.extraSpawnsForPerGuarenteedSpawner;
			spawnAmountVarience = c.spawnAmountVarience;
			minExtraSpawners = c.minExtraSpawners;
			maxExtraSpawners = c.maxExtraSpawners;
			minSpawerX = c.minSpawerX;
			maxSpawerX = c.maxSpawerX;
			minXDistanceBetweenSpawners = c.minXDistanceBetweenSpawners;
			minXDistanceFromGuarenteedSpawners = c.minXDistanceFromGuarenteedSpawners;
			minSpawnerFramesPerSpawn = c.minSpawnerFramesPerSpawn;
			maxSpawnerFramesPerSpawn = c.maxSpawnerFramesPerSpawn;
			if (c.pegPrefab != null)
			{
				pegPrefab = c.pegPrefab.name;
			}
			enabled = c.enabled;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			DragonPegLayoutSimulation dragonPegLayoutSimulation = parent.gameObject.AddComponent<DragonPegLayoutSimulation>();
			dragonPegLayoutSimulation.totalPegCountAvg = totalPegCountAvg;
			dragonPegLayoutSimulation.totalPegCountVarience = totalPegCountVariance;
			dragonPegLayoutSimulation.simulationFrames = simulationFrames;
			dragonPegLayoutSimulation.gridBottomLeft = new Vector2(gridBottomLeftX, gridBottomLeftY);
			dragonPegLayoutSimulation.gridMaxSize = new Vector2Int(gridMaxSizeX, gridMaxSizeY);
			dragonPegLayoutSimulation.distanceBetweenPegs = distanceBetweenPegs;
			dragonPegLayoutSimulation.extraSpawnsForPerGuarenteedSpawner = extraSpawnsForPerGuaranteedSpawner;
			dragonPegLayoutSimulation.spawnAmountVarience = spawnAmountVarience;
			dragonPegLayoutSimulation.minExtraSpawners = minExtraSpawners;
			dragonPegLayoutSimulation.maxExtraSpawners = maxExtraSpawners;
			dragonPegLayoutSimulation.minSpawerX = minSpawerX;
			dragonPegLayoutSimulation.maxSpawerX = maxSpawerX;
			dragonPegLayoutSimulation.minXDistanceBetweenSpawners = minXDistanceBetweenSpawners;
			dragonPegLayoutSimulation.minXDistanceFromGuarenteedSpawners = minXDistanceFromGuarenteedSpawners;
			dragonPegLayoutSimulation.minSpawnerFramesPerSpawn = minSpawnerFramesPerSpawn;
			dragonPegLayoutSimulation.maxSpawnerFramesPerSpawn = maxSpawnerFramesPerSpawn;
			if (pegPrefab != null)
			{
				AssetReference assetReference = new AssetReference(pegPrefab);
				PegLayoutLoader.CheckAndSavePegboardAsset(assetReference);
				dragonPegLayoutSimulation.pegPrefab = (GameObject)PegLayoutLoader.pegboardAssets[assetReference.RuntimeKey.ToString()];
			}
			dragonPegLayoutSimulation.enabled = enabled;
			return parent;
		}
	}

	[Serializable]
	public class PegSquareMovementData : Component
	{
		public int startingCorner;

		public float oppositeCornerOffsetX;

		public float oppositeCornerOffsetY;

		public float edgeMoveSpeed;

		public float cornerDelay;

		public PegSquareMovementData(PegSquareMovement p)
		{
			componentType = componentTypesMap[ComponentType.PegSquareMovement];
			startingCorner = p.startingCorner;
			oppositeCornerOffsetX = p.oppositeCornerOffset.x;
			oppositeCornerOffsetY = p.oppositeCornerOffset.y;
			cornerDelay = p.cornerDelay;
			enabled = p.enabled;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegSquareMovement pegSquareMovement = parent.gameObject.AddComponent<PegSquareMovement>();
			pegSquareMovement.startingCorner = startingCorner;
			pegSquareMovement.oppositeCornerOffset = new Vector2(oppositeCornerOffsetX, oppositeCornerOffsetY);
			pegSquareMovement.cornerDelay = cornerDelay;
			parent.gameObject.AddComponent<Rigidbody2D>().isKinematic = true;
			PegSquareMovementManager component = parent.parent.gameObject.GetComponent<PegSquareMovementManager>();
			if (component == null)
			{
				parent.parent.gameObject.AddComponent<PegSquareMovementManager>().AddPegMover(pegSquareMovement);
			}
			else
			{
				component.AddPegMover(pegSquareMovement);
			}
			pegSquareMovement.enabled = enabled;
			return parent;
		}
	}

	[Serializable]
	public class RefreshingRevealsPegsData : Component, IPostProcessAction
	{
		public int pegsRevealedPerRefresh;

		public string initialPegName;

		private RefreshingRevealsPegsBehaviour _componentInstance;

		public RefreshingRevealsPegsData(RefreshingRevealsPegsBehaviour c)
		{
			pegsRevealedPerRefresh = c.pegsRevealedPerRefresh;
			initialPegName = c.initialPeg.gameObject.name;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			RefreshingRevealsPegsBehaviour refreshingRevealsPegsBehaviour = parent.gameObject.AddComponent<RefreshingRevealsPegsBehaviour>();
			refreshingRevealsPegsBehaviour.pegsRevealedPerRefresh = pegsRevealedPerRefresh;
			refreshingRevealsPegsBehaviour.initialPegName = initialPegName;
			_componentInstance = refreshingRevealsPegsBehaviour;
			return parent;
		}

		public void DoPostProcessAction(Transform root)
		{
			Peg[] componentsInChildren = root.GetComponentsInChildren<Peg>();
			foreach (Peg peg in componentsInChildren)
			{
				if (peg.gameObject.name == initialPegName)
				{
					_componentInstance.initialPeg = peg;
				}
			}
		}
	}

	[Serializable]
	public class PegLocationData : Component
	{
		public string prefabName;

		public PegLocationData(PegLocation c)
		{
			prefabName = c.pegPrefab.name;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegLocation pegLocation = parent.gameObject.AddComponent<PegLocation>();
			AssetReference assetReference = new AssetReference(prefabName);
			PegLayoutLoader.CheckAndSavePegboardAsset(assetReference);
			pegLocation.pegPrefab = (GameObject)PegLayoutLoader.pegboardAssets[assetReference.RuntimeKey.ToString()];
			return parent;
		}
	}

	[Serializable]
	public class PaintingBoardSectionData : Component
	{
		public int[] indexNumbers;

		public Vector2 boxColliderSize;

		public Vector2 boxColliderOffset;

		public PaintingBoardSectionData(PaintingBoardSection c)
		{
			indexNumbers = c.forPaintingInSlotNumbers.ToArray();
			boxColliderSize = c.sectionCollider.size;
			boxColliderOffset = c.sectionCollider.offset;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PaintingBoardSection paintingBoardSection = parent.gameObject.AddComponent<PaintingBoardSection>();
			BoxCollider2D boxCollider2D = parent.gameObject.AddComponent<BoxCollider2D>();
			paintingBoardSection.forPaintingInSlotNumbers = new List<int>(indexNumbers);
			paintingBoardSection.sectionCollider = boxCollider2D;
			boxCollider2D.isTrigger = true;
			boxCollider2D.size = boxColliderSize;
			boxCollider2D.offset = boxColliderOffset;
			return parent;
		}
	}

	[Serializable]
	public class PegLayoutAlternatorData : Component, IPostProcessAction
	{
		public string pegboardAName;

		public string pegboardBName;

		private PegLayoutAlternator instance;

		public PegLayoutAlternatorData(PegLayoutAlternator c)
		{
			pegboardAName = c._pegboardA.name;
			pegboardBName = c._pegboardB.name;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			instance = parent.gameObject.AddComponent<PegLayoutAlternator>();
			return parent;
		}

		public void DoPostProcessAction(Transform root)
		{
			if (instance == null)
			{
				Debug.LogError("Couldn't find the PegLayoutAlternator in our list of objects");
				return;
			}
			bool flag = false;
			bool flag2 = false;
			foreach (Transform item in root)
			{
				if (item.name == pegboardAName)
				{
					instance._pegboardA = item.gameObject;
					flag = true;
				}
				else if (item.name == pegboardBName)
				{
					instance._pegboardB = item.gameObject;
					flag2 = true;
				}
				if (flag & flag2)
				{
					break;
				}
			}
		}
	}

	[Serializable]
	public class PegLayoutHolderData : Component
	{
		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			parent.gameObject.AddComponent<PegLayoutHolder>();
			return parent;
		}
	}

	[Serializable]
	public class PegSpawnPositionData : Component
	{
		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			parent.gameObject.AddComponent<PegSpawnPosition>();
			return parent;
		}
	}

	[Serializable]
	public class PegSplineFollowData : Component
	{
		public string[] pegPrefabs;

		public float[] pegTypeSpawnRatio;

		public float pegDencity;

		public float pegMoveSpeed;

		public int numSamplesPerBezier;

		public Spline spline;

		private static AssetReference PegSplineFollowGeneratorAssetRef = new AssetReference("PegSplineFollowGenerator");

		public PegSplineFollowData(PegSplineFollow c)
		{
			pegPrefabs = new string[c.pegPrefabs.Length];
			for (int i = 0; i < c.pegPrefabs.Length; i++)
			{
				pegPrefabs[i] = c.pegPrefabs[i]?.name;
			}
			pegTypeSpawnRatio = c.pegTypeSpawnRatio;
			pegDencity = c.pegDencity;
			pegMoveSpeed = c.pegMoveSpeed;
			numSamplesPerBezier = c.numSamplesPerBezier;
			spline = c.GetComponent<SpriteShapeController>().spline;
		}

		public override Transform CreateInstance(TransformData td, Transform parent)
		{
			PegLayoutLoader.CheckAndSavePegboardAsset(PegSplineFollowGeneratorAssetRef);
			GameObject gameObject = (GameObject)PegLayoutLoader.pegboardAssets[PegSplineFollowGeneratorAssetRef.RuntimeKey.ToString()];
			Vector3 position = new Vector3(td.posX, td.posY);
			Vector3 localScale = new Vector3(td.scaleX, td.scaleY);
			if (gameObject == null)
			{
				return Component.CreateEmptyTransform(td);
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, parent);
			gameObject2.transform.localScale = localScale;
			gameObject2.transform.name = td.name;
			gameObject2.SetActive(td.enabled);
			PegSplineFollow component = gameObject2.GetComponent<PegSplineFollow>();
			SpriteShapeController component2 = gameObject2.GetComponent<SpriteShapeController>();
			component2.spline.Clear();
			component2.spline.isOpenEnded = spline.isOpenEnded;
			for (int i = 0; i < spline.GetPointCount(); i++)
			{
				component2.spline.InsertPointAt(i, spline.GetPosition(i));
				component2.spline.SetTangentMode(i, spline.GetTangentMode(i));
				component2.spline.SetCorner(i, spline.GetCorner(i));
				component2.spline.SetHeight(i, spline.GetHeight(i));
				component2.spline.SetLeftTangent(i, spline.GetLeftTangent(i));
				component2.spline.SetRightTangent(i, spline.GetRightTangent(i));
				component2.spline.SetSpriteIndex(i, spline.GetSpriteIndex(i));
			}
			Peg[] array = new Peg[pegPrefabs.Length];
			for (int j = 0; j < pegPrefabs.Length; j++)
			{
				if (pegPrefabs[j] != null)
				{
					AssetReference assetReference = new AssetReference(pegPrefabs[j]);
					PegLayoutLoader.CheckAndSavePegboardAsset(assetReference);
					GameObject gameObject3 = (GameObject)PegLayoutLoader.pegboardAssets[assetReference.RuntimeKey.ToString()];
					if (gameObject3 != null)
					{
						array[j] = gameObject3.GetComponent<Peg>();
					}
				}
			}
			component.pegPrefabs = array;
			component.pegTypeSpawnRatio = pegTypeSpawnRatio;
			component.pegDencity = pegDencity;
			component.pegMoveSpeed = pegMoveSpeed;
			component.numSamplesPerBezier = numSamplesPerBezier;
			return component.transform;
		}
	}

	public static readonly Dictionary<ComponentType, string> componentTypesMap = new Dictionary<ComponentType, string>
	{
		{
			ComponentType.RegularPeg,
			"peg_regular"
		},
		{
			ComponentType.BezierMeshGenerator,
			"peg_long"
		},
		{
			ComponentType.SlimeOnlyPeg,
			"peg_slime_only"
		},
		{
			ComponentType.Bomb,
			"peg_bomb"
		},
		{
			ComponentType.IndestructiblePeg,
			"indestructible_peg"
		},
		{
			ComponentType.BouncerPeg,
			"bouncer_peg"
		},
		{
			ComponentType.EffectPeg,
			"effect_peg"
		},
		{
			ComponentType.InvisibleOnAwake,
			"attr_invisible_on_awake"
		},
		{
			ComponentType.FlickeringPeg,
			"attr_flickering_peg"
		},
		{
			ComponentType.LinearPegMovement,
			"attr_moving_peg_linear"
		},
		{
			ComponentType.MirroredPeg,
			"attr_mirrored_peg"
		},
		{
			ComponentType.OffsetPositionOnStart,
			"attr_offset_pos_on_start"
		},
		{
			ComponentType.PegMoveAndReturn,
			"attr_moving_peg_return"
		},
		{
			ComponentType.PegBlock,
			"parent_peg_block"
		},
		{
			ComponentType.RandomPegField,
			"parent_random_peg_field"
		},
		{
			ComponentType.PegTeleport,
			"parent_random_peg_field"
		},
		{
			ComponentType.RotatingPegCircle,
			"parent_rotating_peg_circle"
		},
		{
			ComponentType.FireworkMovement,
			"parent_firework_movement"
		},
		{
			ComponentType.FlickeringPegLoopCreator,
			"parent_flickering_loop"
		},
		{
			ComponentType.PegboardBlackHole,
			"obstacle_black_hole"
		},
		{
			ComponentType.RandomObscuredPegGrid,
			"obstacle_obscured_peg_grid"
		},
		{
			ComponentType.Bouncer,
			"obstacle_bouncer"
		},
		{
			ComponentType.BouncerMines,
			"obstacle_bouncer_mines"
		},
		{
			ComponentType.DragonPegLayoutSimulator,
			"dragon_peg_layout_simulator "
		},
		{
			ComponentType.PegSquareMovement,
			"peg_square_movement"
		},
		{
			ComponentType.DisappearForNavigation,
			"disappear_nav"
		},
		{
			ComponentType.BoidsPegLayout,
			"boids_peg_layout"
		},
		{
			ComponentType.PegSplineFollow,
			"peg_spline_follow"
		}
	};

	public void ParsePegboard(GameObject pegboard)
	{
		PegboardData pegboardData = new PegboardData();
		pegboardData.name = pegboard.name;
		pegboardData.posX = pegboard.transform.position.x;
		pegboardData.posY = pegboard.transform.position.y;
		TransformData transformData = new TransformData(pegboard.transform);
		pegboardData.transforms.Add(transformData);
		ScanForParentTransformComponents(pegboard.transform, transformData);
		if (pegboard.TryGetComponent<PegLayoutAlternator>(out var component))
		{
			component._pegboardA.SetActive(value: true);
			component._pegboardB.SetActive(value: true);
		}
		ParseRecursive(pegboard, pegboardData, transformData);
		byte[] bytes = SerializationUtility.SerializeValue(pegboardData, DataFormat.Binary);
		File.WriteAllBytes(Path.Combine(Application.dataPath, "Data/Pegboards/" + pegboard.name + ".bytes"), bytes);
	}

	public void ParseRecursive(GameObject obj, PegboardData pegboardData, TransformData parent, bool parentIsLongPeg = false)
	{
		if (obj == null)
		{
			return;
		}
		foreach (Transform item in obj.transform)
		{
			if (item == null)
			{
				continue;
			}
			TransformData transformData = new TransformData(item);
			parent?.child.Add(transformData);
			if (ScanForPegComponents(item, transformData))
			{
				if (parentIsLongPeg)
				{
					Debug.LogWarning("Found regular pegs parented to long pegs in object " + obj.name + ", peg layout " + pegboardData.name);
				}
				ScanForPegAttributes(item, transformData);
				if (transformData.prefab is LongPegData)
				{
					ParseRecursive(item.gameObject, pegboardData, transformData, parentIsLongPeg: true);
				}
			}
			else
			{
				ScanForParentTransformComponents(item, transformData);
				ScanForPegHazards(item, transformData);
				ParseRecursive(item.gameObject, pegboardData, transformData);
			}
		}
	}

	private bool ScanForPegComponents(Transform t, TransformData td)
	{
		FireworkMovement component = t.gameObject.GetComponent<FireworkMovement>();
		if (component != null)
		{
			td.prefab = new FireworkMovementData(component);
			return true;
		}
		if (t.TryGetComponent<Bouncer>(out var component2))
		{
			if (component2.useMinesPrefab)
			{
				td.prefab = new BouncerMinesData(component2);
			}
			else
			{
				td.prefab = new BouncerData(component2);
			}
			return true;
		}
		BezierMeshGenerator component3 = t.gameObject.GetComponent<BezierMeshGenerator>();
		if (component3 != null)
		{
			td.prefab = new LongPegData(component3);
			return true;
		}
		if (t.gameObject.GetComponent<SlimeOnlyPeg>() != null)
		{
			td.prefab = new SlimeOnlyPegData();
			return true;
		}
		Bomb component4 = t.gameObject.GetComponent<Bomb>();
		if (component4 != null)
		{
			td.prefab = new BombData(component4);
			return true;
		}
		IndestructiblePeg component5 = t.gameObject.GetComponent<IndestructiblePeg>();
		if (component5 != null)
		{
			td.prefab = new IndestructiblePegData(component5);
			return true;
		}
		BouncerPeg component6 = t.gameObject.GetComponent<BouncerPeg>();
		if (component6 != null && component6.GetComponentInChildren<EffectPeg>() == null)
		{
			td.prefab = new BouncerPegData(component6);
			return true;
		}
		if (component6 != null)
		{
			EffectPeg componentInChildren = component6.GetComponentInChildren<EffectPeg>();
			if (componentInChildren != null)
			{
				td.prefab = new EffectBouncerPegData(componentInChildren);
				return true;
			}
		}
		RegularPeg component7 = t.gameObject.GetComponent<RegularPeg>();
		if (component7 != null)
		{
			FakePeg component8 = t.gameObject.GetComponent<FakePeg>();
			td.prefab = new RegularPegData(component7, component8 != null);
			return true;
		}
		return false;
	}

	private void ScanForPegAttributes(Transform t, TransformData td)
	{
		InvisibleOnAwake component = t.gameObject.GetComponent<InvisibleOnAwake>();
		if (component != null)
		{
			td.components.Add(new InvisibleOnAwakeData(component));
		}
		FlickeringPeg component2 = t.gameObject.GetComponent<FlickeringPeg>();
		if (component2 != null)
		{
			td.components.Add(new FlickeringPegData(component2));
		}
		LinearPegMovement component3 = t.gameObject.GetComponent<LinearPegMovement>();
		if (component3 != null)
		{
			td.components.Add(new LinearPegMovementData(component3));
		}
		MirroredPeg component4 = t.gameObject.GetComponent<MirroredPeg>();
		if (component4 != null)
		{
			td.components.Add(new MirroredPegData(component4));
		}
		OffsetPositionOnStart component5 = t.gameObject.GetComponent<OffsetPositionOnStart>();
		if ((bool)component5)
		{
			td.components.Add(new OffsetPositionOnStartData(component5));
		}
	}

	private void ScanForParentTransformComponents(Transform t, TransformData td)
	{
		LinearPegMovement component = t.gameObject.GetComponent<LinearPegMovement>();
		if (component != null)
		{
			td.components.Add(new LinearPegMovementData(component));
		}
		PegBlock component2 = t.gameObject.GetComponent<PegBlock>();
		if (component2 != null)
		{
			td.components.Add(new PegBlockData(component2));
		}
		RandomPegField component3 = t.gameObject.GetComponent<RandomPegField>();
		if (component3 != null)
		{
			td.components.Add(new RandomPegFieldData(component3));
		}
		PegTeleport component4 = t.gameObject.GetComponent<PegTeleport>();
		if (component4 != null)
		{
			td.components.Add(new PegTeleportData(component4));
		}
		RotatingPegCircle component5 = t.gameObject.GetComponent<RotatingPegCircle>();
		if (component5 != null)
		{
			td.components.Add(new RotatingPegCircleData(component5));
		}
		PegMoveAndReturn component6 = t.gameObject.GetComponent<PegMoveAndReturn>();
		if (component6 != null)
		{
			td.components.Add(new PegMoveAndReturnData(component6));
		}
		FlickeringPegLoopCreator component7 = t.gameObject.GetComponent<FlickeringPegLoopCreator>();
		if (component7 != null)
		{
			td.components.Add(new FlickeringPegLoopCreatorData(component7));
		}
		DragonPegLayoutSimulation component8 = t.gameObject.GetComponent<DragonPegLayoutSimulation>();
		if (component8 != null)
		{
			td.components.Add(new DragonPegLayoutSimData(component8));
		}
		PegSquareMovement component9 = t.gameObject.GetComponent<PegSquareMovement>();
		if (component9 != null)
		{
			td.components.Add(new PegSquareMovementData(component9));
		}
		if (t.gameObject.GetComponent<RandomObscuredPegGrid>() != null)
		{
			td.components.Add(new RandomObscuredPegGridData());
		}
		if (t.TryGetComponent<BoidsPegLayout>(out var component10))
		{
			td.components.Add(new BoidsPegLayoutData(component10));
		}
		RefreshingRevealsPegsBehaviour component11 = t.gameObject.GetComponent<RefreshingRevealsPegsBehaviour>();
		if (component11 != null)
		{
			td.components.Add(new RefreshingRevealsPegsData(component11));
		}
		PegLocation component12 = t.gameObject.GetComponent<PegLocation>();
		if (component12 != null)
		{
			td.components.Add(new PegLocationData(component12));
		}
		PegLayoutAlternator component13 = t.gameObject.GetComponent<PegLayoutAlternator>();
		if (component13 != null)
		{
			td.components.Add(new PegLayoutAlternatorData(component13));
		}
		MirroredPeg component14 = t.gameObject.GetComponent<MirroredPeg>();
		if (component14 != null)
		{
			td.components.Add(new MirroredPegData(component14));
		}
		if (t.gameObject.GetComponent<PegSpawnPosition>() != null)
		{
			td.components.Add(new PegSpawnPositionData());
		}
		PaintingBoardSection component15 = t.gameObject.GetComponent<PaintingBoardSection>();
		if (component15 != null)
		{
			td.components.Add(new PaintingBoardSectionData(component15));
		}
		ScenarioNavigationBonusChest component16 = t.gameObject.GetComponent<ScenarioNavigationBonusChest>();
		if (component16 != null)
		{
			td.components.Add(new ScenarioNavigationBonusChestData(component16));
		}
		if (t.gameObject.GetComponent<PegLayoutHolder>() != null)
		{
			td.components.Add(new PegLayoutHolderData());
		}
		PegSplineFollow component17 = t.gameObject.GetComponent<PegSplineFollow>();
		if (component17 != null)
		{
			td.components.Add(new PegSplineFollowData(component17));
		}
	}

	private void ScanForPegHazards(Transform t, TransformData td)
	{
		PegboardBlackHole component = t.gameObject.GetComponent<PegboardBlackHole>();
		if (component != null)
		{
			td.prefab = new PegboardBlackHoleData(component);
		}
	}
}
