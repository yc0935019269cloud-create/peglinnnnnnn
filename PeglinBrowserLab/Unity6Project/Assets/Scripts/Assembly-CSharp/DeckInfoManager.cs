using System;
using System.Collections.Generic;
using Battle;
using Battle.Attacks;
using Battle.Enemies;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using PeglinUI.OrbDisplay;
using UnityEngine;
using UnityEngine.UI;

public class DeckInfoManager : MonoBehaviour
{
	public delegate void OrbInfoEvent(GameObject ball);

	public delegate void DeckInfoAnim();

	public delegate void ShuffleAnim();

	private const int MAX_UPCOMING_ORBS_ABLE_TO_BE_INSPECTED = 10;

	private const int MAX_UPCOMING_ORBS_INSPECTED_SPIRIT_OF_RAIDA_PHASE_2 = 7;

	public static bool available = false;

	public static bool animating = false;

	public static bool populatingDisplayOrb = false;

	private static float BALL_DRAW_ANIM_SHORTCUT = 0.1f;

	private static float ACTIVE_ORB_DISPLAY_HEIGHT = 0.85f;

	private static float ORB_SPRITE_OFFSET = 0.875f;

	public static OrbInfoEvent onActiveOrbScaleStarted = delegate
	{
	};

	public static OrbInfoEvent onActivePersistOrbDrawFinished = delegate
	{
	};

	public static DeckInfoAnim onActiveOrbScaleCompleted = () =>
	{
	};

	public static ShuffleAnim onShuffleAnimComplete = () =>
	{
	};

	[Header("Variables")]
	public float plungeTargetY = -16f;

	public float plungeDropTime = 1f;

	public float plungePullTime = 0.8f;

	public float moveAndScaleActiveOrbTime = 0.6f;

	public float firedOrbScaleOutTime = 0.4f;

	public float upcomingDisplayOrbFudgeFactor = 0.005f;

	[SerializeField]
	private Sprite[] _orbLevelDisplaySprites;

	[Header("References")]
	[SerializeField]
	private BattleController _battleController;

	[SerializeField]
	private GameObject _displayObjectPrefab;

	[SerializeField]
	private Transform _topTransform;

	[SerializeField]
	private Transform _plungerTransform;

	[SerializeField]
	private Transform _currentOrbDisplayPos;

	[SerializeField]
	private DeckManager _deckManager;

	[SerializeField]
	private Canvas _ballInfoCanvas;

	[SerializeField]
	private Transform _plungerGraphic;

	[SerializeField]
	private SpriteRenderer _currentOrbLevelRingRenderer;

	public GameObject orbInspectionButtonPrompt;

	private ObjectPoolerDict _ballWidgetPool;

	private Transform _plungerParent;

	private Stack<GameObject> _displayOrbs;

	private GameObject _currentOrb;

	private GameObject _nextOrb;

	private Vector3 _startingPlungerGraphicPosition;

	public Stack<GameObject> displayOrbs => _displayOrbs;

	public GameObject currentOrb => _currentOrb;

	private void Awake()
	{
		_plungerParent = _plungerTransform.parent;
		_startingPlungerGraphicPosition = _plungerGraphic.localPosition;
		available = true;
	}

	private void Start()
	{
		_displayOrbs = new Stack<GameObject>();
	}

	private void OnEnable()
	{
		if (_ballWidgetPool == null)
		{
			GameObject gameObject = new GameObject();
			_ballWidgetPool = gameObject.AddComponent<ObjectPoolerDict>();
		}
		DeckManager.onBallUsed = (DeckManager.BallDrawn)Delegate.Combine(DeckManager.onBallUsed, new DeckManager.BallDrawn(DrawNextOrb));
		DeckManager.onPersistBallUsed = (DeckManager.PersistBallUsed)Delegate.Combine(DeckManager.onPersistBallUsed, new DeckManager.PersistBallUsed(PersistBallDrawFinished));
		DeckManager.onDeckShuffled = (DeckManager.Shuffled)Delegate.Combine(DeckManager.onDeckShuffled, new DeckManager.Shuffled(StartShuffleAnimation));
		BattleController.OnAttackStarted = (Action)Delegate.Combine(BattleController.OnAttackStarted, new Action(HandleAttackStarted));
		BattleController.onOrbAttackEnded = (BattleController.PersistenceUpdate)Delegate.Combine(BattleController.onOrbAttackEnded, new BattleController.PersistenceUpdate(UpdatePersistentInfo));
		BattleController.OnOrbDiscarded = (Action)Delegate.Combine(BattleController.OnOrbDiscarded, new Action(HandleOrbDiscarded));
	}

