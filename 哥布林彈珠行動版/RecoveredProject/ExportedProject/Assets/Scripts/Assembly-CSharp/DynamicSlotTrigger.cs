using System;
using DG.Tweening;
using Peglin.PegMinigame;
using PeglinUI;
using UnityEngine;

public class DynamicSlotTrigger : MonoBehaviour
{
	public delegate void SlotShowingTooltip(DynamicSlotTrigger slot);

	public delegate void SlotStopShowingTooltip(DynamicSlotTrigger slot);

	public delegate void SlotTriggerActivated(int index);

	public static SlotShowingTooltip OnSlotShowingTooltip = delegate
	{
	};

	public static SlotStopShowingTooltip OnSlotStopShowingTooltip = delegate
	{
	};

	private const float FADE_TIME = 0.3f;

	private const float FADE_ALPHA = 0.4f;

	[Header("SceneRefs")]
	[SerializeField]
	private SpriteRenderer iconSpriteRenderer;

	public SelectionEventListener selectionEventListener;

	private SlotTriggerActivated onSlotTriggerActivated = delegate
	{
	};

	private int index;

	private BoxCollider2D col;

	private Reward reward;

	private Canvas tooltipCanvas;

	private static float Z_OFFSET = -15f;

	private bool _shouldDisplayTooltip = true;

	private SpriteRenderer iconSprite;

	public void Initialize(int index, SlotTriggerActivated onSlotTriggerActivated, float slotLength, Reward r)
	{
		iconSprite = GetComponentInChildren<SpriteRenderer>();
		this.index = index;
		this.onSlotTriggerActivated = (SlotTriggerActivated)Delegate.Combine(this.onSlotTriggerActivated, onSlotTriggerActivated);
		if (col == null)
		{
			col = GetComponent<BoxCollider2D>();
		}
		col.size = new Vector2(slotLength, col.size.y);
		if (r != null)
		{
			reward = r;
			SetSprite(r.GetSprite(), r.GetSpriteColor(), r.GetSpriteLocalScale());
			SelectionEventListener obj = selectionEventListener;
			obj.OnSelectionEvent = (SelectionEventListener.SelectionEvent)Delegate.Combine(obj.OnSelectionEvent, new SelectionEventListener.SelectionEvent(ShowTooltip));
			SelectionEventListener obj2 = selectionEventListener;
			obj2.OnDeselectionEvent = (SelectionEventListener.SelectionEvent)Delegate.Combine(obj2.OnDeselectionEvent, new SelectionEventListener.SelectionEvent(HideTooltip));
		}
		base.transform.position += new Vector3(0f, 0f, Z_OFFSET);
		MouseOverDetector componentInChildren = GetComponentInChildren<MouseOverDetector>();
		componentInChildren.OnMouseEntered = (MouseOverDetector.MouseEvent)Delegate.Combine(componentInChildren.OnMouseEntered, (MouseOverDetector.MouseEvent)((GameObject x) =>
		{
			ShowTooltip();
		}));
		MouseOverDetector componentInChildren2 = GetComponentInChildren<MouseOverDetector>();
		componentInChildren2.OnMouseExited = (MouseOverDetector.MouseEvent)Delegate.Combine(componentInChildren2.OnMouseExited, (MouseOverDetector.MouseEvent)((GameObject x) =>
		{
			HideTooltip();
		}));
	}

	private void ShowTooltip()
	{
		if (_shouldDisplayTooltip)
		{
			reward?.DrawTooltip(GetTooltipCanvas(), base.transform.position);
			OnSlotShowingTooltip?.Invoke(this);
		}
	}

	private void HideTooltip()
	{
		reward?.DisableTooltip();
		OnSlotStopShowingTooltip?.Invoke(this);
	}

	public void DisableTooltips()
	{
		reward?.DisableTooltip();
		_shouldDisplayTooltip = false;
	}

	public void SetSprite(Sprite s, Color c, Vector3 localScale)
	{
		iconSpriteRenderer.sprite = s;
		iconSpriteRenderer.color = c;
		iconSpriteRenderer.transform.localScale *= localScale.x;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.GetComponent<PachinkoBall>() != null)
		{
			onSlotTriggerActivated(index);
		}
	}

	private Canvas GetTooltipCanvas()
	{
		if (tooltipCanvas == null)
		{
			tooltipCanvas = GameObject.FindWithTag("TooltipCanvas")?.GetComponent<Canvas>();
		}
		return tooltipCanvas;
	}

	public void OnASlotIconShowingTooltip(DynamicSlotTrigger slot)
	{
		if (slot == this)
		{
			iconSprite.DOKill();
			iconSprite.DOColor(Color.white, 0.3f);
		}
		else
		{
			iconSprite.DOKill();
			iconSprite.DOColor(new Color(1f, 1f, 1f, 0.4f), 0.3f);
		}
	}

	public void OnASlotIconHidingTooltip(DynamicSlotTrigger slot)
	{
		iconSprite.DOKill();
		iconSprite.DOColor(Color.white, 0.3f);
	}

	public void IgnoreSelectionHidingAndShowing()
	{
		OnSlotShowingTooltip = (SlotShowingTooltip)Delegate.Remove(OnSlotShowingTooltip, new SlotShowingTooltip(OnASlotIconShowingTooltip));
		OnSlotStopShowingTooltip = (SlotStopShowingTooltip)Delegate.Remove(OnSlotStopShowingTooltip, new SlotStopShowingTooltip(OnASlotIconHidingTooltip));
		iconSprite.DOKill();
	}

	private void OnEnable()
	{
		OnSlotShowingTooltip = (SlotShowingTooltip)Delegate.Combine(OnSlotShowingTooltip, new SlotShowingTooltip(OnASlotIconShowingTooltip));
		OnSlotStopShowingTooltip = (SlotStopShowingTooltip)Delegate.Combine(OnSlotStopShowingTooltip, new SlotStopShowingTooltip(OnASlotIconHidingTooltip));
	}

	private void OnDisable()
	{
		OnSlotShowingTooltip = (SlotShowingTooltip)Delegate.Remove(OnSlotShowingTooltip, new SlotShowingTooltip(OnASlotIconShowingTooltip));
		OnSlotStopShowingTooltip = (SlotStopShowingTooltip)Delegate.Remove(OnSlotStopShowingTooltip, new SlotStopShowingTooltip(OnASlotIconHidingTooltip));
		onSlotTriggerActivated = null;
	}
}
