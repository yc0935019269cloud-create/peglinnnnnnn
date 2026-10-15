using System;
using System.Collections;
using Battle.PegBehaviour;
using Challenges;
using Cruciball;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Loading;
using Map;
using PeglinUI.PostBattle;
using PeglinUtils;
using Relics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Worldmap;

namespace Battle
{
	public class PostBattleController : MonoBehaviour
	{
		public delegate void MisNavigationEvent();

		public static MisNavigationEvent OnMisNavigationEvent = () =>
		{
		};

		[SerializeField]
		private BattleUpgradeCanvas _upgradeOptionsCanvas;

		[SerializeField]
		private CruciballManager _cruciballManager;

		[SerializeField]
		private DeckManager _deckManager;

		[SerializeField]
		private RelicManager _relicManager;

		[SerializeField]
		private SlotManager _leftSlotManager;

		[SerializeField]
		private SlotManager _centerSlotManager;

		[SerializeField]
		private SlotManager _rightSlotManager;

		[SerializeField]
		private GameObject _winTimeline;

		public float endBattleMoveTime = 1.2f;

		[SerializeField]
		private float _slotVisualTweenDelay = 0.2f;

		[SerializeField]
		private float _slotVisualTweenDuration = 0.8f;

		private GameObject _player;

		[SerializeField]
		private PlayerHealthController _playerHealthController;

		private BattleController _battleController;

		private static readonly int Celebration = Animator.StringToHash("celebrate");

		private static readonly int Moving = Animator.StringToHash("moving");

		public AudioClip blackHoleRandomNavSFX;

		public SeededBattleNodeData seededBattleData;

		public GameObject coreQuestionCanvas;

		public GameObject fakeMinesWinScene;

		public GameObject coreQuestionButtonPanel;

		public Image fadeImage;

		[SerializeField]
		private TextMeshProUGUI _coreWarningText;

		[HideInInspector]
		public bool showGoToCoreOption;

		private bool _goingToCore;

		private SpecialSlotController _specialSlotController;

		public BattleUpgradeCanvas upgradeOptionsCanvas => _upgradeOptionsCanvas;

		public GameObject winTimeline => _winTimeline;

		public bool goingToCore => _goingToCore;

		private void OnEnable()
		{
			BattleUpgradeCanvas.OnBattleUpgradeOver = (BattleUpgradeCanvas.BattleUpgradeOver)Delegate.Combine(BattleUpgradeCanvas.OnBattleUpgradeOver, new BattleUpgradeCanvas.BattleUpgradeOver(OnUpgradeFinished));
			SlotTrigger.onSlotTriggerActivated = (SlotTrigger.SlotTriggerActivated)Delegate.Combine(SlotTrigger.onSlotTriggerActivated, new SlotTrigger.SlotTriggerActivated(HandleSlotTriggerActivated));
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegActivated, new Peg.PegHitEvent(CheckPegboardClearedAchievement));
			_player = GameObject.FindGameObjectWithTag("Player");
			_battleController = GameObject.FindGameObjectWithTag("GameController").GetComponent<BattleController>();
			_specialSlotController = UnityEngine.Object.FindObjectOfType<SpecialSlotController>();
			if (showGoToCoreOption)
			{
				StartCoroutine(StartFakeMinesSceneFade());
				return;
			}
			_upgradeOptionsCanvas.ConfigureForPostBattleRewards(_deckManager, _playerHealthController, seededBattleData);
			_upgradeOptionsCanvas.gameObject.SetActive(value: true);
		}

		private IEnumerator StartFakeMinesSceneFade()
		{
			_cruciballManager.VictoryAchieved();
			yield return new WaitForSeconds(2.5f);
			float num = 1f;
			FadeInThenOut(num);
			yield return new WaitForSeconds(num / 2f);
			fakeMinesWinScene.SetActive(value: true);
		}

		private void OnUpgradeFinished()
		{
			StartNavigation();
		}

		private IEnumerator WaitAndEnableWinTimeline(float waitTime)
		{
			yield return new WaitForSecondsRealtime(waitTime);
			_winTimeline.SetActive(value: true);
		}

		public void SkipMimicNavigation()
		{
			StartNavigation(movePeglin: false);
			_battleController.ArmNavigationBall();
		}

		private void CheckPegboardClearedAchievement(Peg.PegType pegType, Peg peg)
		{
			_battleController.CheckPegboardClearedAchievement();
		}