	private void OnDisable()
	{
		if (_ballWidgetPool != null)
		{
			UnityEngine.Object.Destroy(_ballWidgetPool.gameObject);
		}
		DeckManager.onBallUsed = (DeckManager.BallDrawn)Delegate.Remove(DeckManager.onBallUsed, new DeckManager.BallDrawn(DrawNextOrb));
		DeckManager.onPersistBallUsed = (DeckManager.PersistBallUsed)Delegate.Remove(DeckManager.onPersistBallUsed, new DeckManager.PersistBallUsed(PersistBallDrawFinished));
		DeckManager.onDeckShuffled = (DeckManager.Shuffled)Delegate.Remove(DeckManager.onDeckShuffled, new DeckManager.Shuffled(StartShuffleAnimation));
		BattleController.OnAttackStarted = (Action)Delegate.Remove(BattleController.OnAttackStarted, new Action(HandleAttackStarted));
		BattleController.onOrbAttackEnded = (BattleController.PersistenceUpdate)Delegate.Remove(BattleController.onOrbAttackEnded, new BattleController.PersistenceUpdate(UpdatePersistentInfo));
		BattleController.OnOrbDiscarded = (Action)Delegate.Remove(BattleController.OnOrbDiscarded, new Action(HandleOrbDiscarded));
	}

