using System;
using Cruciball;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Peglin;
using Peglin.ClassSystem;
using UnityEngine;

namespace PeglinUI.MainMenu
{
	public class SelectableCharacter : MonoBehaviour
	{
		public delegate void OrbDropAction(Vector2 pos, Class cl);

		public delegate void ClassMouseSelectionAction(SelectableCharacter character);

		public static OrbDropAction OnDropOrbAnimationTrigger = delegate
		{
		};

		public ClassMouseSelectionAction OnClassMouseSelected = delegate
		{
		};

		public Class characterClass;

		public float walkSpeed = 5f;

		public float loadoutShowcasePositionX = 9.5f;

		public GameObject highlight;

		public string classNameLocKey;

		public string classDescriptionLocKey;

		public string classRelicNameLocKey;

		public string classRelicDescriptionLocKey;

		public string classUnlockMethodLocKey;

		public string classUnlockXpMethodLocKey;

		public string classFeatTitleLocKey;

		public string classFeatDescriptionLocKey;

		public Sprite classRelicIcon;

		public Sprite nameScrollImage;

		public Sprite idleSprite;

		public Sprite lockedSprite;

		public SpriteRenderer rewardIcon;

		private Animator _anim;

		private AnimationEventBroadcaster _animBroadcaster;

		private SpriteRenderer _spriteRenderer;

		private float _spriteWidthHalf;

		private static readonly int MovingAnimParam = Animator.StringToHash("moving");

		private bool _droppingOrb;

		private Vector2 _orbDropPosition;

		private Vector2 _queuedTarget = Vector2.zero;

		private bool _queuedMove;

		private static readonly int DropOrbAnimParam = Animator.StringToHash("drop_orb");

		private Collider2D _col;

		private int _originalSortingOrder;

		public bool droppingOrb => _droppingOrb;

		public bool IsUnlocked => CruciballManager.IsClassUnlocked(characterClass);

		public Sprite GetSprite()
		{
			return idleSprite;
		}

		private void Awake()
		{
			_anim = GetComponentInChildren<Animator>();
			_animBroadcaster = GetComponentInChildren<AnimationEventBroadcaster>();
			_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
			_spriteWidthHalf = _spriteRenderer.bounds.size.x * 0.5f;
			_col = GetComponent<Collider2D>();
		}

		private void OnEnable()
		{
			if (!IsUnlocked)
			{
				_spriteRenderer.sprite = lockedSprite;
				_anim.enabled = false;
			}
			AnimationEventBroadcaster animBroadcaster = _animBroadcaster;
			animBroadcaster.OnAnimationAction = (Action)Delegate.Combine(animBroadcaster.OnAnimationAction, new Action(DropOrbTrigger));
			AnimationEventBroadcaster animBroadcaster2 = _animBroadcaster;
			animBroadcaster2.OnAnimationFinished = (Action)Delegate.Combine(animBroadcaster2.OnAnimationFinished, new Action(AnimationFinished));
			_col.enabled = true;
		}

		private void OnDisable()
		{
			AnimationEventBroadcaster animBroadcaster = _animBroadcaster;
			animBroadcaster.OnAnimationAction = (Action)Delegate.Remove(animBroadcaster.OnAnimationAction, new Action(DropOrbTrigger));
			AnimationEventBroadcaster animBroadcaster2 = _animBroadcaster;
			animBroadcaster2.OnAnimationFinished = (Action)Delegate.Remove(animBroadcaster2.OnAnimationFinished, new Action(AnimationFinished));
		}

		public void MoveToPoint(Vector2 pos)
		{
			if (!IsUnlocked)
			{
				return;
			}
			if (_droppingOrb)
			{
				_queuedTarget = pos;
				_queuedMove = true;
				return;
			}
			float x = pos.x;
			if (pos.x < base.transform.position.x)
			{
				_spriteRenderer.flipX = true;
				x += _spriteWidthHalf;
			}
			else
			{
				_spriteRenderer.flipX = false;
				x -= _spriteWidthHalf;
			}
			_anim.SetBool(MovingAnimParam, value: true);
			TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = base.transform.DOMoveX(x, Mathf.Abs(base.transform.position.x - pos.x) / walkSpeed);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(MoveFinished));
		}

		private void MoveFinished()
		{
			_anim.SetBool(MovingAnimParam, value: false);
		}

		public void MoveAndDropOrb(Vector2 pos)
		{
			if (!IsUnlocked)
			{
				return;
			}
			_droppingOrb = true;
			float num = Mathf.Abs(base.transform.position.x - pos.x);
			if (num >= _spriteWidthHalf * 0.5f)
			{
				float x = pos.x;
				if (pos.x < base.transform.position.x)
				{
					_spriteRenderer.flipX = true;
					x += _spriteWidthHalf;
				}
				else
				{
					_spriteRenderer.flipX = false;
					x -= _spriteWidthHalf;
				}
				_anim.SetBool(MovingAnimParam, value: true);
				TweenerCore<Vector3, Vector3, VectorOptions> tweenerCore = base.transform.DOMoveX(x, num / walkSpeed);
				tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, (TweenCallback)(() =>
				{
					PlayDropOrbAnim(pos);
				}));
			}
			else
			{
				PlayDropOrbAnim(pos);
			}
		}

		public void PlayDropOrbAnim(Vector2 pos)
		{
			_droppingOrb = true;
			_orbDropPosition = pos;
			_anim.SetBool(MovingAnimParam, value: false);
			_anim.SetTrigger(DropOrbAnimParam);
		}

		public void SelectCharacter()
		{
			_originalSortingOrder = _spriteRenderer.sortingOrder;
			_spriteRenderer.sortingOrder = 9;
			highlight.gameObject.SetActive(value: true);
			_col.enabled = false;
		}

		public void DeselectCharacter()
		{
			_spriteRenderer.sortingOrder = _originalSortingOrder;
			highlight.gameObject.SetActive(value: false);
			_col.enabled = true;
		}

		private void DropOrbTrigger()
		{
			OnDropOrbAnimationTrigger?.Invoke(_orbDropPosition, characterClass);
		}

		private void AnimationFinished()
		{
			_droppingOrb = false;
			if (_queuedMove)
			{
				MoveToPoint(_queuedTarget);
			}
		}

		private void OnMouseDown()
		{
			OnClassMouseSelected?.Invoke(this);
		}
	}
}
