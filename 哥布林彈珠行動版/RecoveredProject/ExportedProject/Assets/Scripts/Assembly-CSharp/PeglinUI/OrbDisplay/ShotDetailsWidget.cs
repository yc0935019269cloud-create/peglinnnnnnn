using System;
using Battle;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.OrbDisplay
{
	public class ShotDetailsWidget : MonoBehaviour
	{
		public Animator _scrollAnim;

		public Mask _mask;

		public OrbInfoWidget _orbInfoWidget;

		public float _maskMinHeight = 1.3f;

		public float _maskFullHeight = 6.195f;

		public float _maskHideTime = 0.25f;

		public float _maskDisplayTime = 0.65f;

		private static readonly int RollUp = Animator.StringToHash("RollUp");

		private static readonly int RollDown = Animator.StringToHash("RollDown");

		private GameObject prevOrbData;

		private void OnEnable()
		{
			BattleController.onOrbAttackEnded = (BattleController.PersistenceUpdate)Delegate.Combine(BattleController.onOrbAttackEnded, new BattleController.PersistenceUpdate(AttackEnded));
			BattleController.OnOrbDiscarded = (Action)Delegate.Combine(BattleController.OnOrbDiscarded, new Action(HideDetails));
			DeckInfoManager.onActiveOrbScaleStarted = (DeckInfoManager.OrbInfoEvent)Delegate.Combine(DeckInfoManager.onActiveOrbScaleStarted, new DeckInfoManager.OrbInfoEvent(UpdateDetails));
			DeckInfoManager.onActivePersistOrbDrawFinished = (DeckInfoManager.OrbInfoEvent)Delegate.Combine(DeckInfoManager.onActivePersistOrbDrawFinished, new DeckInfoManager.OrbInfoEvent(UpdateDetailsWithoutAnim));
		}

		private void OnDisable()
		{
			BattleController.onOrbAttackEnded = (BattleController.PersistenceUpdate)Delegate.Remove(BattleController.onOrbAttackEnded, new BattleController.PersistenceUpdate(AttackEnded));
			BattleController.OnOrbDiscarded = (Action)Delegate.Remove(BattleController.OnOrbDiscarded, new Action(HideDetails));
			DeckInfoManager.onActiveOrbScaleStarted = (DeckInfoManager.OrbInfoEvent)Delegate.Remove(DeckInfoManager.onActiveOrbScaleStarted, new DeckInfoManager.OrbInfoEvent(UpdateDetails));
			DeckInfoManager.onActivePersistOrbDrawFinished = (DeckInfoManager.OrbInfoEvent)Delegate.Remove(DeckInfoManager.onActivePersistOrbDrawFinished, new DeckInfoManager.OrbInfoEvent(UpdateDetailsWithoutAnim));
		}

		private void Start()
		{
			Vector2 sizeDelta = _mask.rectTransform.sizeDelta;
			if (Math.Abs(sizeDelta.y - _maskMinHeight) > 0.02f)
			{
				sizeDelta.y = _maskMinHeight;
				_mask.rectTransform.sizeDelta = sizeDelta;
			}
		}

		private void AttackEnded(int persistLevel)
		{
			if (persistLevel < -1)
			{
				HideDetails();
			}
		}

		private void HideDetails()
		{
			_scrollAnim.SetTrigger(RollUp);
			Vector2 sizeDelta = _mask.rectTransform.sizeDelta;
			sizeDelta.y = _maskMinHeight;
			_mask.rectTransform.DOSizeDelta(sizeDelta, _maskHideTime);
		}

		private void UpdateDetails(GameObject upcomingOrbDisplay)
		{
			UpcomingOrbDisplay componentInChildren = upcomingOrbDisplay.GetComponentInChildren<UpcomingOrbDisplay>();
			prevOrbData = upcomingOrbDisplay;
			if (componentInChildren != null)
			{
				componentInChildren.UpdateDetails();
				_scrollAnim.SetTrigger(RollDown);
				_orbInfoWidget.Initialize(componentInChildren.attack);
				Vector2 sizeDelta = _mask.rectTransform.sizeDelta;
				sizeDelta.y = _maskFullHeight;
				_mask.rectTransform.DOSizeDelta(sizeDelta, _maskDisplayTime);
			}
		}

		private void UpdateDetailsWithoutAnim(GameObject upcomingOrbDisplay)
		{
			UpcomingOrbDisplay componentInChildren = upcomingOrbDisplay.GetComponentInChildren<UpcomingOrbDisplay>();
			prevOrbData = upcomingOrbDisplay;
			if (componentInChildren != null)
			{
				componentInChildren.UpdateDetails();
				_orbInfoWidget.Initialize(componentInChildren.attack);
			}
		}
	}
}