	private void StartShuffleAnimation(int deckSize)
	{
		animating = true;
		TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = _plungerParent.DOMoveY(plungeTargetY, plungeDropTime).SetEase(Ease.InOutSine);
		tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(PlungerPlungeComplete));
	}

	private void HandleAttackStarted()
	{
		if (_currentOrb != null && BattleController.currentBallPersistLevel < 0)
		{
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = _currentOrb.transform.DOScale(0f, firedOrbScaleOutTime);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(DisableCurrentOrb));
		}
	}

	private void HandleOrbDiscarded()
	{
		if (_currentOrb != null)
		{
			_currentOrb.transform.DOKill();
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = _currentOrb.transform.DOScale(0f, firedOrbScaleOutTime / 2f);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(DisableCurrentOrb));
		}
	}

	private void PlungerPlungeComplete()
	{
		foreach (GameObject displayOrb in _displayOrbs)
		{
			UnityEngine.Object.Destroy(displayOrb);
		}
		_displayOrbs.Clear();
		GameObject[] array = _deckManager.shuffledDeck.ToArray();
		float num = (float)array.Length * (0f - ORB_SPRITE_OFFSET);
		_plungerGraphic.localPosition = new Vector3(_startingPlungerGraphicPosition.x, num + _startingPlungerGraphicPosition.y);
		for (int num2 = array.Length - 1; num2 >= 0; num2--)
		{
			GameObject gameObject = CreatePreviewSprite(array[num2], (float)num2 * 0.01f);
			gameObject.transform.parent = _plungerParent;
			float y = gameObject.GetComponent<SpriteRenderer>().bounds.size.y;
			gameObject.transform.localPosition = Vector3.up * (num + upcomingDisplayOrbFudgeFactor + y * 0.5f);
			num += y;
			_displayOrbs.Push(gameObject);
		}
		TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = _plungerParent.DOMoveY(_topTransform.position.y - num, plungePullTime).SetEase(Ease.InOutSine);
		tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(ShuffleAnimComplete));
	}

	private void ShuffleAnimComplete()
	{
		animating = false;
		onShuffleAnimComplete();
	}

	private GameObject CreatePreviewSprite(GameObject ballPrefab, float zOffset = 0f)
	{
		GameObject obj = UnityEngine.Object.Instantiate(_displayObjectPrefab);
		obj.transform.position -= new Vector3(0f, 0f, zOffset);
		obj.GetComponentInChildren<UpcomingOrbDisplay>().Initialize(ballPrefab, _ballWidgetPool, _ballInfoCanvas, _battleController);
		return obj;
	}

	private void UpdatePersistentInfo(int persistLevel)
	{
		if (_currentOrb != null)
		{
			_currentOrb.GetComponentInChildren<UpcomingOrbDisplay>().UpdatePersistentOrb(persistLevel);
		}
	}

	private void DrawNextOrb(GameObject ball)
	{
		populatingDisplayOrb = true;
		_nextOrb = _displayOrbs.Pop();
		UpcomingOrbDisplay componentInChildren = _nextOrb.GetComponentInChildren<UpcomingOrbDisplay>();
		int num = 1;
		if (componentInChildren.attack != null)
		{
			num = Mathf.Clamp(componentInChildren.attack.Level - 1, 0, 2);
			AssemballAttack assemballAttack = componentInChildren.attack as AssemballAttack;
			if (assemballAttack != null)
			{
				num = Mathf.Clamp(AssemballAttack.GetAssemballDisplayLevel(assemballAttack.GetNumberOfComponents()) - 1, 0, 2);
			}
		}
		_currentOrbLevelRingRenderer.sprite = _orbLevelDisplaySprites[num];
		float y = _nextOrb.GetComponent<SpriteRenderer>().bounds.size.y;
		TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = _plungerParent.DOMoveY(_plungerParent.position.y + y, BattleController.BALL_DRAW_ANIM_TIME - BALL_DRAW_ANIM_SHORTCUT).SetEase(Ease.InOutSine);
		tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(BallDrawFinished));
		componentInChildren.mainOrbLevelFrameMask.gameObject.SetActive(value: true);
		Transform transform = componentInChildren.mainOrbLevelFrameMask.transform;
		transform.transform.DOScale(transform.localScale, 0.35f).From(Vector3.zero);
	}

	private void BallDrawFinished()
	{
		if (_nextOrb != null)
		{
			onActiveOrbScaleStarted(_nextOrb);
			UpcomingOrbDisplay componentInChildren = _nextOrb.GetComponentInChildren<UpcomingOrbDisplay>();
			componentInChildren.StartTextCanvasFade(moveAndScaleActiveOrbTime * 0.5f);
			componentInChildren.PreSetActive();
			_nextOrb.transform.DOMove(_currentOrbDisplayPos.transform.position, moveAndScaleActiveOrbTime);
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = _nextOrb.transform.DOScale(Vector3.one * ACTIVE_ORB_DISPLAY_HEIGHT, moveAndScaleActiveOrbTime);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(EndCurrentOrbScale));
		}
	}

	private void PersistBallDrawFinished()
	{
		if (_nextOrb != null)
		{
			onActivePersistOrbDrawFinished(_nextOrb);
		}
	}

	private void EndCurrentOrbScale()
	{
		if (_currentOrb != null)
		{
			_currentOrb.transform.DOKill();
			UnityEngine.Object.Destroy(_currentOrb);
			_currentOrb = null;
		}
		_currentOrb = _nextOrb;
		_currentOrb.GetComponentInChildren<UpcomingOrbDisplay>().SetActive();
		_currentOrb.GetComponentInChildren<Animator>().speed = 1f;
		onActiveOrbScaleCompleted();
		populatingDisplayOrb = false;
	}

	private void DisableCurrentOrb()
	{
		_currentOrb.GetComponentInChildren<Animator>().speed = 0f;
		_currentOrb.transform.DOKill();
		UnityEngine.Object.Destroy(_currentOrb);
		_currentOrb = null;
	}

	public void UpdateUINavigation()
	{
		List<GameObject> list = new List<GameObject>(_displayOrbs);
		List<Selectable> list2 = new List<Selectable>();
		list2.Add(_currentOrb.GetComponentInChildren<Selectable>());
		int num = ((SpiritOfRadiaBoss.CurrentPhase == 2) ? 7 : 10);
		for (int i = 0; i < Mathf.Min(list.Count, num - 1); i++)
		{
			list2.Add(list[i].GetComponentInChildren<Selectable>());
		}
		if (list2.Count > 1)
		{
			Navigation navigation = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				selectOnDown = list2[1],
				selectOnRight = list2[1]
			};
			list2[0].navigation = navigation;
			for (int j = 1; j < list2.Count - 1; j++)
			{
				Navigation navigation2 = new Navigation
				{
					mode = Navigation.Mode.Explicit,
					selectOnUp = list2[j - 1],
					selectOnLeft = list2[j - 1],
					selectOnDown = list2[j + 1],
					selectOnRight = list2[j + 1]
				};
				list2[j].navigation = navigation2;
			}
			Navigation navigation3 = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				selectOnUp = list2[list2.Count - 2],
				selectOnLeft = list2[list2.Count - 2]
			};
			list2[list2.Count - 1].navigation = navigation3;
		}
	}
}