		private void StartNavigation(bool movePeglin = true)
		{
			StaticGameData.dataToLoad = null;
			_battleController.CheckPegboardClearedAchievement();
			if (StaticGameData.currentNode == null || StaticGameData.currentNode.ChildNodes.Length == 0)
			{
				_player.GetComponent<Animator>().SetBool(Celebration, value: true);
				_winTimeline.SetActive(value: true);
				return;
			}
			if (movePeglin)
			{
				_player.GetComponent<Animator>().SetBool(Moving, value: true);
				TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = _player.transform.DOMoveX(0f, endBattleMoveTime);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(MoveFinished));
			}
			_battleController.RemoveClearedPegs();
			_battleController.ResetDamageTally();
			_battleController.PreparePegsForNavigation();
			if (StaticGameData.currentNode.ChildNodes.Length == 1)
			{
				MapNode mapNode = StaticGameData.currentNode.ChildNodes[0];
				_leftSlotManager.gameObject.SetActive(value: true);
				_leftSlotManager.ConfigureHalfNavigation(MapNode.GetColorForNodeType(mapNode.RoomType), _slotVisualTweenDelay, _slotVisualTweenDuration);
				_rightSlotManager.gameObject.SetActive(value: true);
				_rightSlotManager.ConfigureHalfNavigation(MapNode.GetColorForNodeType(mapNode.RoomType), _slotVisualTweenDelay, _slotVisualTweenDuration);
				_centerSlotManager.gameObject.SetActive(value: true);
				_centerSlotManager.ConfigureForNavigation(mapNode.activeIcon, MapNode.GetColorForNodeType(mapNode.RoomType), _slotVisualTweenDelay, _slotVisualTweenDuration);
				return;
			}
			MapNode mapNode2 = StaticGameData.currentNode.ChildNodes[0];
			_leftSlotManager.gameObject.SetActive(value: true);
			_leftSlotManager.ConfigureForNavigation(mapNode2.activeIcon, MapNode.GetColorForNodeType(mapNode2.RoomType), _slotVisualTweenDelay, _slotVisualTweenDuration);
			mapNode2 = StaticGameData.currentNode.RightChild;
			_rightSlotManager.gameObject.SetActive(value: true);
			_rightSlotManager.ConfigureForNavigation(mapNode2.activeIcon, MapNode.GetColorForNodeType(mapNode2.RoomType), _slotVisualTweenDelay, _slotVisualTweenDuration);
			_centerSlotManager.gameObject.SetActive(value: true);
			if (StaticGameData.currentNode.ChildNodes.Length > 2)
			{
				mapNode2 = StaticGameData.currentNode.ChildNodes[1];
				_centerSlotManager.ConfigureForNavigation(mapNode2.activeIcon, MapNode.GetColorForNodeType(mapNode2.RoomType), _slotVisualTweenDelay, _slotVisualTweenDuration);
			}
			else
			{
				_centerSlotManager.ConfigureDudNavigation();
			}
		}

		private void MoveFinished()
		{
			_player.GetComponent<Animator>().SetBool("moving", value: false);
			_player.GetComponent<Emoter>().ShowEmote(Emoter.Emote.QUESTION);
			_battleController.ArmNavigationBall();
		}

		private void HandleSlotTriggerActivated(int index, PachinkoBall pBall)
		{
			int num = StaticGameData.currentNode.ChildNodes.Length;
			if (num == 1)
			{
				if (index > 0 && index < 4)
				{
					StaticGameData.chosenNextNodeIndex = 0;
					TriggerVictory();
				}
				else
				{
					MisNavigation(pBall, index);
				}
			}
			else if (num > 1)
			{
				if (index < 2)
				{
					StaticGameData.chosenNextNodeIndex = 0;
					TriggerVictory();
				}
				else if (index > 2)
				{
					StaticGameData.chosenNextNodeIndex = StaticGameData.currentNode.ChildNodes.Length - 1;
					TriggerVictory();
				}
				else if (StaticGameData.currentNode.ChildNodes.Length > 2)
				{
					StaticGameData.chosenNextNodeIndex = 1;
					TriggerVictory();
				}
				else
				{
					MisNavigation(pBall, index);
				}
			}
		}

		public IEnumerator DoBlackHoleNavigation(int navDirection, float randomRoll = -1f)
		{
			if (blackHoleRandomNavSFX != null)
			{
				_player.GetComponent<AudioSource>().PlayOneShot(blackHoleRandomNavSFX);
			}
			_player.GetComponent<Emoter>().ShowEmote(Emoter.Emote.EXCLAMATION);
			_player.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InCubic);
			_player.GetComponent<SpriteRenderer>().DOColor(Color.black, 0.4f).SetEase(Ease.InCubic);
			yield return new WaitForSeconds(0.5f);
			switch (navDirection)
			{
			case -1:
				StaticGameData.chosenNextNodeIndex = 0;
				break;
			case 0:
				if (StaticGameData.currentNode.ChildNodes.Length == 3)
				{
					StaticGameData.chosenNextNodeIndex = 1;
				}
				else if (randomRoll == -1f)
				{
					StaticGameData.chosenNextNodeIndex = UnityEngine.Random.Range(0, StaticGameData.currentNode.ChildNodes.Length);
				}
				else
				{
					StaticGameData.chosenNextNodeIndex = ListExtensions.ConvertPercentageToListIndex(randomRoll, StaticGameData.currentNode.ChildNodes.Length);
				}
				break;
			case 1:
				StaticGameData.chosenNextNodeIndex = StaticGameData.currentNode.ChildNodes.Length - 1;
				break;
			default:
				if (randomRoll == -1f)
				{
					StaticGameData.chosenNextNodeIndex = UnityEngine.Random.Range(0, StaticGameData.currentNode.ChildNodes.Length);
				}
				else
				{
					StaticGameData.chosenNextNodeIndex = ListExtensions.ConvertPercentageToListIndex(randomRoll, StaticGameData.currentNode.ChildNodes.Length);
				}
				break;
			}
			TriggerVictory();
		}

		private void MisNavigation(PachinkoBall pBall, int index)
		{
			if (!_relicManager.AttemptUseRelic(RelicEffect.SUPER_BOOTS))
			{
				if (MapController.instance != null && MapController.instance.Act == 4 && !_specialSlotController.purpleFireSlotsThatHaveTurnedOrange.Contains(index))
				{
					_playerHealthController.Damage(Mathf.RoundToInt(_cruciballManager.GetModifiedMisnavigationDamage() / 2f), melee: true, null, PlayerHealthController.DamageSource.UNSPECIFIED, forceMaxHPDamage: true);
					_specialSlotController.purpleFireSlotsThatHaveTurnedOrange.Add(index);
					_specialSlotController.SetSlotIndexOrange(index);
				}
				else
				{
					_playerHealthController.Damage(_cruciballManager.GetModifiedMisnavigationDamage());
				}
			}
			pBall.StartDestroy();
			OnMisNavigationEvent?.Invoke();
		}

		private void TriggerVictory()
		{
			if (BattleController.CurrentBattleState == BattleController.BattleState.NAVIGATION)
			{
				_battleController.NavigationSuccessful();
				if (_battleController.numPegsHit > 0 && _relicManager.AttemptUseRelic(RelicEffect.SUPER_BOOTS))
				{
					_playerHealthController.Heal(_battleController.numPegsHit);
				}
				_player.GetComponent<Emoter>().HideEmote();
				_player.GetComponent<Animator>().SetBool(Celebration, value: false);
				if (PegCoinOverlay.ongoingCoinCollections > 0)
				{
					StartCoroutine(WaitForCoinsToFinishCollecting());
				}
				else
				{
					_winTimeline.SetActive(value: true);
				}
			}
		}

		private IEnumerator WaitForCoinsToFinishCollecting()
		{
			float totalWait = 0f;
			WaitForSeconds waitForSeconds = new WaitForSeconds(0.3f);
			while (PegCoinOverlay.ongoingCoinCollections > 0 && totalWait < 2f)
			{
				yield return waitForSeconds;
				totalWait += 0.3f;
			}
			_winTimeline.SetActive(value: true);
			PegCoinOverlay.ongoingCoinCollections = 0;
		}

		public void ContinuePastFakeMinesWinScene()
		{
			StartCoroutine(StartEnterCoreQuestionFade());
		}

		private IEnumerator StartEnterCoreQuestionFade()
		{
			float num = 5f;
			FadeInThenOut(num);
			yield return new WaitForSeconds(num / 2f);
			fakeMinesWinScene.SetActive(value: false);
			coreQuestionCanvas.SetActive(value: true);
		}

		public void GoToCoreButtonPressed()
		{
			ChallengeManager.Instance.chosenToGoToCore = true;
			MapController.instance.nextScene = PeglinSceneLoader.Scene.CORE_MAP;
			_coreWarningText.DOFade(0f, 0.5f);
			coreQuestionButtonPanel.SetActive(value: false);
			_upgradeOptionsCanvas.afterMinesBoss = true;
			_upgradeOptionsCanvas.ConfigureForPostBattleRewards(_deckManager, _playerHealthController, seededBattleData);
			_upgradeOptionsCanvas.gameObject.SetActive(value: true);
		}

		public void SkipCoreButtonPressed()
		{
			ChallengeManager.Instance.chosenToSkipCore = true;
			winTimeline.SetActive(value: true);
		}

		private void OnDisable()
		{
			BattleUpgradeCanvas.OnBattleUpgradeOver = (BattleUpgradeCanvas.BattleUpgradeOver)Delegate.Remove(BattleUpgradeCanvas.OnBattleUpgradeOver, new BattleUpgradeCanvas.BattleUpgradeOver(OnUpgradeFinished));
			SlotTrigger.onSlotTriggerActivated = (SlotTrigger.SlotTriggerActivated)Delegate.Remove(SlotTrigger.onSlotTriggerActivated, new SlotTrigger.SlotTriggerActivated(HandleSlotTriggerActivated));
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegActivated, new Peg.PegHitEvent(CheckPegboardClearedAchievement));
		}

		public void FadeInThenOut(float length)
		{
			fadeImage.enabled = true;
			TweenerCore<Color, Color, ColorOptions> tweenerCore = fadeImage.DOFade(1f, length / 2f).From(0f);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
			{
				FadeOutAndDisableObj(length / 2f);
			}));
		}

		public void FadeOutAndDisableObj(float length)
		{
			fadeImage.DOKill();
			TweenerCore<Color, Color, ColorOptions> tweenerCore = fadeImage.DOFade(0f, length);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
			{
				fadeImage.enabled = false;
			}));
		}
	}
}
