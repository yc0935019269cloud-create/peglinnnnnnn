using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class UnityUIQuestTemplate : MonoBehaviour
	{
		[Header("Quest Heading")]
		[Tooltip("The heading - name or description depends on window setting")]
		public Button heading;

		[Tooltip("Used for Description")]
		public Text description;

		public UnityUIQuestTemplateAlternateDescriptions alternateDescriptions = new UnityUIQuestTemplateAlternateDescriptions();

		[Header("Quest Entries")]
		[Tooltip("(Optional) If set, holds instantiated quest entries")]
		public Transform entryContainer;

		[Tooltip("Used for quest entries")]
		public Text entryDescription;

		public UnityUIQuestTemplateAlternateDescriptions alternateEntryDescriptions = new UnityUIQuestTemplateAlternateDescriptions();

		[Header("Buttons")]
		[Tooltip("Used for Track button if quest is trackable")]
		public Button trackButton;

		[Tooltip("Used for Abandon button if quest is abandonable")]
		public Button abandonButton;

		protected List<GameObject> entryInstances = new List<GameObject>();

		protected int numEntries;

		public bool ArePropertiesAssigned
		{
			get
			{
				if (heading != null && description != null && entryDescription != null && trackButton != null)
				{
					return abandonButton != null;
				}
				return false;
			}
		}

		public virtual void Initialize()
		{
			if (description != null)
			{
				description.gameObject.SetActive(value: false);
			}
			if (entryDescription != null)
			{
				entryDescription.gameObject.SetActive(value: false);
			}
			alternateEntryDescriptions.SetActive(value: false);
			if (entryContainer != null)
			{
				entryContainer.gameObject.SetActive(value: false);
			}
		}

		public virtual void ClearQuestDetails()
		{
			if (entryContainer == null)
			{
				if (entryDescription != null)
				{
					entryDescription.text = string.Empty;
				}
			}
			else
			{
				for (int i = 0; i < entryInstances.Count; i++)
				{
					Object.Destroy(entryInstances[i]);
				}
				entryInstances.Clear();
			}
			numEntries = 0;
		}

		public virtual void AddEntryDescription(string text, QuestState entryState)
		{
			if (entryContainer == null)
			{
				if (entryState != QuestState.Unassigned)
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

		protected void InstantiateFirstValidTextElement(string text, Transform container, params Text[] textElements)
		{
			for (int i = 0; i < textElements.Length; i++)
			{
				if (textElements[i] != null)
				{
					GameObject gameObject = Object.Instantiate(textElements[i].gameObject);
					entryInstances.Add(gameObject);
					gameObject.transform.SetParent(container.transform, worldPositionStays: false);
					gameObject.SetActive(value: true);
					Text component = gameObject.GetComponent<Text>();
					if (component != null)
					{
						component.text = text;
					}
					break;
				}
			}
		}
	}
}
