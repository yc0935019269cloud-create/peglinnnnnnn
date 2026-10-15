using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Battle;
using Battle.Attacks;
using Battle.Enemies;
using Battle.Pachinko;
using Battle.Pachinko.BallBehaviours;
using Battle.StatusEffects;
using MoreMountains.Tools;
using Peglin.Achievements;
using PeglinUI;
using PeglinUI.ControllerSupport;
using PeglinUtils;
using Relics;
using Rewired;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PachinkoBall : MonoBehaviour
{
	public delegate void PachinkoBallGameObjectEvent(GameObject obj);

	public delegate void PachinkoBallFired(Vector2 aimVector);

	public delegate void PachinkoBallWallBounce(Vector3 pos);

	public delegate void PachinkoBallBouncerBounce(Vector3 pos);

	public delegate void PachinkoWallBounceAudio(float volume);

	public delegate void PachinkoBallDummyBounce(Vector3 collisionPoint, Vector3 collisionOrbPosition);

	public delegate void PachinkoBallDummyDestroyed(Vector3 destroyedPos);

	public delegate void PachinkoBallPegHit(Peg peg, PachinkoBall pBall);

	public delegate void PachinkoBallArmed(PachinkoBall pBall);

	public delegate void PachinkoBallCreated();

	public delegate void PachinkoBallDestroyed(PachinkoBall pBall);

	public enum FireballState
	{
		WAITING = 0,
		AIMING = 1,
		FIRING = 2,
		AWAITING_RESULTS = 3
	}

	public enum OrbRarity
	{
		NOT_PRESENT = 0,
		COMMON = 1,
		UNCOMMON = 2,
		RARE = 3,
		SPECIAL = 4
	}

	private const float STUCK_BALL_SPEED_THRESHOLD = 0.25f;

	private const float LOST_BALL_Y_THRESHOLD = -15f;

	private const float WALL_BOUNCE_AUDIO_MAX_VELO = 20f;

	private const float DEFAULT_ORB_COLLIDER_RADIUS = 0.58f;

	private const float DEFAULT_SMALL_ORB_COLLIDER_RADIUS = 0.29f;

	private const float DEFAULT_SMALL_ORB_SCALE = 0.4f;

	private const float DEFAULT_LARGE_ORB_SCALE = 0.8f;

	private const float DEFAULT_REGULAR_ORB_SCALE = 0.4f;

	public static PachinkoBallGameObjectEvent OnPachinkoBallPermanentDestruction = (GameObject obj) =>
	{
	};

	public static PachinkoBallFired OnShotFired = delegate
	{
	};

	public static PachinkoBallWallBounce OnPachinkoBallWallBounce = delegate
	{
	};

	public static PachinkoBallBouncerBounce OnPachinkoBallBouncerBounce = delegate
	{
	};

	public static PachinkoWallBounceAudio OnPachinkoWallBounceAudio = delegate
	{
	};

	public PachinkoBallDummyBounce OnPachinkoBallDummyBounce = delegate
	{
	};

	public PachinkoBallDummyDestroyed OnPachinkoBallDummyDestroyed = delegate
	{
	};

	public PachinkoBallPegHit OnPachinkoBallPegHit = delegate
	{
	};

	public PachinkoBallPegHit OnPachinkoBallSlimeOnlyPegHit = delegate
	{
	};

	public static PachinkoBallArmed OnPachinkoBallArmed = (PachinkoBall pBall) =>
	{
	};

	public static PachinkoBallCreated OnAdditionalPachinkoBallCreated = () =>
	{
	};

	public static PachinkoBallDestroyed OnPachinkoBallDestroyed = delegate
	{
	};

	public bool IsDummy;

	public bool HasDummyCollided;

	public AudioClip _launchSound;

	protected TrajectorySimulation _trajectorySimulation;

	public float confusionRotateSpeed = 375f;

	public float controllerRotateSpeedBase = 160f;

	public float controllerRotateSpeedMax = 225f;

	public float controllerRotateRampSpeed = 30f;

	protected float _currentControllerRotateSpeed = 160f;

	public float smallMovementForce = 4f;

	public bool faceVelocity = true;

	[Header("Physics")]
	public float FireForce = 400f;

	public float GravityScale = 1.2f;

	private const float MaxSpeed = 50f;

	public float MultiballForceMod = 0.65f;

	private float _radius;

	private Stopwatch _stopwatch = new Stopwatch();

	[HideInInspector]
	public int MaxBounceCount = 1;

	private Rigidbody2D _rigid;

	[SerializeField]
	protected SpriteRenderer _renderer;

	protected AudioSource _wallbounceAudioSource;

	protected Vector2 _previousAimVector = Vector2.zero;

	protected Vector2 _aimVector;

	private const float STUCK_KILL_TIME = 2.5f;

	private const float UNSTUCK_MIN_TIME = 1f;

	private float _stuckTimer;

	private float _unstuckTimer;

	private Camera _mainCamera;

	private Player _player;

	protected RelicManager _relicManager;

	protected DeckManager _deckManager;

	protected PlayerStatusEffectController _playerStatusEffectController;

	private TrailRenderer _trailRenderer;

	protected bool _mouseDownOnPlayfield;

	protected bool _fireButtonPressed;

	public int multiballLevel;

	protected List<PachinkoBall> _childMultiballs;

	protected float _fixedShotTime;

	protected PredictionManager _predictionManager;

	private int _bounceCount;

	private bool _isColliding;

	private static readonly int Crack = Animator.StringToHash("Crack");

	private bool _shouldInitPrediction;

	[SerializeField]
	protected string[] _layerMasksToClearWhenPoppingPegs = new string[2] { "PegCollider", "Bombs" };

	private LayerMask _circleCastLayerMask;

	private bool _refreshPegHitThisTurn;

	public bool isBattleOrb = true;

	private bool _hasBuffedFirstPegHit;

	private Vector2 _preRotatedAimVector;

	private float _aimLimiterOffsetAngle;

	private List<float> _normalizedOrbSizesList = new List<float>();

	[HideInInspector]
	public bool isOrigionalSize = true;

	[HideInInspector]
	public GameObject lastHitPeg;

	[SerializeField]
	private bool _isMirrorb;

	public OrbRarity orbRarity = OrbRarity.COMMON;

	private float _predictionTimer;

	private const float PREDICTION_FRAMERATE_BASE = 0.016f;

	private const float PREDICTION_FRAMERATE_LOW = 0.032f;

	private bool _currentlyDestroyingMultiballs;

	public FireballState CurrentState { get; protected set; }

	public virtual Sprite sprite => _renderer.sprite;

	public Color color => _renderer.color;

	public Vector2 aimVector => _aimVector;

	public float shotTime { get; set; }

	public float fixedShotTime
	{
		get
		{
			return _fixedShotTime;
		}
		set
		{
			_fixedShotTime = value;
		}
	}

	public bool meteoriteEnabled => !_refreshPegHitThisTurn;

	public bool available
	{
		get
		{
			if (CurrentState != FireballState.WAITING)
			{
				return CurrentState == FireballState.AIMING;
			}
			return true;
		}
	}

	public Vector2 preRotatedAimVector => _preRotatedAimVector;

	public bool isMirrorb => _isMirrorb;

	private void Awake()
	{
		_trailRenderer = GetComponentInChildren<TrailRenderer>();
		CheckAndInitializeNormalizaedOrbSizesList();
	}

	private void CheckAndInitializeNormalizaedOrbSizesList()
	{
		if (_normalizedOrbSizesList.Count == 0)
		{
			_normalizedOrbSizesList.Add(0.464f);
			_normalizedOrbSizesList.Add(MultiplyAndRoundTo4Decimals(0.8f, 0.7071f) * 0.58f);
			_normalizedOrbSizesList.Add(0.232f);
			_normalizedOrbSizesList.Add(MultiplyAndRoundTo4Decimals(0.4f, 0.7071f) * 0.58f);
			_normalizedOrbSizesList.Add(0.116f);
			_normalizedOrbSizesList.Add(MultiplyAndRoundTo4Decimals(0.4f, 0.7071f) * 0.29f);
			_normalizedOrbSizesList.Add(MultiplyAndRoundTo4Decimals(MultiplyAndRoundTo4Decimals(0.4f, 0.7071f), 0.7071f) * 0.29f);
		}
	}

	private void OnEnable()
	{
		BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Combine(BattleController.OnBattleEnded, new BattleController.BattleEvent(StartDestroy));
		NavOnlyController.NavigationTriggered = (Action)Delegate.Combine(NavOnlyController.NavigationTriggered, new Action(StartDestroy));
		BattleController.onRefreshPotionActivated = (BattleController.RefreshPotionActivated)Delegate.Combine(BattleController.onRefreshPotionActivated, new BattleController.RefreshPotionActivated(HandleBoardRefresh));
		PlayfieldMouseDetector.OnMouseClicked = (PlayfieldMouseDetector.MouseClicked)Delegate.Combine(PlayfieldMouseDetector.OnMouseClicked, new PlayfieldMouseDetector.MouseClicked(MouseDownOnPlayfield));
		PlayfieldMouseDetector.OnMouseDragged = (PlayfieldMouseDetector.MouseDragged)Delegate.Combine(PlayfieldMouseDetector.OnMouseDragged, new PlayfieldMouseDetector.MouseDragged(MouseDraggedOnPlayfield));
		PachinkoFireButton.OnFireButtonPressed = (PachinkoFireButton.FireButtonPressed)Delegate.Combine(PachinkoFireButton.OnFireButtonPressed, new PachinkoFireButton.FireButtonPressed(FireButtonPressed));
		_radius = GetComponent<CircleCollider2D>().radius * base.transform.localScale.x;
	}

	private void OnDisable()
	{
		BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Remove(BattleController.OnBattleEnded, new BattleController.BattleEvent(StartDestroy));
		NavOnlyController.NavigationTriggered = (Action)Delegate.Remove(NavOnlyController.NavigationTriggered, new Action(StartDestroy));
		BattleController.onRefreshPotionActivated = (BattleController.RefreshPotionActivated)Delegate.Remove(BattleController.onRefreshPotionActivated, new BattleController.RefreshPotionActivated(HandleBoardRefresh));
		PachinkoFireButton.OnFireButtonPressed = (PachinkoFireButton.FireButtonPressed)Delegate.Remove(PachinkoFireButton.OnFireButtonPressed, new PachinkoFireButton.FireButtonPressed(FireButtonPressed));
		PlayfieldMouseDetector.OnMouseClicked = (PlayfieldMouseDetector.MouseClicked)Delegate.Remove(PlayfieldMouseDetector.OnMouseClicked, new PlayfieldMouseDetector.MouseClicked(MouseDownOnPlayfield));
		PlayfieldMouseDetector.OnMouseDragged = (PlayfieldMouseDetector.MouseDragged)Delegate.Remove(PlayfieldMouseDetector.OnMouseDragged, new PlayfieldMouseDetector.MouseDragged(MouseDraggedOnPlayfield));
	}

	public virtual void Init(RelicManager relicManager, DeckManager deckManager, Vector2 previousAimDir, PredictionManager predictionManager, PlayerStatusEffectController playerStatusEffectController)
	{
		_relicManager = relicManager;
		_deckManager = deckManager;
		_previousAimVector = previousAimDir;
		_predictionManager = predictionManager;
		_playerStatusEffectController = playerStatusEffectController;
		_refreshPegHitThisTurn = false;
		if (_relicManager != null && (_relicManager.AttemptUseRelic(RelicEffect.AIM_LIMITER) || _relicManager.AttemptUseRelic(RelicEffect.AIM_LIMITER_MULTIBALL)) && previousAimDir.magnitude < 0.01f)
		{
			_preRotatedAimVector = new Vector2(0.5f, -0.5f).MMRotate(_aimLimiterOffsetAngle);
			_aimVector = _preRotatedAimVector;
		}
		if (_relicManager != null && _relicManager.RelicEffectActive(RelicEffect.ALL_ORBS_MORBID) && GetComponent<ResetPegOnCollision>() == null)
		{
			MorbidPachinkoBehaviour component = base.gameObject.GetComponent<MorbidPachinkoBehaviour>();
			if (component == null)
			{
				base.gameObject.AddComponent<MorbidPachinkoBehaviour>();
			}
			else
			{
				component.morbidLevel++;
			}
		}
		if (_relicManager != null && _relicManager.RelicEffectActive(RelicEffect.ALL_ORBS_DELETE_PEGS) && GetComponent<DestroyPegsOnHit>() == null)
		{
			base.gameObject.AddComponent<DestroyPegsOnHit>();
		}
		InitPredictionAffectingRelicsThatAddComponents();
	}

	public void InitPredictionAffectingRelicsThatAddComponents()
	{
		if (!(_relicManager != null) || !_relicManager.RelicEffectActive(RelicEffect.REVERSE_GRAVITY))
		{
			return;
		}
		if (GravityScale > 0f)
		{
			GravityScale *= -1f;
		}
		if (TryGetComponent<ConcentricationPachinko>(out var _))
		{
			return;
		}
		if (TryGetComponent<GhostOrbPachinko>(out var component2))
		{
			if (!component2.flipAxis)
			{
				component2.bothAxis = true;
			}
		}
		else
		{
			base.gameObject.AddComponent<GhostOrbPachinko>().flipAxis = true;
		}
	}

	public void InitAimLimiterValues(Vector2 previousPreModAim, float currentRotation)
	{
		_aimLimiterOffsetAngle = currentRotation;
		if (previousPreModAim.magnitude > 0.01f)
		{
			_preRotatedAimVector = previousPreModAim;
		}
	}

	public void SetRelicManager(RelicManager relicManager)
	{
		_relicManager = relicManager;
	}

	public void ResetBounces()
	{
		_bounceCount = 0;
	}

	private void Start()
	{
		InitializeMembers();
	}

	public virtual void InitializeMembers()
	{
		_mainCamera = Camera.main;
		_player = ReInput.players.GetPlayer(0);
		_rigid = GetComponent<Rigidbody2D>();
		if ((CurrentState == FireballState.WAITING || CurrentState == FireballState.AIMING) && !IsDummy)
		{
			_rigid.simulated = false;
		}
		_trajectorySimulation = GetComponent<TrajectorySimulation>();
		_wallbounceAudioSource = GetComponent<AudioSource>();
		_circleCastLayerMask = LayerMask.GetMask(_layerMasksToClearWhenPoppingPegs);
		if (!IsDummy && _relicManager != null)
		{
			if (_relicManager.AttemptUseRelic(RelicEffect.ALTERNATE_SHOT_POWER))
			{
				FireForce *= 2f;
			}
			else if (_relicManager.RelicEffectActive(RelicEffect.ALTERNATE_SHOT_POWER))
			{
				FireForce *= 0.5f;
			}
			if (_relicManager.RelicEffectActive(RelicEffect.ORBS_MULTIHIT_BUT_LESS_AIM) && _relicManager.GetRemainingCountdownForRelic(RelicEffect.ORBS_MULTIHIT_BUT_LESS_AIM) == 1)
			{
				if (TryGetComponent<Multihit>(out var component))
				{
					component.multihitLevel++;
				}
				else
				{
					base.gameObject.AddComponent<Multihit>().multihitLevel = 1;
				}
				IncreaseDensity();
			}
		}
		_trajectorySimulation.fireForce = FireForce;
		_trajectorySimulation.mass = _rigid.mass;
		_trajectorySimulation.gravityMod = GravityScale;
		_childMultiballs = new List<PachinkoBall>();
		Vector3 position = base.transform.position;
		position.x -= 0.0005f;
		base.transform.position = position;
		if (_playerStatusEffectController != null && _playerStatusEffectController.HasEffect(StatusEffectType.Confusion))
		{
			_previousAimVector = UnityEngine.Random.insideUnitCircle.normalized;
		}
		else if (_previousAimVector == Vector2.zero)
		{
			_previousAimVector = Vector2.down;
		}
		if (ControllerInputManager.IsPointerBased && !ControllerInputManager.RequireConfirmationToFire)
		{
			Vector3 vector = (base.transform.right = (Vector2)(_mainCamera.ScreenToWorldPoint(Input.mousePosition) - base.transform.position).normalized);
			_aimVector = vector;
		}
		else
		{
			Vector3 vector = (base.transform.right = _previousAimVector);
			_aimVector = vector;
		}
		_currentControllerRotateSpeed = controllerRotateSpeedBase;
	}

	public virtual void SetTrajectorySimulationRadius()
	{
		_trajectorySimulation.radius = base.transform.localScale.x * GetComponent<CircleCollider2D>().radius;
	}

	public void SetAsMirrorbCopiedOrb()
	{
		_isMirrorb = true;
	}

	private void LateUpdate()
	{
		DoLateUpdate();
	}

	public void DoLateUpdate()
	{
		if (PauseMenu.WasPausedAtStartOfFrame || GameBlockingWindow.windowOpen)
		{
			return;
		}
		if (IsDummy && CurrentState != FireballState.FIRING)
		{
			SetFiring();
		}
		switch (CurrentState)
		{
		case FireballState.AIMING:
		{
			bool flag = _playerStatusEffectController != null && _playerStatusEffectController.HasEffect(StatusEffectType.Confusion);
			Transform transform = base.transform;
			if (flag)
			{
				base.transform.Rotate(0f, 0f, confusionRotateSpeed * Time.unscaledDeltaTime);
				_aimVector = transform.right;
			}
			else if (_relicManager != null && (_relicManager.RelicEffectActive(RelicEffect.AIM_LIMITER) || _relicManager.RelicEffectActive(RelicEffect.AIM_LIMITER_MULTIBALL)))
			{
				float x = _preRotatedAimVector.x;
				float y = _preRotatedAimVector.y;
				if (!_mouseDownOnPlayfield && (ControllerInputManager.primaryInputType == InputType.Mouse || ControllerInputManager.primaryInputType == InputType.Touch) && Mathf.Abs(_player.GetAxis("MouseMove")) > 0f)
				{
					Vector2 vector = (_mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
					x = ((vector.x < 0f) ? (-0.5f) : 0.5f);
					y = ((vector.y > 0f) ? 0.5f : (-0.5f));
				}
				else if (!GameBlockingWindow.windowOpen && !ControllerInspection.CurrentlyInspecting)
				{
					float axis = _player.GetAxis("Horizontal");
					float axis2 = _player.GetAxis("Vertical");
					if (Mathf.Abs(axis) >= 0.01f)
					{
						x = ((axis < 0f) ? (-0.5f) : 0.5f);
					}
					if (Mathf.Abs(axis2) >= 0.01f)
					{
						y = ((axis2 < 0f) ? (-0.5f) : 0.5f);
					}
				}
				if (!_fireButtonPressed)
				{
					_preRotatedAimVector = new Vector2(x, y);
					_aimVector = _preRotatedAimVector.MMRotate(_aimLimiterOffsetAngle);
					_trajectorySimulation.enabled = true;
					transform.right = _aimVector;
				}
			}
			else if (ControllerInputManager.primaryInputType == InputType.Mouse && Mathf.Abs(_player.GetAxis("MouseMove")) > 0f && (!ControllerInputManager.RequireConfirmationToFire || PlayfieldMouseDetector.IsMouseDown))
			{
				_aimVector = (_mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
				_trajectorySimulation.enabled = true;
				transform.right = _aimVector;
			}
			else if (!GameBlockingWindow.windowOpen && !ControllerInspection.CurrentlyInspecting)
			{
				float axis3 = _player.GetAxis("Horizontal");
				float axis4 = _player.GetAxis("VerticalSlow");
				float axis5 = _player.GetAxis("HorizontalSlow");
				if (Math.Abs(Mathf.Sign(_player.GetAxisPrev("Horizontal")) - Mathf.Sign(axis3)) > 0.02f)
				{
					_currentControllerRotateSpeed = 0f;
				}
				if (Mathf.Abs(axis4) > 0.065f)
				{
					float z = transform.localRotation.eulerAngles.z;
					float num = 1f;
					if (z > 90f && z < 270f)
					{
						num = -1f;
					}
					ApplyRotationToAimer(axis4 * num * smallMovementForce, keepAccel: false);
				}
				else if (Mathf.Abs(axis3) > 0.065f)
				{
					_currentControllerRotateSpeed = Mathf.MoveTowards(_currentControllerRotateSpeed, controllerRotateSpeedMax, controllerRotateRampSpeed * Time.unscaledDeltaTime);
					ApplyRotationToAimer(axis3 * _currentControllerRotateSpeed, keepAccel: true);
				}
				else if (Mathf.Abs(axis5) > 0.065f)
				{
					ApplyRotationToAimer(axis5 * smallMovementForce, keepAccel: false);
				}
				else
				{
					_currentControllerRotateSpeed = controllerRotateSpeedBase;
				}
			}
			if (!IsDummy && _predictionManager != null)
			{
				if (_shouldInitPrediction)
				{
					GameObject orbToSummon = base.gameObject;
					bool isOrbUninitialized = false;
					bool smallerOrbForPrediction = false;
					SummoningCirclePachinkoBall summoningCirclePachinkoBall = this as SummoningCirclePachinkoBall;
					if (summoningCirclePachinkoBall != null)
					{
						orbToSummon = summoningCirclePachinkoBall.orbToSummon;
						isOrbUninitialized = true;
						smallerOrbForPrediction = _relicManager.RelicEffectActive(RelicEffect.MATRYOSHKA);
					}
					_predictionManager.InitializePachinko(orbToSummon, base.transform.position, isOrbUninitialized, smallerOrbForPrediction);
					bool multiballOnFiredBecomeSmaller = _relicManager.RelicEffectActive(RelicEffect.MATRYOSHKA);
					MultiballOnPrevFiredPath component = GetComponent<MultiballOnPrevFiredPath>();
					if (component != null && component.multiballFireForceMultipliers.Count > 0)
					{
						foreach (float multiballFireForceMultiplier in component.multiballFireForceMultipliers)
						{
							_predictionManager.AddStaticPredictionLine(base.gameObject, transform.position, component.multiballDirection.normalized * FireForce * multiballFireForceMultiplier, multiballOnFiredBecomeSmaller);
						}
					}
					_shouldInitPrediction = false;
				}
				_predictionTimer += Time.deltaTime;
				if (_predictionManager.ResolutionMode == PredictionManager.PredictionResolutionMode.LOW && _predictionTimer >= 0.032f)
				{
					_predictionManager.Predict(base.gameObject, transform.position, _aimVector.normalized * FireForce);
					_predictionTimer = 0f;
				}
				else if (_predictionManager.ResolutionMode == PredictionManager.PredictionResolutionMode.NORMAL && _predictionTimer >= 0.016f)
				{
					_predictionManager.Predict(base.gameObject, transform.position, _aimVector.normalized * FireForce);
					_predictionTimer = 0f;
				}
			}
			if (_player.GetButtonDown("Confirm") && !GameBlockingWindow.wasOpenThisFrame && !PauseMenu.WasPausedAtStartOfFrame && !BattleController.PreBattlePause)
			{
				_fireButtonPressed = true;
			}
			if (_mouseDownOnPlayfield && !BattleController.PreBattlePause)
			{
				if (!ControllerInputManager.RequireConfirmationToFire)
				{
					if (_aimVector.magnitude > 0.01f)
					{
						Fire();
					}
				}
				else if (!flag)
				{
					_aimVector = (_mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
					_trajectorySimulation.enabled = true;
					transform.right = _aimVector;
				}
				_mouseDownOnPlayfield = false;
			}
			else if (_fireButtonPressed && !BattleController.PreBattlePause)
			{
				Fire();
			}
			break;
		}
		case FireballState.FIRING:
			_trajectorySimulation.enabled = false;
			shotTime += Time.deltaTime;
			if (faceVelocity)
			{
				base.transform.right = _rigid.linearVelocity.normalized;
			}
			if (isBattleOrb && (base.transform.position.y < -15f || base.transform.position.y > PegBoardFrame.lostBallYThresholdPos) && !IsDummy)
			{
				UnityEngine.Debug.LogWarning($"Lost a ball of type: {base.gameObject.name}. Destroying from height: {base.transform.position.y}");
				StartDestroy();
			}
			else if (_rigid.linearVelocity.magnitude < 0.25f && !IsDummy)
			{
				if (_unstuckTimer > 1f)
				{
					_stuckTimer = 0f;
				}
				_unstuckTimer = 0f;
				_stuckTimer += Time.deltaTime;
				if (_stuckTimer > 2.5f)
				{
					UnityEngine.Debug.LogWarning($"Killing stuck pachinko ball! Stuck for {_stuckTimer} seconds.");
					StartDestroy();
				}
			}
			else
			{
				_unstuckTimer += Time.deltaTime;
				if (_rigid.linearVelocity.magnitude > 50f)
				{
					_rigid.linearVelocity = _rigid.linearVelocity.normalized * 50f;
				}
			}
			break;
		case FireballState.WAITING:
			break;
		}
	}

	private void FixedUpdate()
	{
		if (CurrentState == FireballState.FIRING)
		{
			_fixedShotTime += Time.fixedDeltaTime;
		}
	}

	private void ApplyRotationToAimer(float zRotate, bool keepAccel)
	{
		if (!keepAccel)
		{
			_currentControllerRotateSpeed = 0f;
		}
		base.transform.Rotate(0f, 0f, zRotate * Time.unscaledDeltaTime);
		_aimVector = base.transform.right;
		_trajectorySimulation.enabled = true;
	}

	public void WaitAndFire(float seconds = 0.1f)
	{
		StartCoroutine(DelayedFire(seconds));
	}

	private IEnumerator DelayedFire(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		Fire(notifyFire: false);
	}

	public bool CheckForApplyingFirstPegHitBuff()
	{
		if (!_hasBuffedFirstPegHit)
		{
			_hasBuffedFirstPegHit = true;
			return true;
		}
		return false;
	}

	public virtual void Fire(bool notifyFire = true)
	{
		Vector3 position = base.transform.position;
		_previousAimVector = _aimVector;
		_currentControllerRotateSpeed = 0f;
		_mouseDownOnPlayfield = false;
		_fireButtonPressed = false;
		_hasBuffedFirstPegHit = false;
		CurrentState = FireballState.FIRING;
		_rigid.simulated = true;
		_rigid.gravityScale = GravityScale;
		_rigid.AddForce(_aimVector.normalized * FireForce);
		shotTime = 0f;
		_fixedShotTime = 0f;
		if (_launchSound != null)
		{
			_wallbounceAudioSource.PlayOneShot(_launchSound);
		}
		if (!IsDummy && _predictionManager != null)
		{
			_predictionManager.PlayerFired();
		}
		if (notifyFire)
		{
			OnShotFired(_aimVector);
		}
		if (BattleController.BattleActive && BattleController.CurrentBattleState != BattleController.BattleState.NAVIGATION)
		{
			HandleMultiballsOnFired(position);
		}
	}

	public void FireDummy()
	{
		_rigid.gravityScale = GravityScale;
		_rigid.AddForce(_aimVector.normalized * FireForce);
		shotTime = 0f;
	}

	private void MouseDownOnPlayfield()
	{
		if (!BattleController.PreBattlePause)
		{
			_mouseDownOnPlayfield = true;
		}
	}

	private void MouseDraggedOnPlayfield()
	{
		if (ControllerInputManager.RequireConfirmationToFire && !BattleController.PreBattlePause)
		{
			_mouseDownOnPlayfield = true;
		}
	}

	public void FireButtonPressed()
	{
		_fireButtonPressed = true;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		BlackHoleNavigation component;
		if (other.GetComponent<Destroyer>() != null && !IsDummy)
		{
			StartDestroy();
		}
		else if (other.GetComponent<Destroyer>() != null && IsDummy)
		{
			HasDummyCollided = true;
		}
		else if (other.GetComponent<SlimeOnlyPeg>() != null && !IsDummy)
		{
			OnPachinkoBallSlimeOnlyPegHit?.Invoke(other.GetComponent<SlimeOnlyPeg>(), this);
		}
		else if (other.TryGetComponent<BlackHoleNavigation>(out component) && !IsDummy)
		{
			if (BattleController.CurrentBattleState == BattleController.BattleState.NAVIGATION)
			{
				BattleController battleController = UnityEngine.Object.FindObjectOfType<BattleController>();
				if (battleController != null)
				{
					battleController.blackHoleNav = component.GetNavigationDirection();
				}
			}
			if (!AchievementData.Achievements[AchievementData.AchievementId.SEND_DEMON_SQUIRREL_TO_HOLE].Unlocked && (bool)GetComponent<DemonSquirrelBlackHoleAchievementChecker>())
			{
				AchievementManager.Instance.UnlockAchievement(AchievementData.AchievementId.SEND_DEMON_SQUIRREL_TO_HOLE);
			}
			StartDestroy();
		}
		else if (other.GetComponent<BlackHoleNavigation>() != null && IsDummy)
		{
			HasDummyCollided = true;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		_stopwatch.Restart();
		if (IsDummy)
		{
			if (_isColliding)
			{
				return;
			}
			_isColliding = true;
			OnPachinkoBallDummyBounce(collision.contacts[0].point, base.transform.position);
			_bounceCount++;
			if (_bounceCount >= MaxBounceCount)
			{
				_bounceCount = 0;
				HasDummyCollided = true;
			}
		}
		if (CurrentState != FireballState.FIRING)
		{
			return;
		}
		bool flag = false;
		if (collision.collider.CompareTag("Wall") && !IsDummy)
		{
			float volume = Mathf.Clamp(_rigid.linearVelocity.magnitude / 20f, 0.05f, 0.65f);
			OnPachinkoWallBounceAudio(volume);
			OnPachinkoBallWallBounce(base.transform.position);
			StartCoroutine(WaitAndTrackLastPegHit(null));
		}
		else if (collision.collider.CompareTag("Bouncer") && !IsDummy)
		{
			float volume2 = Mathf.Clamp(_rigid.linearVelocity.magnitude / 20f, 0.05f, 0.65f);
			OnPachinkoWallBounceAudio(volume2);
			OnPachinkoBallBouncerBounce(base.transform.position);
			StartCoroutine(WaitAndTrackLastPegHit(null));
		}
		else if (collision.collider.CompareTag("Peg") || collision.collider.CompareTag("Bomb"))
		{
			Peg componentInParent = collision.collider.GetComponentInParent<Peg>();
			Bomb componentInParent2 = collision.collider.GetComponentInParent<Bomb>();
			if (componentInParent != null)
			{
				LongPeg longPeg = componentInParent as LongPeg;
				if (longPeg != null && longPeg.hit)
				{
					if (!IsDummy)
					{
						StartCoroutine(WaitAndTrackLastPegHit(componentInParent.gameObject));
					}
					return;
				}
				if (!IsDummy)
				{
					if (IsOrbLargerThanDefault() && _deckManager != null && _relicManager != null && _relicManager.AttemptUseRelic(RelicEffect.LARGE_ORBS_DAMAGE_TARGETED))
					{
						TargetingManager.OnDamageTargetedEnemy?.Invoke(Mathf.Max(1, _deckManager.GetNumberOfLargeOrbsInDeck()), Enemy.EnemyDamageSource.RelicChip);
					}
					OnPachinkoBallPegHit?.Invoke(componentInParent, this);
					StartCoroutine(WaitAndTrackLastPegHit(componentInParent.gameObject));
				}
				flag = componentInParent.slimeType != Peg.SlimeType.None;
				if (componentInParent.slimeType == Peg.SlimeType.Web)
				{
					_rigid.linearVelocity *= 0.4f;
				}
				if (componentInParent.pegType == Peg.PegType.CRIT && _relicManager != null && _relicManager.RelicEffectActive(RelicEffect.CRIT_PIT))
				{
					_rigid.mass = 20f;
					_rigid.linearDamping = 2f;
					_rigid.gravityScale = 4.2f;
					_rigid.linearVelocity = Vector3.down * 2f;
				}
			}
			HandleSpawningMultiballs(collision, componentInParent, componentInParent2);
		}
		if (!GetComponent<EggPachinko>() || flag || !(collision.collider.GetComponentInParent<Bomb>() == null))
		{
			return;
		}
		if (!IsDummy)
		{
			StartDestroy();
			_renderer.enabled = true;
			GetComponentInChildren<Animator>().SetTrigger(Crack);
			StartCoroutine(DisablePachinkoTrail());
			LongPeg component = collision.collider.GetComponent<LongPeg>();
			if (component != null)
			{
				StartCoroutine(ManuallyDisableLongPegHit(component));
			}
			if (!isMirrorb && !GetComponent<PersistentOrb>().shouldPersist)
			{
				OnPachinkoBallPermanentDestruction?.Invoke(base.gameObject);
			}
		}
		else
		{
			HasDummyCollided = true;
		}
	}

	private IEnumerator WaitAndTrackLastPegHit(GameObject hitPeg)
	{
		yield return new WaitForEndOfFrame();
		lastHitPeg = hitPeg;
	}

	public static Vector3 GetNewMultiballScale(Vector3 oldScale, float sizeReductionMultiplier)
	{
		return new Vector3(MultiplyAndRoundTo4Decimals(oldScale.x, sizeReductionMultiplier), MultiplyAndRoundTo4Decimals(oldScale.y, sizeReductionMultiplier), oldScale.z);
	}

	private static float MultiplyAndRoundTo4Decimals(float x, float y)
	{
		return Mathf.Round(x * y * 10000f) / 10000f;
	}

	private void HandleSpawningMultiballs(Collision2D collision, Peg hitPeg, Bomb potentialBomb)
	{
		List<GameObject> list = new List<GameObject>();
		if (multiballLevel > 0 && collision.contacts.Length != 0 && !IsDummy)
		{
			multiballLevel--;
			GameObject multiballGameobject = base.gameObject;
			MultiballPrefabOverridePool component = GetComponent<MultiballPrefabOverridePool>();
			bool flag = component != null;
			if (flag)
			{
				multiballGameobject = component.GetMultiballGameobject();
			}
			GameObject gameObject = SpawnMultiball(multiballGameobject, collision, copyOverMultiballLevel: true);
			if (gameObject != null)
			{
				if (flag)
				{
					component.AddCopyOfComponentToOrb(gameObject);
				}
				list.Add(gameObject);
			}
		}
		if (!IsDummy && _relicManager != null && _relicManager.RelicEffectActive(RelicEffect.CREATE_SQUIRRELS_ON_PEGS_HIT) && BattleController.BattleActive && BattleController.CurrentBattleState != BattleController.BattleState.NAVIGATION && --_relicManager.currentHitsNeededToSpawnSquirrel <= 0 && _relicManager.AttemptUseRelic(RelicEffect.CREATE_SQUIRRELS_ON_PEGS_HIT))
		{
			_relicManager.currentHitsNeededToSpawnSquirrel = 30;
			GameObject gameObject2 = SpawnMultiball(_relicManager.squirrelball, collision, copyOverMultiballLevel: false);
			if (gameObject2 != null)
			{
				list.Add(gameObject2);
			}
		}
		ConvertPegsToGold component2 = GetComponent<ConvertPegsToGold>();
		if (component2 != null && component2.convertPeg != null)
		{
			Vector3 centerOfPeg = component2.convertPeg.GetCenterOfPeg();
			component2.convertPeg.CollectExtraCoins(component2.extraCoinsOnConversion);
			component2.convertPeg.convertedToGold = true;
			component2.convertPeg = null;
			GameObject gameObject3 = SpawnMultiball(component2.goldPegPrefab, collision, copyOverMultiballLevel: false);
			if (gameObject3 != null)
			{
				gameObject3.transform.position = centerOfPeg;
			}
		}
		if (list.Count > 0)
		{
			Vector3 a = Vector2.Perpendicular(base.transform.right).normalized;
			if (a.y < 0f)
			{
				a *= -1f;
			}
			float num = 0f;
			ContactPoint2D[] contacts = collision.contacts;
			foreach (ContactPoint2D contactPoint2D in contacts)
			{
				num += contactPoint2D.normalImpulse;
			}
			num /= Time.fixedDeltaTime;
			if (hitPeg is RegularPeg && !_refreshPegHitThisTurn && _relicManager != null && _relicManager.RelicEffectActive(RelicEffect.BOMB_FORCE_ALWAYS))
			{
				RegularPeg regularPeg = hitPeg as RegularPeg;
				num += regularPeg.bombRelicFlingForce;
			}
			else if (potentialBomb != null && potentialBomb.ShouldExplode())
			{
				num += collision.collider.GetComponent<Bomb>().bombFlingForce;
			}
			Vector3 normalized = base.transform.right.normalized;
			float num2 = list.Count;
			for (int j = 0; j < list.Count; j++)
			{
				float t = (float)j / num2;
				Vector2 vector = Vector3.Slerp(a, normalized, t).normalized;
				list[j].GetComponent<Rigidbody2D>().AddForce(vector * num * MultiballForceMod);
			}
		}
	}

	private GameObject SpawnMultiball(GameObject multiballGameObject, Collision2D collision, bool copyOverMultiballLevel)
	{
		PachinkoBall component = multiballGameObject.GetComponent<PachinkoBall>();
		MultiballsGetSmaller component2 = multiballGameObject.GetComponent<MultiballsGetSmaller>();
		if (((_relicManager != null && _relicManager.RelicEffectActive(RelicEffect.MATRYOSHKA)) || component2 != null) && (component == null || !component.CanOrbBeShrunk()))
		{
			return null;
		}
		Vector3 vector = Vector2.Perpendicular(base.transform.right).normalized;
		if (vector.y < 0f)
		{
			vector *= -1f;
		}
		Vector2 vector2 = collision.contacts[0].point + (Vector2)vector * 0.58f;
		if (BattleController.BattleActive)
		{
			if (vector2.x > StaticGameData.battleXBounds.y)
			{
				vector2.x = 7.75f;
			}
			else if (vector2.x < StaticGameData.battleXBounds.x)
			{
				vector2.x = -7.75f;
			}
			if (SpiritOfRadiaBoss.CurrentPhase == 2)
			{
				if (vector2.y > StaticGameData.act4BossPhase2TallerYBounds[1])
				{
					vector2.y = StaticGameData.act4BossPhase2TallerYBounds[1] - 0.5f;
				}
			}
			else if (vector2.y > 4.5f)
			{
				vector2.y = 4f;
			}
		}
		GameObject obj = UnityEngine.Object.Instantiate(multiballGameObject, vector2, base.transform.rotation);
		PachinkoBall component3 = obj.GetComponent<PachinkoBall>();
		if (_relicManager != null && _relicManager.RelicEffectActive(RelicEffect.MATRYOSHKA) && !component2)
		{
			if (isOrigionalSize && CanOrbBeShrunk())
			{
				StartCoroutine(WaitAndShrink());
				isOrigionalSize = false;
			}
			if (CanOrbBeShrunk())
			{
				StartCoroutine(WaitAndShrink());
				isOrigionalSize = false;
			}
		}
		if ((bool)component2 || (_relicManager != null && _relicManager.RelicEffectActive(RelicEffect.MATRYOSHKA)))
		{
			if (component3.isOrigionalSize && component3.CanOrbBeShrunk())
			{
				component3.ReduceOrbSize();
				component3.isOrigionalSize = false;
			}
			if (component3.CanOrbBeShrunk())
			{
				component3.ReduceOrbSize();
				component3.isOrigionalSize = false;
			}
		}
		component3.Init(_relicManager, _deckManager, _aimVector, _predictionManager, _playerStatusEffectController);
		component3._refreshPegHitThisTurn = _refreshPegHitThisTurn;
		Rigidbody2D component4 = obj.GetComponent<Rigidbody2D>();
		component4.simulated = true;
		component4.gravityScale = component3.GravityScale;
		if (copyOverMultiballLevel)
		{
			component3.multiballLevel = multiballLevel;
		}
		if (TryGetComponent<NewSpritesForMultiballs>(out var component5))
		{
			_renderer.sprite = component5.multiballChildSprite1;
			component3._renderer.sprite = component5.multiballChildSprite2;
		}
		component3.SetFiring();
		if (obj.TryGetComponent<IceOrbPachinko>(out var component6))
		{
			component6.enabled = false;
			component6.GetComponentInChildren<SpriteRenderer>().sprite = sprite;
			component6.gameObject.layer = base.gameObject.layer;
			component6.donePierce = true;
		}
		_childMultiballs.Add(component3);
		OnAdditionalPachinkoBallCreated();
		return obj;
	}

	private IEnumerator WaitAndShrink()
	{
		yield return WaitObjectAllocator.waitForEndOfFrame;
		if (CanOrbBeShrunk())
		{
			ReduceOrbSize();
		}
	}

	protected void HandleMultiballsOnFired(Vector3 fireFromPosition)
	{
		if (_relicManager != null && _relicManager.AttemptUseRelic(RelicEffect.AIM_LIMITER_MULTIBALL))
		{
			Vector3 vector = Vector2.Perpendicular(_aimVector).normalized;
			SpawnOnFiredMultiball(base.gameObject, base.transform.position, Vector3.Slerp(_aimVector.normalized, vector, 0.18f).normalized * FireForce);
			SpawnOnFiredMultiball(base.gameObject, base.transform.position, Vector3.Slerp(_aimVector.normalized, -vector, 0.18f).normalized * FireForce);
		}
		MultiballOnPrevFiredPath component = GetComponent<MultiballOnPrevFiredPath>();
		if (!(component != null) || component.multiballFireForceMultipliers.Count <= 0)
		{
			return;
		}
		foreach (float multiballFireForceMultiplier in component.multiballFireForceMultipliers)
		{
			Vector2 multiballFireForce = component.multiballDirection.normalized * FireForce * multiballFireForceMultiplier;
			SpawnOnFiredMultiball(base.gameObject, fireFromPosition, multiballFireForce);
		}
	}

	protected virtual void SpawnOnFiredMultiball(GameObject gameObject, Vector3 fireFromPosition, Vector2 multiballFireForce)
	{
		SpawnMultiballFromLocation(gameObject, fireFromPosition, multiballFireForce);
	}

	public GameObject SpawnMultiballFromLocation(GameObject multiballGameObject, Vector3 fireFromPosition, Vector2 fireForce, Attack customAttack = null, bool callCreatedDelegate = true)
	{
		if (callCreatedDelegate)
		{
			OnAdditionalPachinkoBallCreated();
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(multiballGameObject.gameObject, fireFromPosition, Quaternion.identity);
		PachinkoBall component = gameObject.GetComponent<PachinkoBall>();
		if (_relicManager != null && _relicManager.RelicEffectActive(RelicEffect.MATRYOSHKA))
		{
			if (component.CanOrbBeShrunk())
			{
				component.ReduceOrbSize();
				component.isOrigionalSize = false;
			}
			if (component.CanOrbBeShrunk())
			{
				component.ReduceOrbSize();
				component.isOrigionalSize = false;
			}
		}
		gameObject.GetComponent<TrajectorySimulation>().enabled = false;
		component.Init(_relicManager, _deckManager, _previousAimVector, _predictionManager, _playerStatusEffectController);
		if (customAttack != null)
		{
			int num = 0;
			if (_relicManager.RelicEffectActive(RelicEffect.MATRYOSHKA))
			{
				num += _relicManager.MATRYOSHKA_MULTIBALL_LEVEL;
			}
			if (_relicManager.RelicEffectActive(RelicEffect.NEW_MATRYORBSHKA))
			{
				num++;
			}
			Multiball component2 = multiballGameObject.GetComponent<Multiball>();
			if (component2 != null)
			{
				num += component2.multiballLevel;
			}
			component.multiballLevel = num;
		}
		else
		{
			customAttack = multiballGameObject.GetComponent<Attack>();
			component.multiballLevel = multiballGameObject.GetComponent<PachinkoBall>().multiballLevel;
		}
		if (component.TryGetComponent<Attack>(out var component3))
		{
			component3.CopyValuesFromDeckAttack(customAttack);
		}
		UnlimitedUpgrades component4 = multiballGameObject.GetComponent<UnlimitedUpgrades>();
		if (component4 != null)
		{
			gameObject.GetComponent<UnlimitedUpgrades>().timesUpgraded = component4.timesUpgraded;
		}
		ResolveMagnetBehavior(gameObject);
		if (component.TryGetComponent<GuaranteedCrit>(out var _))
		{
			BattleController.OnExternalCritTriggerRequest();
		}
		Rigidbody2D component6 = gameObject.GetComponent<Rigidbody2D>();
		component6.simulated = true;
		component6.gravityScale = component.GravityScale;
		component6.AddForce(fireForce);
		component.SetFiring();
		if (gameObject.TryGetComponent<GhostOrbPachinko>(out var component7))
		{
			component7.ForceUpdateGhostEffect(fireForce);
		}
		_childMultiballs.Add(component);
		return gameObject;
	}

	public bool CanOrbBeShrunk(int numShrinks = 1)
	{
		float num = 0.58f;
		if (TryGetComponent<CircleCollider2D>(out var component))
		{
			num = component.radius;
		}
		Vector3 oldScale = base.transform.localScale;
		for (int i = 0; i < numShrinks; i++)
		{
			oldScale = GetNewMultiballScale(oldScale, 0.7071f);
		}
		float num2 = oldScale.x * num;
		CheckAndInitializeNormalizaedOrbSizesList();
		return num2 >= _normalizedOrbSizesList[_normalizedOrbSizesList.Count - 1];
	}

	public void ReduceOrbSize()
	{
		float num = 0.58f;
		if (TryGetComponent<CircleCollider2D>(out var component))
		{
			num = component.radius;
		}
		Vector3 newMultiballScale = GetNewMultiballScale(base.transform.localScale, 0.7071f);
		float num2 = newMultiballScale.x * num;
		CheckAndInitializeNormalizaedOrbSizesList();
		if (TryGetComponent<MultiballsGetSmaller>(out var component2))
		{
			SpriteRenderer componentInChildren = GetComponentInChildren<SpriteRenderer>();
			Sprite[] spritesList = component2.spritesList;
			Sprite sprite = componentInChildren.sprite;
			float num3 = componentInChildren.transform.localScale.x;
			int num4 = 0;
			foreach (float normalizedOrbSizes in _normalizedOrbSizesList)
			{
				if (spritesList[num4] != null)
				{
					sprite = spritesList[num4];
					num3 = MultiballsGetSmaller.SPRITE_SCALE_FOR_SIZES[num4];
				}
				if (num2 > normalizedOrbSizes - 0.01f)
				{
					break;
				}
				num4++;
			}
			if (num4 == 4 && componentInChildren.sprite != sprite)
			{
				newMultiballScale *= 2f;
				component.radius /= 2f;
			}
			componentInChildren.sprite = sprite;
			componentInChildren.transform.localScale = new Vector3(num3, num3, componentInChildren.transform.localScale.z);
		}
		List<float> normalizedOrbSizesList = _normalizedOrbSizesList;
		if (num2 < normalizedOrbSizesList[normalizedOrbSizesList.Count - 1] - 0.01f)
		{
			UnityEngine.Debug.LogError("Pachinko " + base.name + " multiball not spawned because it's size is too small. Use CanOrbBeShrunk() to check before calling this method.");
			return;
		}
		base.transform.localScale = newMultiballScale;
		if (num2 < 0.106f)
		{
			TrailRenderer componentInChildren2 = GetComponentInChildren<TrailRenderer>();
			float value = base.transform.localScale.x * (num / 0.58f) - 0.01f;
			AnimationCurve animationCurve = new AnimationCurve();
			animationCurve.AddKey(new Keyframe(componentInChildren2.widthCurve.keys[0].time, value));
			animationCurve.AddKey(new Keyframe(componentInChildren2.widthCurve.keys[componentInChildren2.widthCurve.keys.Length - 1].time, 0f));
			componentInChildren2.widthCurve = animationCurve;
		}
	}

	public bool IsOrbSmallerThanDefault()
	{
		PachinkoBall pachinkoBall = this;
		if (TryGetComponent<CopyOrbOnReload>(out var component) && component.copiedOrb != null)
		{
			pachinkoBall = component.copiedOrb;
		}
		if (pachinkoBall.TryGetComponent<SnowballDmgAndSize>(out var component2))
		{
			component2.UpdateSize();
		}
		float num = 0.58f;
		if (pachinkoBall.TryGetComponent<CircleCollider2D>(out var component3))
		{
			num = component3.radius;
		}
		return pachinkoBall.transform.localScale.y * num - 0.232f < -0.001f;
	}

	public bool IsOrbLargerThanDefault()
	{
		PachinkoBall pachinkoBall = this;
		if (TryGetComponent<CopyOrbOnReload>(out var component) && component.copiedOrb != null)
		{
			pachinkoBall = component.copiedOrb;
		}
		if (pachinkoBall.TryGetComponent<SnowballDmgAndSize>(out var component2))
		{
			component2.UpdateSize();
		}
		float num = 0.58f;
		if (pachinkoBall.TryGetComponent<CircleCollider2D>(out var component3))
		{
			num = component3.radius;
		}
		return pachinkoBall.transform.localScale.y * num - 0.232f > 0.001f;
	}

	private void ResolveMagnetBehavior(GameObject orb)
	{
		MagnetBehavior component = orb.GetComponent<MagnetBehavior>();
		if (_relicManager.RelicEffectActive(RelicEffect.PEG_MAGNET) && component == null)
		{
			orb.AddComponent<MagnetBehavior>().Initialize(4f, 8f);
		}
		else if (_relicManager.RelicEffectActive(RelicEffect.PEG_MAGNET) && component != null)
		{
			component.Initialize(4f, 8f);
		}
		else if (component != null)
		{
			component.Initialize(0f, 0f);
		}
	}

	public void DestroyMultiballsAndSelf()
	{
		if (base.isActiveAndEnabled)
		{
			StartCoroutine(CoroutineDestroyMultiballsAndSelf());
		}
		else
		{
			DestroyMultiballsAndSelfRecursive();
		}
	}

	private void DestroyMultiballsAndSelfRecursive()
	{
		if (_childMultiballs != null && _childMultiballs.Count > 0)
		{
			foreach (PachinkoBall childMultiball in _childMultiballs)
			{
				childMultiball.DestroyMultiballsAndSelfRecursive();
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator CoroutineDestroyMultiballsAndSelf()
	{
		if (_currentlyDestroyingMultiballs)
		{
			yield break;
		}
		_currentlyDestroyingMultiballs = true;
		if (_childMultiballs != null && _childMultiballs.Count > 0)
		{
			foreach (PachinkoBall childMultiball in _childMultiballs)
			{
				yield return StartCoroutine(childMultiball.CoroutineDestroyMultiballsAndSelf());
				yield return null;
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
		_currentlyDestroyingMultiballs = false;
	}

	public void ForceEndShotDestroyMultiballs()
	{
		if (_childMultiballs == null || _childMultiballs.Count <= 0)
		{
			return;
		}
		foreach (PachinkoBall childMultiball in _childMultiballs)
		{
			childMultiball.ForceEndShotDestroyMultiballs();
			childMultiball.StartDestroy();
		}
	}

	private void ClearOverlappingPegs()
	{
		if (IsDummy)
		{
			return;
		}
		Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, _radius, _circleCastLayerMask);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				Peg component = array[i].GetComponent<Peg>();
				if (component != null && component.overlapActivationsThisShot < 10)
				{
					component.PegActivated(playAudio: true, forcePop: true);
					component.overlapActivationsThisShot++;
				}
			}
		}
	}

	public void TeleportTo(Vector3 position)
	{
		_trailRenderer.emitting = false;
		base.transform.position = position;
		_trailRenderer.Clear();
		StartCoroutine(WaitAndEnablePachinkoTrailEmit());
	}

	private IEnumerator DisablePachinkoTrail()
	{
		yield return new WaitForSeconds(0.4f);
		_trailRenderer.enabled = false;
	}

	private IEnumerator WaitAndEnablePachinkoTrailEmit()
	{
		yield return null;
		_trailRenderer.emitting = true;
	}

	private IEnumerator ManuallyDisableLongPegHit(LongPeg longpeg)
	{
		yield return WaitObjectAllocator.waitForEndOfFrame;
		longpeg.ManuallyEndHit();
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if (_isColliding && IsDummy)
		{
			_isColliding = false;
		}
	}

	public void StartDestroy()
	{
		_rigid.gravityScale = 0f;
		_rigid.linearVelocity = Vector2.zero;
		CurrentState = FireballState.AWAITING_RESULTS;
		_renderer.enabled = false;
		Collider2D[] components = GetComponents<Collider2D>();
		if (components != null && components.Length != 0)
		{
			Collider2D[] array = components;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}
		LineRenderer componentInChildren = GetComponentInChildren<LineRenderer>();
		if (componentInChildren != null)
		{
			componentInChildren.gameObject.SetActive(value: false);
		}
		if (_trailRenderer != null)
		{
			_trailRenderer.enabled = false;
		}
		if (!IsDummy)
		{
			OnPachinkoBallDestroyed(this);
			if (TryGetComponent<RefreshBoardOnPachinkoExit>(out var component))
			{
				component.RequestRefresh();
			}
			if (TryGetComponent<GrantStatusOnPachinkoExit>(out var component2))
			{
				PlayerStatusEffectController.ExternalStatusEffectsAdditionRequest?.Invoke(new StatusEffect[1] { component2.statusToGrant }, StatusEffectSource.PLAYER);
			}
		}
		BattleController.onRefreshPotionActivated = (BattleController.RefreshPotionActivated)Delegate.Remove(BattleController.onRefreshPotionActivated, new BattleController.RefreshPotionActivated(HandleBoardRefresh));
	}

	private void HandleBoardRefresh()
	{
		if (IsFiring())
		{
			_refreshPegHitThisTurn = true;
			ClearOverlappingPegs();
		}
	}

	public void Arm()
	{
		if (CurrentState == FireballState.WAITING)
		{
			OnPachinkoBallArmed?.Invoke(this);
			CurrentState = FireballState.AIMING;
			_predictionManager.UpdateAllPegsStatus();
			_predictionManager.ReenableDisabledUpdateables();
			_predictionManager.SetLineRendererStatus(status: true);
			_shouldInitPrediction = true;
		}
	}

	public void IncreaseDensity()
	{
		Rigidbody2D rigidbody2D = _rigid;
		if (rigidbody2D == null)
		{
			rigidbody2D = GetComponent<Rigidbody2D>();
		}
		if (rigidbody2D.mass < 1.2f)
		{
			rigidbody2D.mass = 1.2f;
		}
		else if (rigidbody2D.mass < 1.8f)
		{
			rigidbody2D.mass = 1.8f;
		}
		else if (rigidbody2D.mass < 2.2f)
		{
			rigidbody2D.mass = 2.2f;
		}
		else if (rigidbody2D.mass < 2.4f)
		{
			rigidbody2D.mass = 2.4f;
		}
		else
		{
			rigidbody2D.mass += 0.1f;
		}
	}

	public void SetFiring()
	{
		CurrentState = FireballState.FIRING;
	}

	public bool IsFiring()
	{
		return CurrentState == FireballState.FIRING;
	}

	public bool IsAiming()
	{
		return CurrentState == FireballState.AIMING;
	}

	public bool IsAwaitingResults()
	{
		return CurrentState == FireballState.AWAITING_RESULTS;
	}
}
