using System;
using System.Collections;
using System.Collections.Generic;
using Battle;
using Battle.Enemies;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Map;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class MimicCutsceneController : MonoBehaviour
{
	public BattleController battleController;

	public GameObject peglin;

	public GameObject playerStatusEffectIcons;

	public Canvas playerStatusEffectsCanvas;

	public GameObject mimicTimeline;

	public Image mimicPegBoardCover;

	public Image mimicPegBoardCoverLeft;

	public Image mimicPegBoardCoverRight;

	public GameObject mimicScenarioHealthbar;

	public GameObject minicSkipButton;

	public Canvas fireButtonCanvas;

	public List<Image> mimicCoverImages;

	public Color coreMimicCurtainColor = new Color(0.05882f, 0.05882f, 0.12156f);

	private Player _player;

	private bool _choiceMade;

	private bool _buttonPressed;

	private Act4BossPegBoardFrameManager _act4BossPegBoardFrameManager;

	private MimicEnemy _mimic;

	private void Awake()
	{
		_player = ReInput.players.GetPlayer(0);
	}

	private void Start()
	{
		_mimic = UnityEngine.Object.FindObjectOfType<MimicEnemy>();
	}

	private void OnEnable()
	{
		MimicEnemy.OnMimicOpened = (MimicEnemy.MimicOpened)Delegate.Combine(MimicEnemy.OnMimicOpened, new MimicEnemy.MimicOpened(OnMimicOpened));
	}

	private void OnDisable()
	{
		MimicEnemy.OnMimicOpened = (MimicEnemy.MimicOpened)Delegate.Remove(MimicEnemy.OnMimicOpened, new MimicEnemy.MimicOpened(OnMimicOpened));
	}

	public void MakeBattleLookLikeChest()
	{
		Camera.main.transform.position = new Vector3(0f, 6f, -10f);
		Camera.main.orthographicSize = 9f;
		Camera.main.backgroundColor = new Color(0.03773582f, 0.03773582f, 0.03773582f, 1f);
		peglin.transform.position = new Vector3(-10.4375f, 5.99f, 0.4401802f);
		playerStatusEffectsCanvas.enabled = false;
		if (MapController.instance.Act != 4)
		{
			return;
		}
		foreach (Image mimicCoverImage in mimicCoverImages)
		{
			mimicCoverImage.color = coreMimicCurtainColor;
		}
		_act4BossPegBoardFrameManager = UnityEngine.Object.FindObjectOfType<Act4BossPegBoardFrameManager>();
		if (_act4BossPegBoardFrameManager != null)
		{
			_act4BossPegBoardFrameManager.LookLikeChestForMimic();
		}
	}

	private void OnMimicOpened()
	{
		if (!_choiceMade)
		{
			_choiceMade = true;
			StartCoroutine(OnMimicOpenedAnimation());
		}
	}

	public void OnMimicSkipped()
	{
		if (!_choiceMade)
		{
			_choiceMade = true;
			_mimic.skipped = true;
			_mimic.chestUI.SetActive(value: false);
			UnityEngine.Object.FindObjectOfType<DeckInfoManager>().gameObject.SetActive(value: false);
			float duration = 2f;
			DOTween.To(() => Camera.main.orthographicSize, (float x) =>
			{
				Camera.main.orthographicSize = x;
			}, 10f, duration);
			Camera.main.transform.DOMove(new Vector3(-2f, 1.25f, -10f), duration);
			TweenerCore<Color, Color, ColorOptions> tweenerCore = mimicPegBoardCover.GetComponent<Image>().DOFade(0f, duration).From(1f);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(DisableCurtain));
			Camera.main.transform.GetComponent<ScreenShaker>().initialCameraPos = new Vector3(-2f, 1.25f, -10f);
			playerStatusEffectsCanvas.enabled = true;
			playerStatusEffectIcons.transform.position += new Vector3(0f, 0.6f, 0f);
			fireButtonCanvas.sortingLayerID = SortingLayer.NameToID("UI");
			fireButtonCanvas.sortingOrder = 101;
			battleController.TriggerVictoryWithNoRewards();
		}
	}

	private void DisableCurtain()
	{
		mimicPegBoardCover.enabled = false;
	}

	private void Update()
	{
		if (!_buttonPressed)
		{
			if (_player.GetButtonDown("UISubmit"))
			{
				_buttonPressed = true;
				_mimic.OpenMimic();
			}
			else if (_player.GetButtonDown("UICancel"))
			{
				_buttonPressed = true;
				OnMimicSkipped();
			}
		}
	}

	private IEnumerator OnMimicOpenedAnimation()
	{
		float num = 3.5f;
		if (_act4BossPegBoardFrameManager != null)
		{
			_act4BossPegBoardFrameManager.OnMimicOpened();
		}
		BGMController.instance.SetBattleAudio(BGMController.instance.regularBattleTracks[MapController.instance.Act - 1]);
		mimicScenarioHealthbar.SetActive(value: false);
		Camera.main.transform.DOMove(new Vector3(-2f, 1.25f, -10f), num).SetEase(Ease.OutSine);
		DOTween.To(() => Camera.main.orthographicSize, (float x) =>
		{
			Camera.main.orthographicSize = x;
		}, 10f, num);
		DOTween.To(() => Camera.main.backgroundColor, (Color x) =>
		{
			Camera.main.backgroundColor = x;
		}, new Color(0.1037736f, 0.1037736f, 0.1037736f, 0f), num);
		mimicPegBoardCover.transform.DOMoveY(-20.43f, num).SetEase(Ease.OutSine);
		mimicPegBoardCoverLeft.transform.DOMoveY(-20.43f, num).SetEase(Ease.OutSine);
		mimicPegBoardCoverRight.transform.DOMoveY(-20.43f, num).SetEase(Ease.OutSine);
		Camera.main.transform.GetComponent<ScreenShaker>().initialCameraPos = new Vector3(-2f, 1.25f, -10f);
		peglin.GetComponent<Emoter>().ShowEmote(Emoter.Emote.EXCLAMATION, num + 1f);
		yield return new WaitForSeconds(num);
		playerStatusEffectsCanvas.enabled = true;
		mimicTimeline.SetActive(value: false);
		BattleController.PreBattlePause = false;
	}
}
