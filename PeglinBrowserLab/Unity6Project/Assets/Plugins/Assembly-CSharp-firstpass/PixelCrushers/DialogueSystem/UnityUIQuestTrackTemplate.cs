using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class UnityUIQuestTrackTemplate : MonoBehaviour
	{
		[Header("Quest Heading")]
		[Tooltip("The heading - name or description depends on tracker setting")]
		public Text description;

		public UnityUIQuestTemplateAlternateDescriptions alternateDescriptions = new UnityUIQuestTemplateAlternateDescriptions();

		[Header("Quest Entries")]
		[Tooltip("(Optional) If set, holds instantiated quest entries")]
		public Transform entryContainer;

		[Tooltip("Used for quest entries")]
		public Text entryDescription;

		public UnityUIQuestTemplateAlternateDescriptions alternateEntryDescriptions = new UnityUIQuestTemplateAlternateDescriptions();

		private List<Text> instances;

		private int numEntries;

		public bool ArePropertiesAssigned
		{
			get
			{
				if (description != null)
				{
					return entryDescription != null;
				}
				return false;
			}
		}

		public void Initialize()
		{
			if (description != null)
			{
				description.gameObject.SetActive(value: false);
			}
			alternateDescriptions.SetActive(value: false);
			if (entryDescription != null)
			{
				entryDescription.gameObject.SetActive(value: false);
			}
			alternateEntryDescriptions.SetActive(value: false);
			if (entryContainer != null)
			{
				entryContainer.gameObject.SetActive(value: false);
				if (instances != null)
				{
					for (int i = 0; i < instances.Count; i++)
					{
						if (instances[i] != null)
						{
							Object.Destroy(instances[i].gameObject);
						}
					}
				}
				instances = new List<Text>();
			}
			numEntries = 0;
		}

		public void SetDescription(string text, QuestState questState)
		{
			if (text != null)
			{
				switch (questState)
				{
				case QuestState.Active:
					SetFirstValidTextElement(text, description);
					break;
				case QuestState.Success:
					SetFirstValidTextElement(text, alternateDescriptions.successDescription, description);
					break;
				case QuestState.Failure:
					SetFirstValidTextElement(text, alternateDescriptions.failureDescription, description);
					break;
				}
			}
		}

		private void SetFirstValidTextElement(string text, params Text[] textElements)
		{
			for (int i = 0; i < textElements.Length; i++)
			{
				if (textElements[i] != null)
				{
					textElements[i].gameObject.SetActive(value: true);
					textElements[i].text = text;
					break;
				}
			}
		}

		public void AddEntryDescription(string text, QuestState entryState)
		{
			if (entryContainer == null)
			{
				alternateEntryDescriptions.SetActive(value: false);
				if (entryDescription != null)
				{
					if (numEntries == 0)
					{
						entryDescription.gameObject.SetActive(value: true);
						entryDescription.text = text;
					}
					else
					{
						Text text2 = entryDescription;
						text2.text = text2.text + "\n" + text;
					}
				}
			}
			else
			{
				if (numEntries == 0)
				{
					entryContainer.gameObject.SetActive(value: true);
					if (entryDescription != null)
					{
						entryDescription.gameObject.SetActive(value: false);
					}
					alternateEntryDescriptions.SetActive(value: false);
				}
				switch (entryState)
				{
				case QuestState.Active:
					InstantiateFirstValidTextElement(text, entryContainer, entryDescription);
					break;
				case QuestState.Success:
					InstantiateFirstValidTextElement(text, entryContainer, alternateEntryDescriptions.successDescription, entryDescription);
					break;
				case QuestState.Failure:
					InstantiateFirstValidTextElement(text, entryContainer, alternateEntryDescriptions.failureDescription, entryDescription);
					break;
				}
			}
			numEntries++;
		}

		private void InstantiateFirstValidTextElement(string text, Transform container, params Text[] textElements)
		{
			for (int i = 0; i < textElements.Length; i++)
			{
				if (textElements[i] != null)
				{
					GameObject obj = Object.Instantiate(textElements[i].gameObject);
					obj.transform.SetParent(container.transform, worldPositionStays: false);
					obj.SetActive(value: true);
					Text component = obj.GetComponent<Text>();
					if (component != null)
					{
						component.text = text;
					}
					instances.Add(component);
					break;
				}
			}
		}
	}
}
