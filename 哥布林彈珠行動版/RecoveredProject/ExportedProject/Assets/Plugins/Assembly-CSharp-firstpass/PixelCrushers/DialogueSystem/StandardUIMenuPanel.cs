using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class StandardUIMenuPanel : UIPanel
	{
		[Tooltip("(Optional) Main response menu panel.")]
		public Graphic panel;

		[Tooltip("(Optional) Image to show PC portrait during response menu.")]
		public Image pcImage;

		[Tooltip("(Optional) Text element to show PC name during response menu.")]
		public UITextField pcName;

		[Tooltip("Set PC Image to actor portrait's native size. Image's Rect Transform can't use Stretch anchors.")]
		public bool usePortraitNativeSize;

		[Tooltip("(Optional) Slider for timed menus.")]
		public Slider timerSlider;

		[Tooltip("Assign design-time positioned buttons starting with first or last button.")]
		public ResponseButtonAlignment buttonAlignment;

		[Tooltip("Show buttons that aren't assigned to any responses. If using a 'dialogue wheel' for example, you'll want to show unused buttons so the entire wheel structure is visible.")]
		public bool showUnusedButtons;

		[Tooltip("Design-time positioned response buttons. (Optional if Button Template is assigned.)")]
		public StandardUIResponseButton[] buttons;

		[Tooltip("Template from which to instantiate response buttons. (Optional if using Buttons list above.)")]
		public StandardUIResponseButton buttonTemplate;

		[Tooltip("If using Button Template, instantiate buttons under this GameObject.")]
		public Graphic buttonTemplateHolder;

		[Tooltip("(Optional) Scrollbar to use if instantiated button holder is in a scroll rect.")]
		public Scrollbar buttonTemplateScrollbar;

		[Tooltip("(Optional) Component that enables or disables scrollbar as necessary for content.")]
		public UIScrollbarEnabler scrollbarEnabler;

		[Tooltip("Reset the scroll bar to this value when preparing response menu. To skip resetting the scrollbar, specify a negative value.")]
		public float buttonTemplateScrollbarResetValue = 1f;

		[Tooltip("Automatically set up explicit joystick/keyboard navigation for instantiated template buttons instead of using Automatic navigation.")]
		public bool explicitNavigationForTemplateButtons = true;

		[Tooltip("If explicit navigation is enabled, loop around when navigating past end of menu.")]
		public bool loopExplicitNavigation;

		public UIAutonumberSettings autonumber = new UIAutonumberSettings();

		[Tooltip("If non-zero, prevent input for this duration in seconds when opening menu.")]
		public float blockInputDuration;

		public UnityEvent onContentChanged = new UnityEvent();

		[Tooltip("When focusing panel, set this animator trigger.")]
		public string focusAnimationTrigger = string.Empty;

		[Tooltip("When unfocusing panel, set this animator trigger.")]
		public string unfocusAnimationTrigger = string.Empty;

		[Tooltip("Wait for panels within this dialogue UI (not external) to close before showing menu.")]
		public bool waitForClose;

		public UnityEvent onFocus = new UnityEvent();

		public UnityEvent onUnfocus = new UnityEvent();

		[SerializeField]
		[Tooltip("Panel is currently in focused state.")]
		private bool m_hasFocus;

		private List<GameObject> m_instantiatedButtons = new List<GameObject>();

		private List<GameObject> m_instantiatedButtonPool = new List<GameObject>();

		private string m_processedAutonumberFormat = string.Empty;

		protected const float WaitForCloseTimeoutDuration = 8f;

		protected StandardUITimer m_timer;

		protected Action m_timeoutHandler;

		protected CanvasGroup m_mainCanvasGroup;

		protected static bool s_isInputDisabled;

		private StandardDialogueUI m_dialogueUI;

		public virtual bool hasFocus
		{
			get
			{
				return m_hasFocus;
			}
			protected set
			{
				m_hasFocus = value;
			}
		}

		public override bool waitForShowAnimation => true;

		public List<GameObject> instantiatedButtons => m_instantiatedButtons;

		protected List<GameObject> instantiatedButtonPool => m_instantiatedButtonPool;

		protected StandardDialogueUI dialogueUI
		{
			get
			{
				if (m_dialogueUI == null)
				{
					m_dialogueUI = GetComponentInParent<StandardDialogueUI>();
				}
				return m_dialogueUI ?? DialogueManager.standardDialogueUI;
			}
		}

		public virtual void Awake()
		{
			Tools.SetGameObjectActive(buttonTemplate, value: false);
		}

		public virtual void SetPCPortrait(Sprite portraitSprite, string portraitName)
		{
			if (pcImage != null)
			{
				Tools.SetGameObjectActive(pcImage, portraitSprite != null);
				pcImage.sprite = portraitSprite;
				if (usePortraitNativeSize && portraitSprite != null)
				{
					pcImage.rectTransform.sizeDelta = new Vector2(portraitSprite.texture.width, portraitSprite.texture.height);
				}
			}
			pcName.text = portraitName;
		}

		[Obsolete("Use SetPCPortrait(Sprite,string) instead.")]
		public virtual void SetPCPortrait(Texture2D portraitTexture, string portraitName)
		{
			SetPCPortrait(UITools.CreateSprite(portraitTexture), portraitName);
		}

		public virtual void ShowResponses(Subtitle subtitle, Response[] responses, Transform target)
		{
			if (waitForClose && dialogueUI != null && dialogueUI.AreAnyPanelsClosing())
			{
				DialogueManager.instance.StartCoroutine(ShowAfterPanelsClose(subtitle, responses, target));
			}
			else
			{
				ShowResponsesNow(subtitle, responses, target);
			}
		}

		protected virtual void ShowResponsesNow(Subtitle subtitle, Response[] responses, Transform target)
		{
			if (responses == null || responses.Length == 0)
			{
				if (DialogueDebug.logWarnings)
				{
					Debug.LogWarning("Dialogue System: StandardDialogueUI ShowResponses received an empty list of responses.", this);
				}
				return;
			}
			ClearResponseButtons();
			SetResponseButtons(responses, target);
			ActivateUIElements();
			Open();
			Focus();
			if (blockInputDuration > 0f)
			{
				DisableInput();
				Invoke("EnableInput", blockInputDuration);
			}
			else if (s_isInputDisabled)
			{
				EnableInput();
			}
			StartCoroutine(CheckTMProAutoScroll());
		}

		protected IEnumerator CheckTMProAutoScroll()
		{
			StandardDialogueUI componentInParent = GetComponentInParent<StandardDialogueUI>();
			if (componentInParent == null || componentInParent.conversationUIElements.defaultNPCSubtitlePanel == null || componentInParent.conversationUIElements.defaultNPCSubtitlePanel.subtitleText == null)
			{
				yield break;
			}
			TextMeshProUGUI textMeshProUGUI = componentInParent.conversationUIElements.defaultNPCSubtitlePanel.subtitleText.textMeshProUGUI;
			if (!(textMeshProUGUI == null))
			{
				LayoutElement component = textMeshProUGUI.GetComponent<LayoutElement>();
				if (component != null)
				{
					component.preferredHeight = -1f;
				}
				UIScrollbarEnabler uiScrollbarEnabler = GetComponentInParent<UIScrollbarEnabler>();
				if (uiScrollbarEnabler != null)
				{
					yield return null;
					uiScrollbarEnabler.CheckScrollbarWithResetValue(buttonTemplateScrollbarResetValue);
				}
			}
		}

		protected virtual IEnumerator ShowAfterPanelsClose(Subtitle subtitle, Response[] responses, Transform target)
		{
			if (dialogueUI != null)
			{
				float safeguardTime = Time.realtimeSinceStartup + 8f;
				while (dialogueUI.AreAnyPanelsClosing() && Time.realtimeSinceStartup < safeguardTime)
				{
					yield return null;
				}
			}
			ShowResponsesNow(subtitle, responses, target);
		}

		public virtual void HideResponses()
		{
			StopTimer();
			Unfocus();
			Close();
		}

		public override void Close()
		{
			if (base.isOpen)
			{
				base.Close();
			}
		}

		public virtual void Focus()
		{
			if (!hasFocus)
			{
				if (base.panelState == PanelState.Opening && base.enabled && base.gameObject.activeInHierarchy)
				{
					StartCoroutine(FocusWhenOpen());
				}
				else
				{
					FocusNow();
				}
			}
		}

		protected IEnumerator FocusWhenOpen()
		{
			float timeout = Time.realtimeSinceStartup + 5f;
			while (base.panelState != PanelState.Open && Time.realtimeSinceStartup < timeout)
			{
				yield return null;
			}
			FocusNow();
		}

		protected virtual void FocusNow()
		{
			base.panelState = PanelState.Open;
			base.animatorMonitor.SetTrigger(focusAnimationTrigger, null, wait: false);
			UITools.EnableInteractivity(base.gameObject);
			if (!hasFocus)
			{
				if (string.IsNullOrEmpty(focusAnimationTrigger))
				{
					OnFocused();
				}
				else
				{
					base.animatorMonitor.SetTrigger(focusAnimationTrigger, OnFocused);
				}
				onFocus.Invoke();
			}
		}

		private void OnFocused()
		{
			hasFocus = true;
		}

		public virtual void Unfocus()
		{
			if (hasFocus)
			{
				hasFocus = false;
				base.animatorMonitor.SetTrigger(unfocusAnimationTrigger, null, wait: false);
				onUnfocus.Invoke();
			}
		}

		protected void ActivateUIElements()
		{
			SetUIElementsActive(value: true);
		}

		protected void DeactivateUIElements()
		{
			SetUIElementsActive(value: false);
		}

		protected virtual void SetUIElementsActive(bool value)
		{
			Tools.SetGameObjectActive(panel, value);
			Tools.SetGameObjectActive(pcImage, value && pcImage != null && pcImage.sprite != null);
			pcName.SetActive(value);
			Tools.SetGameObjectActive(timerSlider, value: false);
			if (!value)
			{
				ClearResponseButtons();
			}
		}

		public virtual void HideImmediate()
		{
			DeactivateUIElements();
		}

		protected virtual void ClearResponseButtons()
		{
			DestroyInstantiatedButtons();
			if (buttons == null)
			{
				return;
			}
			for (int i = 0; i < buttons.Length; i++)
			{
				if (!(buttons[i] == null))
				{
					buttons[i].Reset();
					buttons[i].isVisible = showUnusedButtons;
					buttons[i].gameObject.SetActive(showUnusedButtons);
				}
			}
		}

		protected virtual void SetResponseButtons(Response[] responses, Transform target)
		{
			firstSelected = null;
			DestroyInstantiatedButtons();
			bool hasDisabledButton = false;
			if (autonumber.enabled)
			{
				m_processedAutonumberFormat = autonumber.format.Replace("\\t", "\t").Replace("\\n", "\n");
			}
			if (buttons != null && responses != null)
			{
				int num = 0;
				for (int i = 0; i < responses.Length; i++)
				{
					if (responses[i].formattedText.position != -1)
					{
						int position = responses[i].formattedText.position;
						if (0 <= position && position < buttons.Length && buttons[position] != null)
						{
							SetResponseButton(buttons[position], responses[i], target, num++);
						}
						else
						{
							Debug.LogWarning("Dialogue System: Buttons list doesn't contain a button for position " + position + ".", this);
						}
					}
				}
				if (buttonTemplate != null && buttonTemplateHolder != null)
				{
					if (buttonTemplateScrollbarResetValue >= 0f)
					{
						if (buttonTemplateScrollbar != null)
						{
							buttonTemplateScrollbar.value = buttonTemplateScrollbarResetValue;
						}
						if (scrollbarEnabler != null)
						{
							scrollbarEnabler.CheckScrollbarWithResetValue(buttonTemplateScrollbarResetValue);
						}
					}
					else if (scrollbarEnabler != null)
					{
						scrollbarEnabler.CheckScrollbar();
					}
					for (int j = 0; j < responses.Length; j++)
					{
						if (responses[j].formattedText.position != -1)
						{
							continue;
						}
						GameObject gameObject = InstantiateButton();
						if (gameObject == null)
						{
							Debug.LogError("Dialogue System: Couldn't instantiate response button template.");
							continue;
						}
						instantiatedButtons.Add(gameObject);
						gameObject.transform.SetParent(buttonTemplateHolder.transform, worldPositionStays: false);
						gameObject.transform.SetAsLastSibling();
						gameObject.SetActive(value: true);
						StandardUIResponseButton component = gameObject.GetComponent<StandardUIResponseButton>();
						SetResponseButton(component, responses[j], target, num++);
						if (component != null)
						{
							gameObject.name = "Response: " + component.text;
							if (explicitNavigationForTemplateButtons && !component.isClickable)
							{
								hasDisabledButton = true;
							}
						}
						if (firstSelected == null)
						{
							firstSelected = gameObject;
						}
					}
				}
				else if (buttonAlignment == ResponseButtonAlignment.ToFirst)
				{
					for (int k = 0; k < Mathf.Min(buttons.Length, responses.Length); k++)
					{
						if (responses[k].formattedText.position == -1)
						{
							int num2 = Mathf.Clamp(GetNextAvailableResponseButtonPosition(0, 1), 0, buttons.Length - 1);
							SetResponseButton(buttons[num2], responses[k], target, num++);
							if (firstSelected == null)
							{
								firstSelected = buttons[num2].gameObject;
							}
						}
					}
				}
				else
				{
					for (int num3 = Mathf.Min(buttons.Length, responses.Length) - 1; num3 >= 0; num3--)
					{
						if (responses[num3].formattedText.position == -1)
						{
							int num4 = Mathf.Clamp(GetNextAvailableResponseButtonPosition(buttons.Length - 1, -1), 0, buttons.Length - 1);
							SetResponseButton(buttons[num4], responses[num3], target, num++);
							firstSelected = buttons[num4].gameObject;
						}
					}
				}
			}
			if (explicitNavigationForTemplateButtons)
			{
				SetupTemplateButtonNavigation(hasDisabledButton);
			}
			if (InputDeviceManager.autoFocus)
			{
				SetFocus(firstSelected);
			}
			NotifyContentChanged();
		}

		protected virtual void SetResponseButton(StandardUIResponseButton button, Response response, Transform target, int buttonNumber)
		{
			if (!(button != null))
			{
				return;
			}
			button.gameObject.SetActive(value: true);
			button.isVisible = true;
			button.isClickable = response.enabled;
			button.target = target;
			if (response != null)
			{
				button.SetFormattedText(response.formattedText);
			}
			button.response = response;
			if (!autonumber.enabled)
			{
				return;
			}
			button.text = string.Format(m_processedAutonumberFormat, buttonNumber + 1, button.text);
			UIButtonKeyTrigger uIButtonKeyTrigger = button.GetComponent<UIButtonKeyTrigger>();
			if (autonumber.regularNumberHotkeys)
			{
				if (uIButtonKeyTrigger == null)
				{
					uIButtonKeyTrigger = button.gameObject.AddComponent<UIButtonKeyTrigger>();
				}
				uIButtonKeyTrigger.key = (KeyCode)(49 + buttonNumber);
			}
			if (autonumber.numpadHotkeys)
			{
				if (autonumber.regularNumberHotkeys || uIButtonKeyTrigger == null)
				{
					uIButtonKeyTrigger = button.gameObject.AddComponent<UIButtonKeyTrigger>();
				}
				uIButtonKeyTrigger.key = (KeyCode)(257 + buttonNumber);
			}
		}

		protected int GetNextAvailableResponseButtonPosition(int start, int direction)
		{
			if (buttons != null)
			{
				for (int i = start; 0 <= i && i < buttons.Length; i += direction)
				{
					if (!buttons[i].isVisible || buttons[i].response == null)
					{
						return i;
					}
				}
			}
			return 5;
		}

		public virtual void SetupTemplateButtonNavigation(bool hasDisabledButton)
		{
			if (instantiatedButtons == null || instantiatedButtons.Count == 0)
			{
				return;
			}
			List<GameObject> list = new List<GameObject>();
			if (hasDisabledButton)
			{
				list.AddRange(instantiatedButtons.FindAll((GameObject x) => x.GetComponent<StandardUIResponseButton>().isClickable));
			}
			else
			{
				list.AddRange(instantiatedButtons);
			}
			for (int num = 0; num < list.Count; num++)
			{
				Button component = list[num].GetComponent<Button>();
				if (!(component == null))
				{
					Button button = ((num != 0) ? list[num - 1].GetComponent<Button>() : (loopExplicitNavigation ? list[list.Count - 1].GetComponent<Button>() : null));
					Button button2 = ((num != list.Count - 1) ? list[num + 1].GetComponent<Button>() : (loopExplicitNavigation ? list[0].GetComponent<Button>() : null));
					component.navigation = new Navigation
					{
						mode = Navigation.Mode.Explicit,
						selectOnUp = button,
						selectOnLeft = button,
						selectOnDown = button2,
						selectOnRight = button2
					};
				}
			}
		}

		protected virtual GameObject InstantiateButton()
		{
			if (m_instantiatedButtonPool.Count > 0)
			{
				GameObject result = m_instantiatedButtonPool[0];
				m_instantiatedButtonPool.RemoveAt(0);
				return result;
			}
			return UnityEngine.Object.Instantiate(buttonTemplate.gameObject);
		}

		public void DestroyInstantiatedButtons()
		{
			for (int i = 0; i < instantiatedButtons.Count; i++)
			{
				instantiatedButtons[i].SetActive(value: false);
			}
			m_instantiatedButtonPool.AddRange(instantiatedButtons);
			instantiatedButtons.Clear();
			NotifyContentChanged();
		}

		public virtual void MakeButtonsNonclickable()
		{
			for (int i = 0; i < instantiatedButtons.Count; i++)
			{
				StandardUIResponseButton standardUIResponseButton = ((instantiatedButtons[i] != null) ? instantiatedButtons[i].GetComponent<StandardUIResponseButton>() : null);
				if (standardUIResponseButton != null)
				{
					standardUIResponseButton.isClickable = false;
				}
			}
			for (int j = 0; j < buttons.Length; j++)
			{
				if (buttons[j] != null)
				{
					buttons[j].isClickable = false;
				}
			}
		}

		protected void NotifyContentChanged()
		{
			onContentChanged.Invoke();
		}

		protected void DisableInput()
		{
			SetInput(value: false);
		}

		protected void EnableInput()
		{
			SetInput(value: true);
		}

		protected void SetInput(bool value)
		{
			s_isInputDisabled = !value;
			if (m_mainCanvasGroup == null)
			{
				StandardDialogueUI componentInParent = GetComponentInParent<StandardDialogueUI>();
				if (componentInParent != null && componentInParent.conversationUIElements.mainPanel != null)
				{
					UIPanel mainPanel = componentInParent.conversationUIElements.mainPanel;
					m_mainCanvasGroup = mainPanel.GetComponent<CanvasGroup>() ?? mainPanel.gameObject.AddComponent<CanvasGroup>();
				}
				else
				{
					Graphic graphic = panel;
					if (graphic == null)
					{
						graphic = buttonTemplateHolder;
					}
					if (graphic != null)
					{
						m_mainCanvasGroup = graphic.GetComponent<CanvasGroup>() ?? graphic.gameObject.AddComponent<CanvasGroup>();
					}
				}
			}
			if (m_mainCanvasGroup != null)
			{
				m_mainCanvasGroup.interactable = value;
			}
			if (EventSystem.current != null)
			{
				PointerInputModule component = EventSystem.current.GetComponent<PointerInputModule>();
				if (component != null)
				{
					component.enabled = value;
				}
			}
			PixelCrushers.UIButtonKeyTrigger.monitorInput = value;
			if (value)
			{
				RefreshSelectablesList();
				CheckFocus();
			}
		}

		public virtual void StartTimer(float timeout, Action timeoutHandler)
		{
			if (m_timer == null)
			{
				if (timerSlider != null)
				{
					Tools.SetGameObjectActive(timerSlider, value: true);
					m_timer = timerSlider.GetComponent<StandardUITimer>();
					if (m_timer == null)
					{
						m_timer = timerSlider.gameObject.AddComponent<StandardUITimer>();
					}
				}
				else
				{
					m_timer = GetComponentInChildren<StandardUITimer>();
					if (m_timer == null)
					{
						m_timer = base.gameObject.AddComponent<StandardUITimer>();
					}
				}
			}
			Tools.SetGameObjectActive(m_timer, value: true);
			m_timer.StartCountdown(timeout, timeoutHandler);
		}

		public virtual void StopTimer()
		{
			if (m_timer != null)
			{
				m_timer.StopCountdown();
				Tools.SetGameObjectActive(m_timer, value: false);
			}
		}
	}
}
