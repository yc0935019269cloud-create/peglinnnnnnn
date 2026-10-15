using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using I2.Loc;
using Loading;
using Map;
using PeglinUI;
using PeglinUI.PostBattle;
using Relics;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Scenarios
{
	public class ChestScenarioController : MonoBehaviour
	{
		private enum State
		{
			UNOPENED = 0,
			RELIC_SELECTION = 1,
			AWAITING_NAV_SHOT = 2,
			AWAITING_NAV_COMPLETE = 3
		}

		private const int ACCEPT_BUTTON_ID = 9;

		private const int BACK_BUTTON_ID = 10;

		private const int BOMB_CHESTS_ALLOWED_PER_AREA = 1;

		public static int bombChestsOpened = 0;

		public static bool isBonusChest = false;

		public static bool rareChestRequested = false;

		private State _state;

		public float introFadeTime = 0.6f;

		public float curtainFadeTime = 1f;

		public float cameraNavY = 2f;

		public float cameraToNavMoveTime = 2f;

		[Header("Managers")]
		public RelicManager relicManager;

		[Header("GameObject Refs")]
		public GameObject chestHighlight;

		public GameObject BombContainer;

		public GameObject clickTextContainer;

		public BattleUpgradeCanvas relicGrantPopup;

		public NavOnlyController navController;

		public GameObject playfieldMouseDetection;

		public GameObject pegLayout;

		public Image navCurtain;

		public Image skipButton;

		[Header("Audio")]
		public float chestOpenDelay = 0.1f;

		public AudioClip chestOpenSound;

		public AudioClip bonusChestSound;

		public AudioClip orbertsStoryMadeItRareSound;

		private AudioSource _audioSource;

		private Image _fullCurtain;

		private Player _player;

		private int _bombsRemaining = int.MaxValue;

		private bool _isBonusChest;

		private static readonly int ChestOpenString = Animator.StringToHash("Open");

		private SeededTreasureNodeData _seededTreasureNodeData;

		private void Awake()
		{
			_audioSource = GetComponent<AudioSource>();
			DOTween.Clear();
			_isBonusChest = isBonusChest;
			isBonusChest = false;
			if (_isBonusChest || bombChestsOpened >= 1)
			{
				UnityEngine.Object.Destroy(BombContainer);
			}
			else
			{
				Bomb[] componentsInChildren = BombContainer.GetComponentsInChildren<Bomb>();
				_bombsRemaining = componentsInChildren.Length;
				navController.SetUpTreasureBombs(componentsInChildren);
			}
			_player = ReInput.players.GetPlayer(0);
			_fullCurtain = GameObject.FindGameObjectWithTag("Curtain").GetComponent<Image>();
			_fullCurtain.enabled = true;
			PauseMenu.PauseBlock = true;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = _fullCurtain.DOFade(0f, introFadeTime).From(1f);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(HideMainCurtain));
			_seededTreasureNodeData = StaticGameData.seededNodeData as SeededTreasureNodeData;
			if (StaticGameData.dataToLoad != null)
			{
				MapDataTreasure mapDataTreasure = StaticGameData.dataToLoad as MapDataTreasure;
				GameObject[] scenarioDoodads = mapDataTreasure.scenarioDoodads;
				for (int i = 0; i < scenarioDoodads.Length; i++)
				{
					UnityEngine.Object.Instantiate(scenarioDoodads[i]);
				}
				if (mapDataTreasure.background != null)
				{
					UnityEngine.Object.Instantiate(mapDataTreasure.background);
				}
				if (mapDataTreasure.pegboardFrame != null)
				{
					UnityEngine.Object.Instantiate(mapDataTreasure.pegboardFrame);
				}
				navController.Initialize();
			}
		}

		private void OnEnable()
		{
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegDestruction));
			BattleUpgradeCanvas.OnBattleUpgradeOver = (BattleUpgradeCanvas.BattleUpgradeOver)Delegate.Combine(BattleUpgradeCanvas.OnBattleUpgradeOver, new BattleUpgradeCanvas.BattleUpgradeOver(Skip));
		}

		private void OnDisable()
		{
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegDestruction));
			BattleUpgradeCanvas.OnBattleUpgradeOver = (BattleUpgradeCanvas.BattleUpgradeOver)Delegate.Remove(BattleUpgradeCanvas.OnBattleUpgradeOver, new BattleUpgradeCanvas.BattleUpgradeOver(Skip));
		}

		private void OnMouseEnter()
		{
			if (_state == State.UNOPENED)
			{
				chestHighlight.SetActive(value: true);
			}
		}

		private void OnMouseExit()
		{
			chestHighlight.SetActive(value: false);
		}

		public void OnMouseDown()
		{
			if (_state == State.UNOPENED)
			{
				OpenChest();
			}
		}

		private void HandlePegDestruction(Peg.PegType type, Peg peg)
		{
			if (type != Peg.PegType.BOMB)
			{
				return;
			}
			_bombsRemaining--;
			Debug.Log($"Bombs remaining: {_bombsRemaining}, bomb chests opened: {bombChestsOpened}, isBonusChest: {_isBonusChest}");
			if (_bombsRemaining == 0 && bombChestsOpened < 1 && !_isBonusChest)
			{
				Debug.Log("opening bonus chest");
				_audioSource.PlayOneShot(bonusChestSound);
				bombChestsOpened++;
				isBonusChest = true;
				PachinkoBall[] array = UnityEngine.Object.FindObjectsOfType<PachinkoBall>();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].gameObject.SetActive(value: false);
				}
				navController.FadeAndLoad(PeglinSceneLoader.Scene.TREASURE, bonusChestSound.length + 0.1f);
			}
		}

		public void DoBonusChestStuff()
		{
			_audioSource.PlayOneShot(bonusChestSound);
			bombChestsOpened++;
			isBonusChest = true;
		}

		private void Update()
		{
			if (!GameBlockingWindow.windowOpen && !ControllerInspection.CurrentlyInspecting)
			{
				if (_player.GetButtonDown(9) && _state == State.UNOPENED)
				{
					OpenChest();
				}
				if (_player.GetButtonDown(10) && (_state == State.UNOPENED || _state == State.RELIC_SELECTION))
				{
					Skip();
				}
			}
		}

		private void HideMainCurtain()
		{
			PauseMenu.PauseBlock = false;
			_fullCurtain.enabled = false;
		}

		private void OpenChest()
		{
			MapDataTreasure mapDataTreasure = StaticGameData.dataToLoad as MapDataTreasure;
			float num = RelicManager.CHEST_RARE_CHANCE;
			if (mapDataTreasure != null)
			{
				num = mapDataTreasure.rareChance;
			}
			if (relicManager.RelicEffectActive(RelicEffect.MORE_TREASURE_NODES))
			{
				num += 0.1f;
			}
			skipButton.GetComponentInChildren<Localize>().Term = "Menu/Skip Relic";
			float num2 = ((_seededTreasureNodeData == null) ? UnityEngine.Random.value : _seededTreasureNodeData.rareRelicChanceRoll);
			bool flag = rareChestRequested || _isBonusChest || num2 <= num;
			rareChestRequested = false;
			bool flag2 = flag && relicManager.RelicEffectActive(RelicEffect.MORE_TREASURE_NODES) && !rareChestRequested && !_isBonusChest && !(num2 <= num - 0.1f);
			if (flag2)
			{
				relicManager.FlashRelic(RelicEffect.MORE_TREASURE_NODES);
			}
			relicGrantPopup.gameObject.SetActive(value: true);
			relicGrantPopup.ConfigureForRelicGrantOnly((!flag) ? RelicRarity.COMMON : RelicRarity.RARE);
			clickTextContainer.SetActive(value: false);
			GetComponent<Animator>().SetTrigger(ChestOpenString);
			_state = State.RELIC_SELECTION;
			StartCoroutine(PlayChestOpenSound(flag2));
		}

		private IEnumerator PlayChestOpenSound(bool orbertsStoryMadeItRare)
		{
			yield return new WaitForSeconds(chestOpenDelay);
			_audioSource.PlayOneShot(chestOpenSound);
			if (orbertsStoryMadeItRare)
			{
				_audioSource.PlayOneShot(orbertsStoryMadeItRareSound);
			}
		}

		public void Skip()
		{
			clickTextContainer.SetActive(value: false);
			skipButton.GetComponent<Button>().enabled = false;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = skipButton.DOFade(0f, 0.2f);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(DisableSkipButton));
			if (_state == State.UNOPENED || _state == State.RELIC_SELECTION)
			{
				_state = State.AWAITING_NAV_SHOT;
			}
			PauseMenu.PauseBlock = true;
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore2 = Camera.main.transform.DOMoveY(cameraNavY, cameraToNavMoveTime);
			tweenerCore2.onComplete = (TweenCallback)Delegate.Combine(tweenerCore2.onComplete, new TweenCallback(CameraMoveComplete));
			navCurtain.enabled = true;
			TweenerCore<Color, Color, ColorOptions> tweenerCore3 = navCurtain.DOFade(0f, curtainFadeTime).From(1f);
			tweenerCore3.onComplete = (TweenCallback)Delegate.Combine(tweenerCore3.onComplete, new TweenCallback(DisableCurtain));
			StartCoroutine(PrepareForNavigation());
			MapDataTreasure mapDataTreasure = StaticGameData.dataToLoad as MapDataTreasure;
			PredictionManager predictionManager = UnityEngine.Object.FindObjectOfType<PredictionManager>();
			predictionManager.Initialize(pegLayout, mapDataTreasure.pegboardFrame, relicManager, navigation: true);
			predictionManager.CreateFakePegboard();
			predictionManager.CopyAllPegs();
		}

		private IEnumerator PrepareForNavigation()
		{
			yield return new WaitForSeconds(0.1f);
			navController.PrepareForNavigation();
		}

		private void CameraMoveComplete()
		{
			PauseMenu.PauseBlock = false;
		}

		private void DisableCurtain()
		{
			playfieldMouseDetection.SetActive(value: true);
			navCurtain.enabled = false;
		}

		private void DisableSkipButton()
		{
			skipButton.gameObject.SetActive(value: false);
		}
	}
}
