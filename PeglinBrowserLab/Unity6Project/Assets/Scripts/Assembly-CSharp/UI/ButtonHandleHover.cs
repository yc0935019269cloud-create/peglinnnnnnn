using DG.Tweening;
using PixelCrushers.DialogueSystem.Wrappers;
using Rewired.Integration.UnityUI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
	public class ButtonHandleHover : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public delegate void UIButtonHover(int noteOffset);

		public static UIButtonHover OnUIButtonHover = delegate
		{
		};

		public int noteOffset;

		[Tooltip("Change the text colour of the button")]
		public bool changeStyle = true;

		public bool controlAnimation = true;

		public bool highlight = true;

		public Vector2 hoverScale = Vector2.one;

		public bool onlyControllerSfx;

		private Vector2 initialScale = Vector2.one;

		public Color swapColor = Color.white;

		private Color _origTextColor = Color.black;

		private TMP_Text _text;

		private Animator _animator;

		private float _cachedAnimSpeed;

		public GameObject[] toggleObjects;

		private bool _hovering;

		private RewiredEventSystem _eventSystem;

		private StandardUIResponseButton _dialogueResponse;

		public void Awake()
		{
			_dialogueResponse = GetComponent<StandardUIResponseButton>();
			_eventSystem = GameObject.FindWithTag("InputSystem").GetComponent<RewiredEventSystem>();
			_animator = GetComponentInChildren<Animator>();
			if (_animator != null && _animator.enabled && controlAnimation)
			{
				_cachedAnimSpeed = _animator.speed;
				_animator.speed = 0f;
			}
			if (_text == null)
			{
				_text = GetComponentInChildren<TMP_Text>();
				if (_text != null)
				{
					_origTextColor = _text.color;
				}
			}
		}

		public void OnDisable()
		{
			if (highlight)
			{
				UnHighlight(tween: false);
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			if (!_hovering)
			{
				if (highlight)
				{
					Highlight();
				}
				OnUIButtonHover(noteOffset);
			}
		}

		public void OnDeselect(BaseEventData eventData)
		{
			if (highlight)
			{
				UnHighlight();
			}
		}

		public void OnPointerEnter(PointerEventData ped)
		{
			Highlight();
			if (!onlyControllerSfx)
			{
				OnUIButtonHover(noteOffset);
			}
			_hovering = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			UnHighlight();
			_hovering = false;
		}

		public void Highlight()
		{
			if (changeStyle)
			{
				if (_text == null)
				{
					_text = GetComponentInChildren<TMP_Text>();
				}
				if (_text != null)
				{
					_text.color = swapColor;
				}
			}
			if (toggleObjects != null && toggleObjects.Length != 0)
			{
				GameObject[] array = toggleObjects;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: true);
				}
			}
			if (_dialogueResponse != null && _eventSystem.currentSelectedGameObject != base.gameObject)
			{
				_eventSystem.SetSelectedGameObject(base.gameObject);
			}
			if (hoverScale != Vector2.one)
			{
				initialScale = base.transform.localScale;
				base.transform.DOScale(hoverScale, 0.2f);
			}
			if (_animator != null && _animator.enabled && controlAnimation)
			{
				_animator.speed = _cachedAnimSpeed;
			}
		}

		private void UnHighlight(bool tween = true)
		{
			if (changeStyle && _text != null)
			{
				_text.color = _origTextColor;
			}
			if (toggleObjects != null && toggleObjects.Length != 0)
			{
				GameObject[] array = toggleObjects;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: false);
				}
			}
			if (hoverScale != Vector2.one)
			{
				if (tween)
				{
					base.transform.DOScale(initialScale, 0.2f);
				}
				else
				{
					base.transform.localScale = initialScale;
				}
			}
			if (_animator != null && _animator.enabled && controlAnimation)
			{
				_animator.speed = 0f;
			}
		}
	}
}
