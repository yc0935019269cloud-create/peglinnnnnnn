using System;
using System.Collections;
using Battle;
using Battle.Enemies;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Act4BossPegBoardFrameManager : PegBoardFrame
{
	private static readonly int Crack = Animator.StringToHash("crack");

	private static readonly float _wallAnimationDuration = 2.35f;

	[SerializeField]
	private GameObject _invisCollision;

	[SerializeField]
	private GameObject _roofTiles;

	[SerializeField]
	private GameObject _floorTiles;

	[SerializeField]
	private GameObject _island;

	[SerializeField]
	private GameObject _crystalWalls;

	[SerializeField]
	private GameObject _plungerWall;

	[SerializeField]
	private GameObject _rightWall;

	[SerializeField]
	private GameObject _leftWall;

	[SerializeField]
	private GameObject _voidWalls;

	[SerializeField]
	private GameObject _floorCollision;

	[SerializeField]
	private TilemapCollider2D _phase2RoofCollision;

	[SerializeField]
	private GameObject _regular2DRoofCollision;

	[SerializeField]
	private GameObject _regular2DLeftCollision;

	[SerializeField]
	private GameObject _regular2DRightCollision;

	[SerializeField]
	private SpriteRenderer _fadeSprite;

	[SerializeField]
	private ParticleSystem[] _particleSystems;

	[SerializeField]
	private GameObject _frameObjectHolders;

	[SerializeField]
	private AudioSource _droneAudioSource;

	[SerializeField]
	private AudioSource _earthquakeAudioSource;

	[SerializeField]
	private AudioSource _bigCrystalBreakAudioSource;

	private Camera _camera;

	private bool _isSpedUp;

	private int _phase2SpecialLayer;

	private int _sidescrollerLayer;

	private int _pegboardMainLayer;

	protected override void OnEnable()
	{
		_sidescrollerLayer = SortingLayer.NameToID("Sidescroller");
		_phase2SpecialLayer = SortingLayer.NameToID("Phase2Special");
		_pegboardMainLayer = SortingLayer.NameToID("PegboardMain");
		base.OnEnable();
		_camera = Camera.main;
		PegBoardFrame.lostBallYThresholdPos = 6f;
	}

	public void RegisterCallbacks()
	{
		SpiritOfRadiaBoss.PreTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Combine(SpiritOfRadiaBoss.PreTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(StartPhase2PreTransition));
		SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Combine(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(StartPhase2Transition));
	}

	public void OnDestroy()
	{
		Debug.Log("Calling DestroyCallbacks");
		SpiritOfRadiaBoss.PreTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Remove(SpiritOfRadiaBoss.PreTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(StartPhase2PreTransition));
		SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Remove(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(StartPhase2Transition));
	}

	private void StartPhase2Transition()
	{
		StartCoroutine(Phase2Transition());
	}

	private void StartPhase2PreTransition()
	{
		StartCoroutine(Phase2PreTransition());
	}

	private IEnumerator Phase2PreTransition()
	{
		SpiritOfRadiaBoss boss = UnityEngine.Object.FindObjectOfType<SpiritOfRadiaBoss>();
		PlayerHealthController peglin = UnityEngine.Object.FindObjectOfType<PlayerHealthController>();
		_isSpedUp = TimescaleManager.Instance.isSpedUp;
		if (_isSpedUp)
		{
			TimescaleManager.Instance.ManualSpeedupToggle();
		}
		TimescaleManager.Instance.SetSpeedupToggleBlock(value: true);
		_droneAudioSource.Play();
		_earthquakeAudioSource.Play();
		_bigCrystalBreakAudioSource.Play();
		Animator component = _plungerWall.GetComponent<Animator>();
		Animator component2 = _rightWall.GetComponent<Animator>();
		_floorTiles.GetComponent<Animator>().SetTrigger(Crack);
		component.SetTrigger(Crack);
		component2.SetTrigger(Crack);
		yield return new WaitForSeconds(_wallAnimationDuration);
		ParticleSystem[] particleSystems = _particleSystems;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			particleSystems[i].Play();
		}
		peglin.GetComponent<SpriteRenderer>().sortingLayerID = _phase2SpecialLayer;
		boss.GetComponent<SpriteRenderer>().sortingLayerID = _phase2SpecialLayer;
		_floorTiles.SetActive(value: false);
		_crystalWalls.SetActive(value: false);
		yield return new WaitForSeconds(1f);
		_fadeSprite.DOFade(1f, 1f);
		yield return new WaitForSeconds(1f);
		boss.gameObject.transform.DOMove(new Vector3(boss.phase2EyePos.x, boss.phase2EyePos.y, base.transform.position.z), 2f).SetEase(Ease.InCubic);
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(boss.StartPhase2Transition());
	}

	private IEnumerator Phase2Transition()
	{
		PegBoardFrame.lostBallYThresholdPos = 5.5f + SpiritOfRadiaBoss.EXTRA_PEGBOARD_HEIGHT;
		wallsCollider.transform.position = new Vector3(wallsCollider.transform.position.x, wallsCollider.transform.position.y + SpiritOfRadiaBoss.EXTRA_PEGBOARD_HEIGHT, wallsCollider.transform.position.z);
		_invisCollision.transform.position = new Vector3(_invisCollision.transform.position.x, _invisCollision.transform.position.y + SpiritOfRadiaBoss.EXTRA_PEGBOARD_HEIGHT, _invisCollision.transform.position.z);
		_floorCollision.transform.position = new Vector3(_floorCollision.transform.position.x, _floorCollision.transform.position.y - 4.57f, _floorCollision.transform.position.z);
		_roofTiles.SetActive(value: false);
		_floorTiles.SetActive(value: false);
		_crystalWalls.SetActive(value: false);
		_voidWalls.SetActive(value: true);
		_regular2DRoofCollision.SetActive(value: true);
		_regular2DLeftCollision.SetActive(value: true);
		_regular2DRightCollision.SetActive(value: true);
		_fadeSprite.DOFade(0f, 1f);
		yield return new WaitForSeconds(2f);
		TimescaleManager.Instance.SetSpeedupToggleBlock(value: false);
		if (_isSpedUp)
		{
			TimescaleManager.Instance.ManualSpeedupToggle();
		}
		SpiritOfRadiaBoss spiritOfRadiaBoss = UnityEngine.Object.FindObjectOfType<SpiritOfRadiaBoss>();
		UnityEngine.Object.FindObjectOfType<PlayerHealthController>().GetComponent<SpriteRenderer>().sortingLayerID = _sidescrollerLayer;
		spiritOfRadiaBoss.GetComponent<SpriteRenderer>().sortingLayerID = _pegboardMainLayer;
		UnityEngine.Object.FindObjectOfType<PredictionManager>().DestroyFakePegboard(_frameObjectHolders);
	}

	public void LookLikeChestForMimic()
	{
		_plungerWall.SetActive(value: false);
		_leftWall.SetActive(value: true);
	}

	public void OnMimicOpened()
	{
		_plungerWall.SetActive(value: true);
		_leftWall.SetActive(value: false);
	}
}
