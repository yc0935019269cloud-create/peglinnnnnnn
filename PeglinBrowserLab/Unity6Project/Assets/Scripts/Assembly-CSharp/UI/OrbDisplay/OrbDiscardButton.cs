using System;
using Battle;
using PeglinUI;
using PeglinUI.ControllerSupport;
using Rewired;
using TMPro;
using UnityEngine;

namespace UI.OrbDisplay
{
	public class OrbDiscardButton : MonoBehaviour
	{
		public delegate void OrbDiscardEvent();

		public static OrbDiscardEvent OnOrbDiscardButtonClicked = () =>
		{
		};

		public static OrbDiscardEvent OnDiscardFinished = () =>
		{
		};

		private static readonly int HoveringParam = Animator.StringToHash("Hovering");

		private static readonly int OrbDiscardedParam = Animator.StringToHash("OrbDiscarded");

		[SerializeField]
		private BattleController _battleController;

		[SerializeField]
		private GameObject gamepadPrompt;

		private Animator _anim;

		private TMP_Text _text;

		private Player _player;

		private bool _orbActive;

		private bool _hovering;

		private bool _cachedCanDiscard;

		private void Start()
		{
			_anim = GetComponent<Animator>();
			_text = GetComponentInChildren<TMP_Text>();
			_player = ReInput.players.GetPlayer(0);
			UpdateDiscardCount();
		}

		private void OnEnable()
		{
			BattleController.OnAttackStarted = (Action)Delegate.Combine(BattleController.OnAttackStarted, new Action(UpdateDiscardCount));
			BattleController.OnReloadStarted = (BattleController.ReloadStarted)Delegate.Combine(BattleController.OnReloadStarted, new BattleController.ReloadStarted(UpdateDiscardCount));
			BattleController.OnOrbDiscarded = (Action)Delegate.Combine(BattleController.OnOrbDiscarded, new Action(OrbDiscarded));
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Combine(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(InputTypeChanged));
			DeckInfoManager.onActiveOrbScaleCompleted = (DeckInfoManager.DeckInfoAnim)Delegate.Combine(DeckInfoManager.onActiveOrbScaleCompleted, new DeckInfoManager.DeckInfoAnim(ActiveOrbReady));
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Combine(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(ShotFired));
		}

		private void OnDisable()
		{
			BattleController.OnAttackStarted = (Action)Delegate.Remove(BattleController.OnAttackStarted, new Action(UpdateDiscardCount));
			BattleController.OnReloadStarted = (BattleController.ReloadStarted)Delegate.Remove(BattleController.OnReloadStarted, new BattleController.ReloadStarted(UpdateDiscardCount));
			BattleController.OnOrbDiscarded = (Action)Delegate.Remove(BattleController.OnOrbDiscarded, new Action(OrbDiscarded));
			ControllerInputManager.OnInputTypeChanged = (ControllerInputManager.InputTypeChanged)Delegate.Remove(ControllerInputManager.OnInputTypeChanged, new ControllerInputManager.InputTypeChanged(InputTypeChanged));
			DeckInfoManager.onActiveOrbScaleCompleted = (DeckInfoManager.DeckInfoAnim)Delegate.Remove(DeckInfoManager.onActiveOrbScaleCompleted, new DeckInfoManager.DeckInfoAnim(ActiveOrbReady));
			PachinkoBall.OnShotFired = (PachinkoBall.PachinkoBallFired)Delegate.Remove(PachinkoBall.OnShotFired, new PachinkoBall.PachinkoBallFired(ShotFired));
		}

		private void OrbDiscarded()
		{
			_anim.SetTrigger(OrbDiscardedParam);
			UpdateDiscardCount();
			if (!CanDiscard())
			{
				_anim.SetBool(HoveringParam, value: false);
			}
		}

		private void ActiveOrbReady()
		{
			_orbActive = true;
			if (CanDiscard() && _hovering)
			{
				_anim.SetBool(HoveringParam, value: true);
			}
			OnDiscardFinished?.Invoke();
		}

		private void ShotFired(Vector2 aimVector)
		{
			_orbActive = false;
		}

		private void Update()
		{
			if (ControllerInputManager.primaryInputType != InputType.Touch && _player.GetButtonDown("Back") && !PauseMenu.Paused && !GameBlockingWindow.windowOpen && !ControllerInspection.CurrentlyInspecting)
			{
				OnOrbDiscardButtonClicked();
			}
			if (_cachedCanDiscard != CanDiscard())
			{
				_cachedCanDiscard = CanDiscard();
				gamepadPrompt.SetActive(_cachedCanDiscard);
			}
		}

		public void OnMouseEnter()
		{
			_hovering = true;
			if (CanDiscard())
			{
				_anim.SetBool(HoveringParam, value: true);
			}
		}

		public void OnMouseExit()
		{
			_hovering = false;
			_anim.SetBool(HoveringParam, value: false);
		}

		private void OnMouseDown()
		{
			OnOrbDiscardButtonClicked();
		}

		private void UpdateDiscardCount()
		{
			_text.text = _battleController.NumShotsDiscarded + "/" + _battleController.MaxDiscardedShots;
		}

		private bool CanDiscard()
		{
			if (_orbActive)
			{
				return _battleController.NumShotsDiscarded < _battleController.MaxDiscardedShots;
			}
			return false;
		}

		private void InputTypeChanged()
		{
			if (!ControllerInputManager.IsPointerBased && gamepadPrompt != null)
			{
				gamepadPrompt.SetActive(CanDiscard());
			}
		}
	}
}
