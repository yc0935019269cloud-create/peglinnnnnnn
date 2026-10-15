using ClassSystem;
using Peglin.ClassSystem;
using Saving;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI.Encirclepedia
{
	public class ClassTab : MonoBehaviour
	{
		private enum TabState
		{
			FOLDED = 0,
			IDLE = 1,
			UNFOLDED = 2
		}

		public Image characterSprite;

		public Animator tabAnimator;

		public Image tabBackground;

		public TextMeshProUGUI cruciballDisplay;

		public Class character;

		private TabState _tabState = TabState.IDLE;

		private bool _animTriggeredThisFrame;

		private bool _isUnlocked;

		private static readonly int UNFOLD_TRIGGER = Animator.StringToHash("unfold");

		private static readonly int FOLD_TRIGGER = Animator.StringToHash("fold");

		private static readonly int IDLE_TRIGGER = Animator.StringToHash("idle");

		private OverallProgressManager _manager;

		public void Initialize(ClassInfo info, OverallProgressManager manager)
		{
			character = info.characterClass;
			_isUnlocked = info.ClassUnlocked();
			characterSprite.sprite = (_isUnlocked ? info.classSprite : info.lockedSprite);
			if (!_isUnlocked)
			{
				characterSprite.color = Color.black;
			}
			_manager = manager;
			tabBackground.sprite = info.tabSprite;
			int num = PersistentPlayerData.Instance.CruciballLevels[info.characterClass];
			if (num > 0)
			{
				cruciballDisplay.text = num.ToString();
			}
			else
			{
				cruciballDisplay.transform.parent.gameObject.SetActive(value: false);
			}
		}

		public bool IsUnlocked()
		{
			return _isUnlocked;
		}

		private void Update()
		{
			if (_animTriggeredThisFrame)
			{
				_animTriggeredThisFrame = false;
			}
		}

		public void Clicked()
		{
			if (_isUnlocked && (AnimatorUtils.AnimationIsPlaying(tabAnimator, "tab_neutral") || (!_animTriggeredThisFrame && !AnimatorUtils.AnyAnimationIsPlaying(tabAnimator))))
			{
				_manager.OnClassTabClicked(this);
			}
		}

		public void Unfold()
		{
			if (_tabState != TabState.UNFOLDED)
			{
				tabAnimator.SetTrigger(UNFOLD_TRIGGER);
				_tabState = TabState.UNFOLDED;
				_animTriggeredThisFrame = true;
			}
		}

		public void Fold()
		{
			if (_tabState != TabState.FOLDED)
			{
				tabAnimator.SetTrigger(FOLD_TRIGGER);
				_tabState = TabState.FOLDED;
				_animTriggeredThisFrame = true;
			}
		}

		public void AnimateToIdle()
		{
			if (_tabState != TabState.IDLE)
			{
				tabAnimator.SetTrigger(IDLE_TRIGGER);
				_tabState = TabState.IDLE;
				_animTriggeredThisFrame = true;
			}
		}
	}
}
