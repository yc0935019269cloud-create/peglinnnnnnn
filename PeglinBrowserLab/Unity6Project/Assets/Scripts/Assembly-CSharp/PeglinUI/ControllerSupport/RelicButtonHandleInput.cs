using System.Collections;
using PeglinUtils;
using Relics;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeglinUI.ControllerSupport
{
	public class RelicButtonHandleInput : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public Image selectionArrow;

		private RewiredEventSystem _rewiredEventSystem;

		private bool _hovering;

		private bool _highlighted;

		private Relic _relic;

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

		public void OnSelect(BaseEventData eventData)
		{
			Highlight(followMouse: false);
		}

		public void OnDeselect(BaseEventData eventData)
		{
			UnHighlight();
		}

		public void OnPointerEnter(PointerEventData ped)
		{
			Highlight(followMouse: true);
			_hovering = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			UnHighlight();
			_hovering = false;
		}

		private void Highlight(bool followMouse)
		{
			_highlighted = true;
			RelicIcon componentInChildren = GetComponentInChildren<RelicIcon>();
			if (componentInChildren != null)
			{
				_relic = componentInChildren.relic;
			}
			else
			{
				Debug.LogWarning("RelicIcon not found in children of object: " + base.gameObject.name + " with applied script RelicButtonHandleInput");
			}
			if (selectionArrow != null)
			{
				Vector3 position = selectionArrow.rectTransform.position;
				position.x = base.transform.position.x;
				selectionArrow.transform.position = position;
				selectionArrow.enabled = true;
			}
			if (_relic != null && TooltipManager.Instance != null && componentInChildren != null)
			{
				TooltipManager.Instance.ShowTooltipRelic(_relic, componentInChildren.transform.position, componentInChildren.tooltipAnchor, followMouse);
			}
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
				if (TooltipManager.Instance != null)
				{
					TooltipManager.Instance.HideTooltip();
				}
				if (selectionArrow != null)
				{
					selectionArrow.enabled = false;
				}
			}
		}
	}
}
