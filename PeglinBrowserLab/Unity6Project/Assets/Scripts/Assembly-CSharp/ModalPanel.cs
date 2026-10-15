using Peglin.OdinSerializer.Utilities;
using PeglinUI.ControllerSupport;
using Rewired.Integration.UnityUI;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class ModalPanel : MonoBehaviour
{
	[SerializeField]
	private Button _yesButton;

	[SerializeField]
	private Button _noButton;

	[SerializeField]
	private Button _okayButton;

	[SerializeField]
	private TextMeshProUGUI _questionText;

	[SerializeField]
	private GameObject _dismissCheckmarkParent;

	[SerializeField]
	private Toggle _dismissToggle;

	private RewiredEventSystem _rewiredEventSystem;

	private ControllerMenuHookup _controllerMenuHookup;

	private string _checkmarkDismissKey;

	private void Awake()
	{
		_rewiredEventSystem = GameObject.FindGameObjectWithTag("InputSystem").GetComponent<RewiredEventSystem>();
		_controllerMenuHookup = _rewiredEventSystem.GetComponent<ControllerMenuHookup>();
	}

	private void OnEnable()
	{
		_checkmarkDismissKey = "";
	}

	public void Choice(string question, UnityAction yesCallback, UnityAction noCallback)
	{
		base.gameObject.SetActive(value: true);
		_questionText.text = question;
		_yesButton.gameObject.SetActive(value: true);
		_noButton.gameObject.SetActive(value: true);
		_okayButton.gameObject.SetActive(value: false);
		_yesButton.onClick.RemoveAllListeners();
		_yesButton.onClick.AddListener(yesCallback);
		_yesButton.onClick.AddListener(ClosePanel);
		_controllerMenuHookup.LastSelected = _yesButton.gameObject;
		if (!ControllerInputManager.IsPointerBased)
		{
			_rewiredEventSystem.SetSelectedGameObject(_yesButton.gameObject);
			ButtonHandleHover component = _yesButton.GetComponent<ButtonHandleHover>();
			if (component != null)
			{
				component.Highlight();
			}
		}
		_noButton.onClick.RemoveAllListeners();
		_noButton.onClick.AddListener(ClosePanel);
		_noButton.onClick.AddListener(noCallback);
	}

	public void Notify(string notificationText)
	{
		base.gameObject.SetActive(value: true);
		_questionText.text = notificationText;
		_noButton.gameObject.SetActive(value: false);
		_yesButton.gameObject.SetActive(value: false);
		_dismissCheckmarkParent.SetActive(value: false);
		_okayButton.gameObject.SetActive(value: true);
		_okayButton.onClick.RemoveAllListeners();
		_okayButton.onClick.AddListener(ClosePanel);
		EventSystem.current.SetSelectedGameObject(_okayButton.gameObject);
	}

	public void NotifyWithDismiss(string notificationText, string dismissOption)
	{
		if (PlayerPrefs.GetInt(dismissOption, 0) <= 0)
		{
			Notify(notificationText);
			_dismissCheckmarkParent.SetActive(value: true);
			_checkmarkDismissKey = dismissOption;
			_okayButton.onClick.AddListener(SaveCheckmarkState);
		}
	}

	public void ClosePanel()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SaveCheckmarkState()
	{
		if (!_checkmarkDismissKey.IsNullOrWhitespace())
		{
			PlayerPrefs.SetInt(_checkmarkDismissKey, _dismissToggle.isOn ? 1 : 0);
		}
	}
}
