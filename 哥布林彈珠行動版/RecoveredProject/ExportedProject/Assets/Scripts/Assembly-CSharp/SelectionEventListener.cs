using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionEventListener : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public delegate void SelectionEvent();

	public SelectionEvent OnSelectionEvent;

	public SelectionEvent OnDeselectionEvent;

	public void OnSelect(BaseEventData eventData)
	{
		OnSelectionEvent?.Invoke();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		OnDeselectionEvent?.Invoke();
	}
}
