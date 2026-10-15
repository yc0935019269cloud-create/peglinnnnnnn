using PeglinUI.ControllerSupport;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArrowSelection : MonoBehaviour
{
	public GameObject overrideParent;

	[HideInInspector]
	public RewiredEventSystem rewiredEventSystem;

	private Image _image;

	private GameObject _selectable;

	private void OnEnable()
	{
		if (_image == null)
		{
			_image = GetComponent<Image>();
		}
		_image.enabled = false;
		_selectable = ((overrideParent != null) ? overrideParent : base.transform.parent.gameObject);
	}

	private void Update()
	{
		if (!ControllerInputManager.IsPointerBased)
		{
			if (!_image.enabled && EventSystem.current.currentSelectedGameObject == _selectable)
			{
				_image.enabled = true;
			}
			else if (_image.enabled && EventSystem.current.currentSelectedGameObject != _selectable)
			{
				_image.enabled = false;
			}
		}
	}
}
