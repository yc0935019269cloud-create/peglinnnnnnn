using System;
using System.Collections;
using Battle.Attacks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using I2.Loc;
using UnityEngine;

public class SummoningCirclePachinkoBall : PachinkoBall
{
	private const int MAX_NUMBER_OF_COPIES = 16;

	private const float SPRITE_SPAWN_SCALE_DURATION = 0.4f;

	private const float SUMMON_EFFECTS_STARTING_SCALE = 0.4f;

	private const float SUMMON_EFFECTS_LINGER_LENGTH = 1f;

	public int numberOfCopiesToSummon = 1;

	public float timeBetweenOrbSpawns = 0.5f;

	public GameObject summonEffects;

	public AudioClip orbSummonSound;

	[SerializeField]
	private GameObject defaultOrbToSummon;

	[HideInInspector]
	public DeckManager deckManager;

	private Vector3 _summonPosition = Vector3.zero;

	private GameObject _orbToSummon;

	private bool hasBeenInitialized;

	private Attack _copiedOrbsAttack;

	[HideInInspector]
	public bool addDuplicationStationMultiball;

	private int totalNumberOfCopiesToSummon;

	private GameObject summonEffectsInstance;

	public GameObject orbToSummon => _orbToSummon;

	public Attack copiedOrbsAttack => _copiedOrbsAttack;

	public void OnValidate()
	{
		LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
		if (component != null)
		{
			component.SetParameterValue("SUMMON_AMOUNT", numberOfCopiesToSummon.ToString());
		}
		else
		{
			Debug.Log(base.gameObject.name + " does not have loc param manager   ");
		}
	}

	public void InitSummoningCircle(DeckManager inputDeckManager, Vector3 summonPosition)
	{
		hasBeenInitialized = true;
		deckManager = inputDeckManager;
		_summonPosition = summonPosition;
		addDuplicationStationMultiball = false;
		Attack component = GetComponent<Attack>();
		totalNumberOfCopiesToSummon = numberOfCopiesToSummon;
		_orbToSummon = defaultOrbToSummon;
		foreach (GameObject item in deckManager.shuffledDeck)
		{
			SummoningCirclePachinkoBall component2 = item.GetComponent<SummoningCirclePachinkoBall>();
			if (component2 == null)
			{
				_orbToSummon = item;
				break;
			}
			Attack component3 = item.GetComponent<Attack>();
			if (component3 != null && component3.instanceID != component.instanceID)
			{
				totalNumberOfCopiesToSummon *= component2.numberOfCopiesToSummon;
				totalNumberOfCopiesToSummon = Mathf.Min(16, totalNumberOfCopiesToSummon);
			}
		}
		totalNumberOfCopiesToSummon = Mathf.Min(16, totalNumberOfCopiesToSummon);
		Attack component4 = _orbToSummon.GetComponent<Attack>();
		component4.IsSummonedBy = this;
		_copiedOrbsAttack = component4;
	}

	public override void Fire(bool notifyFire = true)
	{
		Vector3 position = base.transform.position;
		_previousAimVector = _aimVector;
		_currentControllerRotateSpeed = 0f;
		_mouseDownOnPlayfield = false;
		_fireButtonPressed = false;
		base.CurrentState = FireballState.FIRING;
		base.shotTime = 0f;
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
			PachinkoBall.OnShotFired(_aimVector);
		}
		float fireForce = _orbToSummon.GetComponent<PachinkoBall>().FireForce;
		Vector2 fireVector = base.aimVector.normalized * fireForce;
		HandleMultiballsOnFired(position);
		StartCoroutine(FireOrbs(fireVector));
		StartDestroy();
	}

	protected override void SpawnOnFiredMultiball(GameObject _gameObject, Vector3 _fireFromPosition, Vector2 multiballFireForce)
	{
		StartCoroutine(FireOrbs(multiballFireForce, doEffects: false));
	}

	private IEnumerator FireOrbs(Vector2 fireVector, bool doEffects = true)
	{
		for (int i = 0; i < totalNumberOfCopiesToSummon; i++)
		{
			PachinkoBall.OnAdditionalPachinkoBallCreated();
		}
		if (doEffects)
		{
			StartSummoningEffects();
		}
		Attack copiedAttack = _orbToSummon.GetComponent<Attack>();
		bool addedDuplicationStationMultiball = false;
		WaitForSeconds waitForSeconds = new WaitForSeconds(timeBetweenOrbSpawns);
		for (int j = 0; j < totalNumberOfCopiesToSummon; j++)
		{
			if (orbSummonSound != null)
			{
				_wallbounceAudioSource.PlayOneShot(orbSummonSound);
			}
			GameObject gameObject = SpawnMultiballFromLocation(_orbToSummon, _summonPosition, fireVector, copiedAttack, callCreatedDelegate: false);
			if (gameObject != null && !addedDuplicationStationMultiball && addDuplicationStationMultiball)
			{
				addedDuplicationStationMultiball = true;
				gameObject.GetComponent<PachinkoBall>().multiballLevel++;
			}
			yield return waitForSeconds;
		}
		yield return new WaitForSeconds(1f);
		if (doEffects)
		{
			StopSummoningEffects();
		}
	}

	public override void InitializeMembers()
	{
		base.InitializeMembers();
		if (hasBeenInitialized)
		{
			PachinkoBall component = _orbToSummon.GetComponent<PachinkoBall>();
			_trajectorySimulation.fireForce = component.FireForce;
			_trajectorySimulation.mass = _orbToSummon.GetComponent<Rigidbody2D>().mass;
			_trajectorySimulation.gravityMod = component.GravityScale;
		}
	}

	public override void SetTrajectorySimulationRadius()
	{
		_trajectorySimulation.radius = _orbToSummon.transform.localScale.x * _orbToSummon.GetComponent<CircleCollider2D>().radius;
	}

	private void StartSummoningEffects()
	{
		summonEffectsInstance = UnityEngine.Object.Instantiate(summonEffects, _summonPosition, Quaternion.identity);
		summonEffectsInstance.transform.DOScale(summonEffectsInstance.transform.localScale, 0.4f).From(Vector3.one * 0.4f).SetEase(Ease.OutBack);
	}

	private void StopSummoningEffects()
	{
		TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = summonEffectsInstance.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InQuad);
		tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(OnStopSummoningEffectsFinished));
	}

	private void OnStopSummoningEffectsFinished()
	{
		if (summonEffectsInstance != null)
		{
			UnityEngine.Object.Destroy(summonEffectsInstance);
		}
	}

	private void OnDestroy()
	{
		if (summonEffectsInstance != null)
		{
			UnityEngine.Object.Destroy(summonEffectsInstance);
		}
	}
}
