using System.Collections;
using PeglinUtils;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Challenges
{
	public class ChallengeButtonHandleInput : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public Image selectionArrow;

		private RewiredEventSystem _rewiredEventSystem;

		private bool _hovering;

		private bool _highlighted;

		private ChallengeEffect _challenge;

		public void Start()
		{
			_rewiredEventSystem = GameObject.FindWithTag("InputSystem").GetComponent<RewiredEventSystem>();
			StartCoroutine(FixAlignment());
		}

		private void OnDisable()
		{
			if (_highlighted)
			{
				UnHighlight();
			}
		}

		public void StartFixAlignmentRoutine()
		{
			StartCoroutine(FixAlignment());
		}

		private IEnumerator FixAlignment()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			if (_rewiredEventSystem.currentSelectedGameObject == base.gameObject)
			{
				Highlight(!_hovering);
			}
		}

		private void Highlight(bool followMouse)
		{
			_highlighted = true;
			ChallengeIcon componentInChildren = GetComponentInChildren<ChallengeIcon>();
			_challenge = componentInChildren.challenge;
			if (selectionArrow != null)
			{
				Vector3 position = selectionArrow.rectTransform.position;
				position.x = base.transform.position.x;
				selectionArrow.transform.position = position;
				selectionArrow.enabled = true;
			}
			TooltipManager.Instance.ShowTooltipChallenge(_challenge, componentInChildren.transform.position, componentInChildren.tooltipAnchor, followMouse);
			if (_rewiredEventSystem.currentSelectedGameObject != null && _rewiredEventSystem.currentSelectedGameObject != base.gameObject)
			{
				_rewiredEventSystem.SetSelectedGameObject(base.gameObject);
			}
		}

		private void UnHighlight()
		{
			if (_highlighted)
			{
				_highlighted = false;
				TooltipManager.Instance.HideTooltip();
				if (selectionArrow != null)
				{
					selectionArrow.enabled = false;
				}
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			Highlight(followMouse: false);
		}

		public void OnDeselect(BaseEventData eventData)
		{
			UnHighlight();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			Highlight(followMouse: true);
			_hovering = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			UnHighlight();
			_hovering = false;
		}
	}
}
