using System;
using System.Collections.Generic;
using System.Diagnostics;
using Battle.Pachinko.Obstacles;
using Battle.PegBehaviour;
using DG.Tweening;
using PeglinUtils;
using Relics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class PredictionManager : MonoBehaviour
{
	public enum PredictionResolutionMode
	{
		NORMAL = 0,
		LOW = 1
	}

	public struct StaticPredictionLineData
	{
		public LineRenderer lineRenderer;

		public GameObject orb;

		public Vector3 startingPosition;

		public Vector3 firingForce;

		public bool multiballOnFiredBecomeSmaller;

		public Guid guid;

		public StaticPredictionLineData(LineRenderer lineRenderer, GameObject orb, Vector3 startingPosition, Vector3 firingForce, bool multiballOnFiredBecomeSmaller)
		{
			guid = Guid.NewGuid();
			this.lineRenderer = lineRenderer;
			this.orb = orb;
			this.startingPosition = startingPosition;
			this.firingForce = firingForce;
			this.multiballOnFiredBecomeSmaller = multiballOnFiredBecomeSmaller;
		}
	}

	private class CurrentPachinkoInfo
	{
		public PachinkoBall pachinko;

		public Rigidbody2D rigidbody;

		public PierceBehavior[] pierce;

		public IUpdateableBySimulation[] updateableBySimulation;

		public bool smallerOrbForPrediction;

		public float originalMass;

		public float originalDrag;

		public float originalGravityScale;

		public float originalInertia;

		public Vector3 origionalScale;

		public void SetPachinkoInfo(PachinkoBall p, bool smallerOrbForPrediction)
		{
			pachinko = p;
			rigidbody = p.GetComponent<Rigidbody2D>();
			pierce = p.GetComponents<PierceBehavior>();
			updateableBySimulation = p.GetComponentsInChildren<IUpdateableBySimulation>();
			this.smallerOrbForPrediction = smallerOrbForPrediction;
			originalMass = rigidbody.mass;
			originalGravityScale = p.GravityScale;
			originalDrag = rigidbody.drag;
			originalInertia = rigidbody.inertia;
			origionalScale = p.gameObject.transform.localScale;
		}

		public void SetPachinkoInfo(CurrentPachinkoInfo info)
		{
			pachinko = info.pachinko;
			rigidbody = info.rigidbody;
			pierce = info.pierce;
			updateableBySimulation = info.updateableBySimulation;
			smallerOrbForPrediction = info.smallerOrbForPrediction;
			originalMass = rigidbody.mass;
			originalGravityScale = info.originalGravityScale;
			originalDrag = rigidbody.drag;
			origionalScale = info.origionalScale;
		}

		public void ResetPhysicsAttributes()
		{
			rigidbody.mass = originalMass;
			rigidbody.gravityScale = originalGravityScale;
			rigidbody.drag = originalDrag;
			rigidbody.inertia = originalInertia;
			pachinko.gameObject.transform.localScale = origionalScale;
		}
	}

	public static Action RequestSimulationPegboardUpdate;

	private const int MIN_ITERATIONS = 2;

	public static Action<List<IDummyPeg>> RequestSpecificPegsUpdate;

	public PredictionResolutionMode ResolutionMode;

	[SerializeField]
	private int _baseMaxIterations = 35;

	private int _maxIterations;

	[SerializeField]
	private float _additionalBounceOffset = 0.15f;

	[SerializeField]
	private GameObject BounceIndicatorPrefab;

	[SerializeField]
	private GameObject DestroyedIndicatorPrefab;

	[SerializeField]
	private GameObject StaticLineRendererPrefab;

	[SerializeField]
	private Material SolidLineMaterial;

	[SerializeField]
	private Material SolidStaticLineMaterial;

	private List<GameObject> _dummyPegs = new List<GameObject>();

	private GameObject _pegLayout;

	private GameObject _dummy;

	private LineRenderer _lineRenderer;

	private List<StaticPredictionLineData> _staticPredictionLinesData = new List<StaticPredictionLineData>();

	private Scene _currentScene;

	private Scene _predictionScene;

	private PhysicsScene2D _currentPhysicsScene;

	private PhysicsScene2D _predictionPhysicsScene;

	private GameObject _pegBoard;

	private GameObject _fakePegboard;

	private ObjectPool _bounceIndicatorObjectPool;

	private ObjectPool _destroyedIndicatorObjectPool;

	private List<GameObject> _bounceIndicatorList = new List<GameObject>();

	private List<GameObject> _destroyedIndicatorList = new List<GameObject>();

	private int _bounceCount = 1;

	private Material _scrollingMaterial;

	private List<Material> _staticScrollingMaterials = new List<Material>();

	private RelicManager _relicManager;

	private Dictionary<IDummiableMovingPeg, IDummiableMovingPeg> _movingPegs = new Dictionary<IDummiableMovingPeg, IDummiableMovingPeg>();

	private Dictionary<IDummiableLayerChanger, IDummiableLayerChanger> _layerChangers = new Dictionary<IDummiableLayerChanger, IDummiableLayerChanger>();

	private Dictionary<IDummiablePegTypeChanger, IDummiablePegTypeChanger> _pegTypeChangers = new Dictionary<IDummiablePegTypeChanger, IDummiablePegTypeChanger>();

	private Dictionary<IDummiableMovingPegEndOfTurn, IDummiableMovingPegEndOfTurn> _movingPegsEndOfTurn = new Dictionary<IDummiableMovingPegEndOfTurn, IDummiableMovingPegEndOfTurn>();

	private Dictionary<GameObject, GameObject> _addedObjects = new Dictionary<GameObject, GameObject>();

	private Dictionary<GameObject, GameObject> _addedUP = new Dictionary<GameObject, GameObject>();

	private Dictionary<IDummyPeg, IDummyPeg> _allPegs = new Dictionary<IDummyPeg, IDummyPeg>();

	private List<IUpdateableBySimulation> _updateableSceneObjs = new List<IUpdateableBySimulation>();

	private List<IUpdateableBySimulation> _dynamicallyAddedUpdateableSceneObjs = new List<IUpdateableBySimulation>();

	private CurrentPachinkoInfo currentPachinkoInfo = new CurrentPachinkoInfo();

	private Guid _chainsMultiballPredictionLineDataId;

	private Guid _chainsMultiballPredictionLineDataId2;

	private GameObject _simulationPegHolder;

	private bool _navigationOrb;

	private bool _shortSolidLineMode;

	private Stopwatch _stopwatch = new Stopwatch();

	private void Start()
	{
		Physics2D.simulationMode = SimulationMode2D.Script;
		_currentScene = SceneManager.GetActiveScene();
		_currentPhysicsScene = _currentScene.GetPhysicsScene2D();
		CreateSceneParameters parameters = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
		_predictionScene = SceneManager.CreateScene("Prediction", parameters);
		_predictionPhysicsScene = _predictionScene.GetPhysicsScene2D();
		_lineRenderer = GetComponent<LineRenderer>();
		if (_pegBoard != null)
		{
			CreateFakePegboard();
		}
		if (_bounceIndicatorObjectPool == null)
		{
			_bounceIndicatorObjectPool = base.gameObject.AddComponent<ObjectPool>();
			_bounceIndicatorObjectPool.CreatePool(10, BounceIndicatorPrefab);
		}
		if (_destroyedIndicatorObjectPool == null && DestroyedIndicatorPrefab != null)
		{
			_destroyedIndicatorObjectPool = base.gameObject.AddComponent<ObjectPool>();
			_destroyedIndicatorObjectPool.CreatePool(10, DestroyedIndicatorPrefab);
		}
		_scrollingMaterial = _lineRenderer.material;
	}

	private void OnEnable()
	{
		RequestSimulationPegboardUpdate = (Action)Delegate.Combine(RequestSimulationPegboardUpdate, new Action(UpdateAllPegsStatus));
		RequestSpecificPegsUpdate = (Action<List<IDummyPeg>>)Delegate.Combine(RequestSpecificPegsUpdate, new Action<List<IDummyPeg>>(UpdateSpecificPegs));
		Peg.OnPegAdded = (Peg.PegAdded)Delegate.Combine(Peg.OnPegAdded, new Peg.PegAdded(CopyPegToSimulation));
		Peg.OnPegDestroyed = (Peg.PegDestroyed)Delegate.Combine(Peg.OnPegDestroyed, new Peg.PegDestroyed(RemovePegFromSimulation));
	}

	private void OnDisable()
	{
		RequestSimulationPegboardUpdate = (Action)Delegate.Remove(RequestSimulationPegboardUpdate, new Action(UpdateAllPegsStatus));
		RequestSpecificPegsUpdate = (Action<List<IDummyPeg>>)Delegate.Remove(RequestSpecificPegsUpdate, new Action<List<IDummyPeg>>(UpdateSpecificPegs));
		Peg.OnPegAdded = (Peg.PegAdded)Delegate.Remove(Peg.OnPegAdded, new Peg.PegAdded(CopyPegToSimulation));
		Peg.OnPegDestroyed = (Peg.PegDestroyed)Delegate.Remove(Peg.OnPegDestroyed, new Peg.PegDestroyed(RemovePegFromSimulation));
	}

	public void Initialize(GameObject pegLayout, GameObject pegboard, RelicManager relicManager, bool navigation)
	{
		_pegLayout = pegLayout;
		_pegBoard = pegboard;
		CreateFakePegboard();
		_relicManager = relicManager;
		_navigationOrb = navigation;
		if (Application.isEditor)
		{
			_relicManager.AttemptUseRelic(RelicEffect.LONGER_AIMER);
		}
		_maxIterations = _baseMaxIterations;
		if (relicManager.RelicEffectActive(RelicEffect.LONGER_AIMER))
		{
			_maxIterations = _baseMaxIterations * 2;
		}
		_shortSolidLineMode = false;
		if (_maxIterations <= 2)
		{
			if (_lineRenderer == null)
			{
				_lineRenderer = GetComponent<LineRenderer>();
			}
			_shortSolidLineMode = true;
			_lineRenderer.material = SolidLineMaterial;
		}
		_bounceCount = ((!relicManager.RelicEffectActive(RelicEffect.LONGER_AIMER)) ? 1 : 2);
	}

	public void SetNavigation(bool navigation)
	{
		_navigationOrb = navigation;
	}

	public void SetLowResolutionMode(PredictionResolutionMode value)
	{
		ResolutionMode = value;
	}

	private void Update()
	{
		if (_lineRenderer.enabled && _scrollingMaterial != null)
		{
			_scrollingMaterial.mainTextureOffset = -(Vector2.one * Mathf.Repeat(Time.time, 1f));
		}
		foreach (Material staticScrollingMaterial in _staticScrollingMaterials)
		{
			staticScrollingMaterial.mainTextureOffset = -(Vector2.one * Mathf.Repeat(Time.time, 1f));
		}
	}

	private void FixedUpdate()
	{
		if (_currentPhysicsScene.IsValid())
		{
			_currentPhysicsScene.Simulate(Time.fixedDeltaTime);
		}
	}

	public void CopyAllPegs()
	{
		KillAllPegs();
		CopyChildren(_pegLayout.transform);
	}

	public void UpdateAllPegsStatus()
	{
		foreach (IDummyPeg key in _allPegs.Keys)
		{
			_allPegs[key].SetPegStatus(key.GetPegStatus());
			_allPegs[key].SetSpecialCollider(key.IsSpecialPeg());
			_allPegs[key].SetPegType(key.GetPegType());
			if (key is Bomb bomb)
			{
				Bomb bomb2 = _allPegs[bomb] as Bomb;
				if (bomb2 != null)
				{
					bomb2.HitCount = bomb.HitCount;
					bomb2.SetPhysicsMaterial(bomb);
				}
			}
		}
		foreach (IDummiableMovingPegEndOfTurn key2 in _movingPegsEndOfTurn.Keys)
		{
			_movingPegsEndOfTurn[key2].SetPosition(key2.GetPosition());
		}
	}

	public void ReenableDisabledUpdateables()
	{
		foreach (IUpdateableBySimulation updateableSceneObj in _updateableSceneObjs)
		{
			updateableSceneObj.EnableIfDisabledDuringShot();
		}
	}

	public void UpdateSpecificPegs(List<IDummyPeg> pegs)
	{
		foreach (IDummyPeg peg in pegs)
		{
			_allPegs[peg].SetPegStatus(peg.GetPegStatus());
			_allPegs[peg].SetSpecialCollider(peg.IsSpecialPeg());
			_allPegs[peg].SetPegType(peg.GetPegType());
		}
	}

	private void CopyChildren(Transform trans)
	{
		_pegTypeChangers.Clear();
		List<IDummyPeg> list = SceneUtils.Find<IDummyPeg>();
		foreach (IDummyPeg item in list)
		{
			item.GenerateId();
		}
		List<IDummiablePegTypeChanger> list2 = SceneUtils.Find<IDummiablePegTypeChanger>();
		List<IDummiableMovingPeg> list3 = SceneUtils.Find<IDummiableMovingPeg>();
		List<IDummiableLayerChanger> list4 = SceneUtils.Find<IDummiableLayerChanger>();
		List<IDummiableMovingPegEndOfTurn> list5 = SceneUtils.Find<IDummiableMovingPegEndOfTurn>();
		_simulationPegHolder = UnityEngine.Object.Instantiate(trans.gameObject, trans.position, trans.rotation);
		NonDummyPegboardBehaviour[] componentsInChildren = _simulationPegHolder.GetComponentsInChildren<NonDummyPegboardBehaviour>();
		List<LongPeg> list6 = SceneUtils.Find<LongPeg>();
		IDummiablePegTypeChanger[] componentsInChildren2 = _simulationPegHolder.GetComponentsInChildren<IDummiablePegTypeChanger>();
		foreach (IDummiablePegTypeChanger dummiablePegTypeChanger in componentsInChildren2)
		{
			IDummiablePegTypeChanger dummiablePegTypeChanger2 = null;
			foreach (IDummiablePegTypeChanger item2 in list2)
			{
				if (item2.GetId() == dummiablePegTypeChanger.GetId())
				{
					dummiablePegTypeChanger2 = item2;
					break;
				}
			}
			if (dummiablePegTypeChanger2 == null)
			{
				UnityEngine.Debug.LogWarning("Did not find parent long peg in original set");
			}
			else if (!_pegTypeChangers.ContainsKey(dummiablePegTypeChanger2))
			{
				dummiablePegTypeChanger.SetDummyStatus(status: true);
				_pegTypeChangers.Add(dummiablePegTypeChanger2, dummiablePegTypeChanger);
			}
		}
		NonDummyPegboardBehaviour[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].isDummy = true;
		}
		int num = 0;
		IDummyPeg[] componentsInChildren3 = _simulationPegHolder.GetComponentsInChildren<IDummyPeg>(includeInactive: true);
		foreach (IDummyPeg dummyPeg in componentsInChildren3)
		{
			num++;
			IDummyPeg dummyPeg2 = null;
			foreach (IDummyPeg item3 in list)
			{
				if (item3.GetId() == dummyPeg.GetId())
				{
					dummyPeg2 = item3;
					break;
				}
			}
			if (dummyPeg2 == null)
			{
				UnityEngine.Debug.LogWarning("Did not find parent peg in original set");
			}
			else if (!_allPegs.ContainsKey(dummyPeg2))
			{
				dummyPeg.SetDummyStatus(status: true);
				_allPegs.Add(dummyPeg2, dummyPeg);
			}
		}
		IDummiableMovingPeg[] componentsInChildren4 = _simulationPegHolder.GetComponentsInChildren<IDummiableMovingPeg>(includeInactive: true);
		foreach (IDummiableMovingPeg dummiableMovingPeg in componentsInChildren4)
		{
			IDummiableMovingPeg dummiableMovingPeg2 = null;
			foreach (IDummiableMovingPeg item4 in list3)
			{
				if (item4.GetId() == dummiableMovingPeg.GetId())
				{
					dummiableMovingPeg2 = item4;
					break;
				}
			}
			if (dummiableMovingPeg2 == null)
			{
				UnityEngine.Debug.LogWarning("Did not find parent game object for parametrized animation");
			}
			else if (!_movingPegs.ContainsKey(dummiableMovingPeg2))
			{
				dummiableMovingPeg.SetDummyStatus(status: true);
				_movingPegs.Add(dummiableMovingPeg2, dummiableMovingPeg);
			}
		}
		IDummiableMovingPegEndOfTurn[] componentsInChildren5 = _simulationPegHolder.GetComponentsInChildren<IDummiableMovingPegEndOfTurn>(includeInactive: true);
		foreach (IDummiableMovingPegEndOfTurn dummiableMovingPegEndOfTurn in componentsInChildren5)
		{
			IDummiableMovingPegEndOfTurn dummiableMovingPegEndOfTurn2 = null;
			foreach (IDummiableMovingPegEndOfTurn item5 in list5)
			{
				if (item5.GetId() == dummiableMovingPegEndOfTurn.GetId())
				{
					dummiableMovingPegEndOfTurn2 = item5;
					break;
				}
			}
			if (dummiableMovingPegEndOfTurn2 == null)
			{
				UnityEngine.Debug.LogWarning("Did not find parent game object for parametrized animation");
			}
			else if (!_movingPegsEndOfTurn.ContainsKey(dummiableMovingPegEndOfTurn2))
			{
				dummiableMovingPegEndOfTurn.SetDummyStatus(status: true);
				_movingPegsEndOfTurn.Add(dummiableMovingPegEndOfTurn2, dummiableMovingPegEndOfTurn);
			}
		}
		IDummiableLayerChanger[] componentsInChildren6 = _simulationPegHolder.GetComponentsInChildren<IDummiableLayerChanger>(includeInactive: true);
		foreach (IDummiableLayerChanger dummiableLayerChanger in componentsInChildren6)
		{
			IDummiableLayerChanger dummiableLayerChanger2 = null;
			foreach (IDummiableLayerChanger item6 in list4)
			{
				if (item6.GetId() == dummiableLayerChanger.GetId())
				{
					dummiableLayerChanger2 = item6;
					break;
				}
			}
			if (dummiableLayerChanger2 == null)
			{
				UnityEngine.Debug.LogWarning("Did not find parent game object for parametrized animation - Collision Toggles");
			}
			else if (!_layerChangers.ContainsKey(dummiableLayerChanger2))
			{
				dummiableLayerChanger.SetDummyStatus(status: true);
				_layerChangers.Add(dummiableLayerChanger2, dummiableLayerChanger);
			}
		}
		LongPeg[] componentsInChildren7 = _simulationPegHolder.GetComponentsInChildren<LongPeg>();
		foreach (LongPeg longPeg in componentsInChildren7)
		{
			foreach (LongPeg item7 in list6)
			{
				if (item7.id == longPeg.id)
				{
					longPeg.Initialize(item7);
				}
			}
		}
		_updateableSceneObjs.Clear();
		IUpdateableBySimulation[] componentsInChildren8 = _simulationPegHolder.GetComponentsInChildren<IUpdateableBySimulation>(includeInactive: true);
		for (int i = 0; i < componentsInChildren8.Length; i++)
		{
			componentsInChildren8[i].SetDummy(value: true);
		}
		_updateableSceneObjs.AddRange(_simulationPegHolder.GetComponentsInChildren<IUpdateableBySimulation>(includeInactive: true));
		SceneManager.MoveGameObjectToScene(_simulationPegHolder, _predictionScene);
		DeactivateRenderers(_simulationPegHolder);
		DeactivatePrediction(_simulationPegHolder);
		Peg[] componentsInChildren9 = _simulationPegHolder.GetComponentsInChildren<Peg>();
		foreach (Peg peg in componentsInChildren9)
		{
			if (peg.numCoinOverride > 0)
			{
				peg.numCoinOverride = 0;
			}
		}
		_dummyPegs.Add(_simulationPegHolder);
	}

	private void DeactivatePrediction(GameObject g)
	{
		DisableFromPrediction[] componentsInChildren = g.GetComponentsInChildren<DisableFromPrediction>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
		}
	}

	private void DeactivateRenderers(GameObject g)
	{
		Renderer[] componentsInChildren = g.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer is SpriteShapeRenderer)
			{
				renderer.DOKill();
				renderer.enabled = false;
			}
			else if (renderer is TilemapRenderer)
			{
				renderer.enabled = false;
			}
			else
			{
				renderer.DOKill();
				UnityEngine.Object.Destroy(renderer);
			}
		}
		Graphic[] componentsInChildren2 = g.GetComponentsInChildren<Graphic>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren2[i]);
		}
		AudioSource[] componentsInChildren3 = g.GetComponentsInChildren<AudioSource>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren3[i]);
		}
	}

	private void RemovePegFromSimulation(Peg.PegType pegtype, Peg peg)
	{
		if (peg is IDummyPeg key && _allPegs.ContainsKey(key))
		{
			_allPegs[key].SetPegStatus(status: false);
		}
	}

	public void DestroyPegInSimulation(Peg peg)
	{
		if (!(peg is IDummyPeg dummyPeg))
		{
			return;
		}
		if (dummyPeg is IDummiableLayerChanger key && _layerChangers.ContainsKey(key))
		{
			_layerChangers.Remove(key);
		}
		IDummiableLayerChanger componentInChildren = peg.GetComponentInChildren<IDummiableLayerChanger>(includeInactive: true);
		if (componentInChildren != null && _layerChangers.ContainsKey(componentInChildren))
		{
			_layerChangers.Remove(componentInChildren);
		}
		if (dummyPeg is IDummiableMovingPeg key2 && _movingPegs.ContainsKey(key2))
		{
			_movingPegs.Remove(key2);
		}
		IDummiableMovingPeg componentInChildren2 = peg.GetComponentInChildren<IDummiableMovingPeg>(includeInactive: true);
		if (componentInChildren2 != null && _movingPegs.ContainsKey(componentInChildren2))
		{
			_movingPegs.Remove(componentInChildren2);
		}
		if (dummyPeg is IDummiableMovingPegEndOfTurn key3 && _movingPegsEndOfTurn.ContainsKey(key3))
		{
			_movingPegsEndOfTurn.Remove(key3);
		}
		IDummiableMovingPegEndOfTurn componentInChildren3 = peg.GetComponentInChildren<IDummiableMovingPegEndOfTurn>(includeInactive: true);
		if (componentInChildren3 != null && _movingPegsEndOfTurn.ContainsKey(componentInChildren3))
		{
			_movingPegsEndOfTurn.Remove(componentInChildren3);
		}
		if (dummyPeg is IDummiablePegTypeChanger key4 && _pegTypeChangers.ContainsKey(key4))
		{
			_pegTypeChangers.Remove(key4);
		}
		IDummiablePegTypeChanger componentInChildren4 = peg.GetComponentInChildren<IDummiablePegTypeChanger>(includeInactive: true);
		if (componentInChildren4 != null && _pegTypeChangers.ContainsKey(componentInChildren4))
		{
			_pegTypeChangers.Remove(componentInChildren4);
		}
		if (peg.pegType == Peg.PegType.DESTROYED)
		{
			Peg[] componentsInChildren = peg.GetComponentsInChildren<Peg>(includeInactive: true);
			foreach (Peg peg2 in componentsInChildren)
			{
				if (!(peg2.transform == peg.transform))
				{
					DestroyPegInSimulation(peg2);
				}
			}
		}
		if (_allPegs.ContainsKey(dummyPeg))
		{
			if (((Component)_allPegs[dummyPeg]).gameObject != null)
			{
				UnityEngine.Object.Destroy(((Component)_allPegs[dummyPeg]).gameObject);
			}
			_allPegs.Remove(dummyPeg);
		}
	}

	public void CopyPegToSimulation(GameObject newpeg, IDummyPeg parent)
	{
		if (!_predictionScene.IsValid())
		{
			return;
		}
		IDummyPeg component = newpeg.GetComponent<IDummyPeg>();
		if (component == null || _allPegs.ContainsKey(component))
		{
			return;
		}
		Transform transform = UnityEngine.Object.Instantiate(newpeg.transform, newpeg.transform.position, newpeg.transform.rotation);
		DeactivateRenderers(transform.gameObject);
		IDummyPeg component2 = transform.GetComponent<IDummyPeg>();
		component2.SetDummyStatus(status: true);
		IDummiableLayerChanger component3 = transform.GetComponent<IDummiableLayerChanger>();
		if (component3 != null)
		{
			component3.SetDummyStatus(status: true);
			IDummiableLayerChanger component4 = newpeg.GetComponent<IDummiableLayerChanger>();
			_layerChangers.TryAdd(component4, component3);
		}
		IDummiableMovingPegEndOfTurn component5 = transform.GetComponent<IDummiableMovingPegEndOfTurn>();
		if (component5 != null)
		{
			component5.SetDummyStatus(status: true);
			_movingPegsEndOfTurn.TryAdd(newpeg.GetComponent<IDummiableMovingPegEndOfTurn>(), component5);
		}
		SceneManager.MoveGameObjectToScene(transform.gameObject, _predictionScene);
		if (!transform.gameObject.activeInHierarchy)
		{
			transform.gameObject.SetActive(value: true);
		}
		if (parent != null && _allPegs.TryGetValue(parent, out var value))
		{
			Component component6 = value as Component;
			if (component6 != null)
			{
				transform.transform.SetParent(component6.transform);
				transform.transform.localPosition = newpeg.transform.localPosition;
			}
		}
		_allPegs.Add(component, component2);
	}

	private void ApplyParentTransformationsRecursively(Transform copy, Transform original)
	{
		if (!(original.parent == null))
		{
			copy.localScale = new Vector3(original.parent.localScale.x * copy.localScale.x, original.parent.localScale.y * copy.localScale.y);
			ApplyParentTransformationsRecursively(copy, original.parent);
		}
	}

	public void CreateFakePegboard()
	{
		if (_fakePegboard != null || !_predictionScene.IsValid())
		{
			return;
		}
		if (_pegBoard == null)
		{
			if (SceneManager.GetActiveScene().name != "MainMenu")
			{
				UnityEngine.Debug.LogWarning("AimSim Could not be created, pegboard is null!");
			}
		}
		else
		{
			_fakePegboard = UnityEngine.Object.Instantiate(_pegBoard.gameObject, _pegBoard.transform.position, _pegBoard.transform.rotation);
			DeactivateRenderers(_fakePegboard);
			SceneManager.MoveGameObjectToScene(_fakePegboard, _predictionScene);
		}
	}

	public void DestroyFakePegboard(GameObject updatedPegboard)
	{
		if (_fakePegboard == null || !_predictionScene.IsValid())
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in _fakePegboard.transform)
		{
			if (!item.gameObject.GetComponent<TilemapRenderer>())
			{
				list.Add(item.gameObject);
			}
		}
		foreach (GameObject item2 in list)
		{
			UnityEngine.Object.Destroy(item2);
		}
		_pegBoard = updatedPegboard;
		_fakePegboard = null;
		CreateFakePegboard();
	}

	public void AddObjectToSimulationScene(GameObject obj)
	{
		if (!_predictionScene.IsValid())
		{
			return;
		}
		if (obj == null)
		{
			UnityEngine.Debug.LogError("Tried to add a null object to simulation");
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(obj, obj.transform.position, obj.transform.rotation);
		foreach (Transform item in gameObject.transform)
		{
			Renderer component = item.GetComponent<Renderer>();
			if ((bool)component)
			{
				component.enabled = false;
			}
		}
		IUpdateableBySimulation[] componentsInChildren = gameObject.GetComponentsInChildren<IUpdateableBySimulation>(includeInactive: true);
		foreach (IUpdateableBySimulation updateableBySimulation in componentsInChildren)
		{
			updateableBySimulation.SetDummy(value: true);
			_dynamicallyAddedUpdateableSceneObjs.Add(updateableBySimulation);
		}
		_addedObjects.TryAdd(obj, gameObject);
		SceneManager.MoveGameObjectToScene(gameObject, _predictionScene);
	}

	public void DestroyObjectInSimulation(GameObject parent)
	{
		if (!_addedObjects.Remove(parent, out var value))
		{
			return;
		}
		IUpdateableBySimulation[] componentsInChildren = value.GetComponentsInChildren<IUpdateableBySimulation>(includeInactive: true);
		foreach (IUpdateableBySimulation item in componentsInChildren)
		{
			if (_dynamicallyAddedUpdateableSceneObjs.Contains(item))
			{
				_dynamicallyAddedUpdateableSceneObjs.Remove(item);
			}
		}
		UnityEngine.Object.Destroy(value);
	}

	public void KillAllPegs()
	{
		foreach (GameObject dummyPeg in _dummyPegs)
		{
			UnityEngine.Object.Destroy(dummyPeg);
		}
		_dummyPegs.Clear();
		_movingPegs.Clear();
		_layerChangers.Clear();
		_movingPegsEndOfTurn.Clear();
	}

	public StaticPredictionLineData AddStaticPredictionLine(GameObject orb, Vector3 firingPosition, Vector3 force, bool multiballOnFiredBecomeSmaller)
	{
		LineRenderer component = UnityEngine.Object.Instantiate(StaticLineRendererPrefab).GetComponent<LineRenderer>();
		_staticScrollingMaterials.Add(component.material);
		StaticPredictionLineData staticPredictionLineData = new StaticPredictionLineData(component, orb, firingPosition, force, multiballOnFiredBecomeSmaller);
		_staticPredictionLinesData.Add(staticPredictionLineData);
		return staticPredictionLineData;
	}

	public void InitializePachinko(GameObject subject, Vector3 position, bool isOrbUninitialized, bool smallerOrbForPrediction)
	{
		UnityEngine.Object.Destroy(_dummy);
		foreach (StaticPredictionLineData staticPredictionLinesDatum in _staticPredictionLinesData)
		{
			UnityEngine.Object.Destroy(staticPredictionLinesDatum.lineRenderer.gameObject);
		}
		_staticPredictionLinesData.Clear();
		_staticScrollingMaterials.Clear();
		_dummy = UnityEngine.Object.Instantiate(subject);
		SceneManager.MoveGameObjectToScene(_dummy, _predictionScene);
		PachinkoBall component = _dummy.GetComponent<PachinkoBall>();
		component.SetRelicManager(_relicManager);
		if (isOrbUninitialized)
		{
			component.InitPredictionAffectingRelicsThatAddComponents();
		}
		currentPachinkoInfo.SetPachinkoInfo(component, smallerOrbForPrediction);
		currentPachinkoInfo.pachinko.IsDummy = true;
		currentPachinkoInfo.pachinko.InitializeMembers();
		currentPachinkoInfo.pachinko.MaxBounceCount = _bounceCount;
		PachinkoBall pachinko = currentPachinkoInfo.pachinko;
		pachinko.OnPachinkoBallDummyBounce = (PachinkoBall.PachinkoBallDummyBounce)Delegate.Combine(pachinko.OnPachinkoBallDummyBounce, new PachinkoBall.PachinkoBallDummyBounce(CreateBounceIndicator));
		PachinkoBall pachinko2 = currentPachinkoInfo.pachinko;
		pachinko2.OnPachinkoBallDummyDestroyed = (PachinkoBall.PachinkoBallDummyDestroyed)Delegate.Combine(pachinko2.OnPachinkoBallDummyDestroyed, new PachinkoBall.PachinkoBallDummyDestroyed(CreateDestroyedIndicator));
		currentPachinkoInfo.pachinko.GetComponentInChildren<SpriteRenderer>().enabled = false;
		currentPachinkoInfo.rigidbody.simulated = true;
		currentPachinkoInfo.rigidbody.gravityScale = currentPachinkoInfo.pachinko.GravityScale;
		if (_relicManager.RelicEffectActive(RelicEffect.AIM_LIMITER_MULTIBALL) && !_navigationOrb)
		{
			_chainsMultiballPredictionLineDataId = AddStaticPredictionLine(_dummy, position, Vector3.zero, _relicManager.RelicEffectActive(RelicEffect.MATRYOSHKA)).guid;
			_chainsMultiballPredictionLineDataId2 = AddStaticPredictionLine(_dummy, position, Vector3.zero, _relicManager.RelicEffectActive(RelicEffect.MATRYOSHKA)).guid;
		}
	}

	public void Predict(GameObject subject, Vector3 currentPosition, Vector3 force)
	{
		if (!_currentPhysicsScene.IsValid() || !_predictionPhysicsScene.IsValid())
		{
			return;
		}
		foreach (GameObject bounceIndicator in _bounceIndicatorList)
		{
			bounceIndicator.SetActive(value: false);
		}
		foreach (GameObject destroyedIndicator in _destroyedIndicatorList)
		{
			destroyedIndicator.SetActive(value: false);
		}
		Vector3 localScale = _dummy.gameObject.transform.localScale;
		Vector3 newMultiballScale = PachinkoBall.GetNewMultiballScale(_dummy.gameObject.transform.localScale, 0.7071f);
		newMultiballScale = PachinkoBall.GetNewMultiballScale(newMultiballScale, 0.7071f);
		for (int i = -1; i < _staticPredictionLinesData.Count; i++)
		{
			LineRenderer lineRenderer = _lineRenderer;
			_dummy.transform.localScale = localScale;
			Vector3 vector = currentPosition;
			Vector3 vector2 = force;
			bool flag = currentPachinkoInfo.smallerOrbForPrediction;
			if (i != -1)
			{
				lineRenderer = _staticPredictionLinesData[i].lineRenderer;
				if (_maxIterations <= 2)
				{
					lineRenderer.material = SolidStaticLineMaterial;
				}
				vector = _staticPredictionLinesData[i].startingPosition;
				if ((_relicManager.RelicEffectActive(RelicEffect.AIM_LIMITER_MULTIBALL) && _chainsMultiballPredictionLineDataId == _staticPredictionLinesData[i].guid) || _chainsMultiballPredictionLineDataId2 == _staticPredictionLinesData[i].guid)
				{
					Vector3 vector3 = Vector2.Perpendicular(force).normalized;
					if (_chainsMultiballPredictionLineDataId == _staticPredictionLinesData[i].guid)
					{
						vector2 = Vector3.Slerp(force.normalized, vector3, 0.18f).normalized * force.magnitude;
					}
					else if (_chainsMultiballPredictionLineDataId2 == _staticPredictionLinesData[i].guid)
					{
						vector2 = Vector3.Slerp(force.normalized, -vector3, 0.18f).normalized * force.magnitude;
					}
				}
				else
				{
					vector2 = _staticPredictionLinesData[i].firingForce;
				}
				flag = _staticPredictionLinesData[i].multiballOnFiredBecomeSmaller;
			}
			currentPachinkoInfo.rigidbody.position = vector;
			currentPachinkoInfo.pachinko.fixedShotTime = 0f;
			currentPachinkoInfo.rigidbody.AddForce(vector2);
			currentPachinkoInfo.pachinko.ResetBounces();
			lineRenderer.positionCount = _maxIterations;
			if (_dummy.TryGetComponent<GhostOrbPachinko>(out var component))
			{
				component.ForceUpdateGhostEffect(vector2);
			}
			if (flag)
			{
				_dummy.transform.localScale = newMultiballScale;
			}
			int num = 0;
			foreach (IDummiablePegTypeChanger key in _pegTypeChangers.Keys)
			{
				_pegTypeChangers[key].SetPegType(key.GetPegType());
			}
			_stopwatch.Start();
			for (int j = 0; j < _maxIterations; j++)
			{
				if (currentPachinkoInfo.pachinko.HasDummyCollided && !_shortSolidLineMode)
				{
					break;
				}
				_stopwatch.Restart();
				_predictionPhysicsScene.Simulate(Time.fixedDeltaTime);
				currentPachinkoInfo.pachinko.fixedShotTime += Time.fixedDeltaTime;
				_stopwatch.Restart();
				IUpdateableBySimulation[] updateableBySimulation = currentPachinkoInfo.updateableBySimulation;
				for (int k = 0; k < updateableBySimulation.Length; k++)
				{
					updateableBySimulation[k].DoUpdate(_predictionPhysicsScene);
				}
				foreach (IUpdateableBySimulation updateableSceneObj in _updateableSceneObjs)
				{
					updateableSceneObj.DoUpdate(_predictionPhysicsScene);
				}
				foreach (IUpdateableBySimulation dynamicallyAddedUpdateableSceneObj in _dynamicallyAddedUpdateableSceneObjs)
				{
					dynamicallyAddedUpdateableSceneObj.DoUpdate(_predictionPhysicsScene);
				}
				UpdatePredictionPegs();
				_stopwatch.Restart();
				if (!currentPachinkoInfo.pachinko.HasDummyCollided || _shortSolidLineMode)
				{
					lineRenderer.SetPosition(j, new Vector3(_dummy.transform.position.x, _dummy.transform.position.y, -1f));
				}
				num = j;
			}
			if (_shortSolidLineMode && currentPachinkoInfo.pachinko.HasDummyCollided)
			{
				lineRenderer.SetPosition(num, new Vector3(_bounceIndicatorList[0].transform.position.x, _bounceIndicatorList[0].transform.position.y, -1f));
			}
			else if (currentPachinkoInfo.pachinko.HasDummyCollided && num > 0)
			{
				for (int l = num; l < _maxIterations; l++)
				{
					Vector3 position = lineRenderer.GetPosition(l - 1);
					lineRenderer.SetPosition(l, position);
				}
			}
			else if (currentPachinkoInfo.pachinko.HasDummyCollided && num == 0)
			{
				for (int m = num; m < _maxIterations; m++)
				{
					lineRenderer.SetPosition(m, vector);
				}
			}
			_stopwatch.Restart();
			if (!(_dummy != null))
			{
				continue;
			}
			currentPachinkoInfo.rigidbody.velocity = Vector2.zero;
			currentPachinkoInfo.rigidbody.angularVelocity = 0f;
			currentPachinkoInfo.rigidbody.isKinematic = true;
			currentPachinkoInfo.rigidbody.MovePosition(vector);
			_predictionPhysicsScene.Simulate(Time.fixedDeltaTime);
			currentPachinkoInfo.rigidbody.isKinematic = false;
			currentPachinkoInfo.pachinko.HasDummyCollided = false;
			currentPachinkoInfo.rigidbody.simulated = true;
			currentPachinkoInfo.pachinko.MaxBounceCount = _bounceCount;
			currentPachinkoInfo.pachinko.IsDummy = true;
			if (currentPachinkoInfo.pierce != null)
			{
				PierceBehavior[] pierce = currentPachinkoInfo.pierce;
				for (int k = 0; k < pierce.Length; k++)
				{
					pierce[k].RestartPierceForSimulation();
				}
			}
			currentPachinkoInfo.ResetPhysicsAttributes();
			_stopwatch.Reset();
		}
	}

	public void StopPredictionDebug()
	{
		StopAllCoroutines();
	}

	private void UpdatePredictionPegs()
	{
		foreach (IDummiableMovingPeg key in _movingPegs.Keys)
		{
			_movingPegs[key].SetPosition(key.GetPosition());
		}
		foreach (IDummiableLayerChanger key2 in _layerChangers.Keys)
		{
			_layerChangers[key2].SetLayer(key2.GetLayer());
		}
	}

	public void SetLineRendererStatus(bool status)
	{
		_lineRenderer.enabled = status;
		foreach (StaticPredictionLineData staticPredictionLinesDatum in _staticPredictionLinesData)
		{
			staticPredictionLinesDatum.lineRenderer.enabled = status;
		}
		if (status)
		{
			return;
		}
		foreach (GameObject bounceIndicator in _bounceIndicatorList)
		{
			bounceIndicator.SetActive(value: false);
		}
		foreach (GameObject destroyedIndicator in _destroyedIndicatorList)
		{
			destroyedIndicator.SetActive(value: false);
		}
	}

	public void PlayerFired()
	{
		SetLineRendererStatus(status: false);
		foreach (IUpdateableBySimulation updateableSceneObj in _updateableSceneObjs)
		{
			updateableSceneObj.Disable();
		}
	}

	private void CreateBounceIndicator(Vector3 collisionPoint, Vector3 collisionOrbPosition)
	{
		GameObject pooledObject = _bounceIndicatorObjectPool.GetPooledObject();
		if (!(pooledObject == null))
		{
			float num = pooledObject.transform.localScale.x + _additionalBounceOffset;
			pooledObject.transform.position = collisionPoint + (collisionOrbPosition - collisionPoint) * num;
			pooledObject.SetActive(value: true);
			if (!_bounceIndicatorList.Contains(pooledObject))
			{
				_bounceIndicatorList.Add(pooledObject);
			}
		}
	}

	private void CreateDestroyedIndicator(Vector3 destroyedPos)
	{
		GameObject pooledObject = _destroyedIndicatorObjectPool.GetPooledObject();
		if (!(pooledObject == null))
		{
			pooledObject.transform.position = destroyedPos;
			pooledObject.SetActive(value: true);
			if (!_destroyedIndicatorList.Contains(pooledObject))
			{
				_destroyedIndicatorList.Add(pooledObject);
			}
		}
	}

	private void OnDestroy()
	{
		KillAllPegs();
	}
}
