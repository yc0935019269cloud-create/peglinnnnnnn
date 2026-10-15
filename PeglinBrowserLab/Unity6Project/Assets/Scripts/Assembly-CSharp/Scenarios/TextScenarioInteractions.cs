using System;
using System.Collections;
using System.Collections.Generic;
using PeglinUI;
using PeglinUI.ControllerSupport;
using PeglinUI.PostBattle;
using Relics;
using Rewired.Integration.UnityUI;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Scenarios
{
	public class TextScenarioInteractions : MonoBehaviour
	{
		public static Action OnInteractionComplete = () =>
		{
		};

		public static bool isOpen = false;

		public RelicManager relicManager;

		public GameObject mainText;

		public GameObject relicPanel;

		public Button skipRelicButton;

		public GameObject orbListPanel;

		public UpgradeConfirmationRelic upgradeConfirmationRelic;

		private RewiredEventSystem _eventSystem;

		private Relic _stagedRelic;

		private void Awake()
		{
			_eventSystem = GameObject.FindWithTag("InputSystem").GetComponent<RewiredEventSystem>();
		}

		private void OnEnable()
		{
			UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Combine(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(HandleUpgradeOptionClicked));
			mainText.SetActive(value: false);
			isOpen = true;
		}

		private void OnDisable()
		{
			UpgradeOption.OnUpgradeOptionClicked = (UpgradeOption.UpgradeOptionClicked)Delegate.Remove(UpgradeOption.OnUpgradeOptionClicked, new UpgradeOption.UpgradeOptionClicked(HandleUpgradeOptionClicked));
			if (mainText != null && isOpen)
			{
				mainText.SetActive(value: true);
				isOpen = false;
			}
		}

		public void OfferRelic(RelicRarity rarity, int numChoices = 1)
		{
			relicPanel.SetActive(value: true);
			RelicIcon[] componentsInChildren = relicPanel.GetComponentsInChildren<RelicIcon>();
			Relic[] multipleRelicsOffOfQueue = relicManager.GetMultipleRelicsOffOfQueue(numChoices, rarity);
			List<RelicRarity> list = new List<RelicRarity>();
			for (int i = 0; i < numChoices; i++)
			{
				list.Add(rarity);
			}
			multipleRelicsOffOfQueue = relicManager.CheckAndUpdateSeededRelicOffers(multipleRelicsOffOfQueue, list.ToArray());
			int num = 0;
			RelicIcon[] array = componentsInChildren;
			foreach (RelicIcon relicIcon in array)
			{
				if (num < multipleRelicsOffOfQueue.Length)
				{
					relicIcon.SetRelic(multipleRelicsOffOfQueue[num]);
				}
				else
				{
					componentsInChildren[num].transform.parent.gameObject.SetActive(value: false);
				}
				num++;
			}
			Button componentInParent = componentsInChildren[0].GetComponentInParent<Button>();
			StartCoroutine(SetSelectedButton(componentInParent.gameObject));
			skipRelicButton.gameObject.SetActive(value: true);
		}

		public void OfferSpecificRelic(Relic relic)
		{
			relicPanel.SetActive(value: true);
			RelicIcon[] componentsInChildren = relicPanel.GetComponentsInChildren<RelicIcon>();
			int num = 0;
			RelicIcon[] array = componentsInChildren;
			foreach (RelicIcon relicIcon in array)
			{
				if (num == 0)
				{
					relicIcon.SetRelic(relic);
					Button componentInParent = relicIcon.GetComponentInParent<Button>();
					StartCoroutine(SetSelectedButton(componentInParent.gameObject));
				}
				else if (componentsInChildren[num].transform.parent.gameObject.activeInHierarchy)
				{
					componentsInChildren[num].transform.parent.gameObject.SetActive(value: false);
				}
				num++;
			}
			skipRelicButton.gameObject.SetActive(value: true);
		}

		public void OfferSpecificRelicNonSkippable(Relic relic)
		{
			mainText.gameObject.SetActive(value: false);
			OfferSpecificRelic(relic);
			skipRelicButton.gameObject.SetActive(value: false);
		}

		public void ShowRelicDetails(GameObject clickedButton, Relic relic)
		{
			upgradeConfirmationRelic.gameObject.SetActive(value: true);
			upgradeConfirmationRelic.Populate(relic, 0);
			upgradeConfirmationRelic.GetComponent<SubMenuControllerConnection>().setActiveOnReturn = clickedButton;
			_stagedRelic = relic;
		}

		public IEnumerator SetSelectedButton(GameObject button)
		{
			yield return null;
			_eventSystem.SetSelectedGameObject(button);
		}

		private void HandleUpgradeOptionClicked(UpgradeOption.UpgradeType type, GameObject clickedButton = null, GameObject chosen = null)
		{
			if (!PauseMenu.Paused && !InventoryViewController.isOpen)
			{
				switch (type)
				{
				case UpgradeOption.UpgradeType.INSPECT_RELIC:
					RelicIcon.HideTooltip?.Invoke();
					ShowRelicDetails(clickedButton, clickedButton.GetComponentInChildren<RelicIcon>().relic);
					break;
				case UpgradeOption.UpgradeType.ACCEPT_RELIC:
					RelicIcon.HideTooltip?.Invoke();
					UIButtonClickEventDispatcher.OnUIButtonClicked?.Invoke(ButtonSfx.AddRelic);
					upgradeConfirmationRelic.gameObject.SetActive(value: false);
					relicManager.AddRelic(_stagedRelic);
					EndInteraction();
					break;
				case UpgradeOption.UpgradeType.SKIP:
					EndInteraction();
					break;
				case UpgradeOption.UpgradeType.REMOVE_ORB_CONFIRMED:
				case UpgradeOption.UpgradeType.DUPLICATE_ORB_CONFIRMED:
					break;
				}
			}
		}

		private void EndInteraction()
		{
			OnInteractionComplete?.Invoke();
			base.gameObject.SetActive(value: false);
			mainText.SetActive(value: true);
		}
	}
}
