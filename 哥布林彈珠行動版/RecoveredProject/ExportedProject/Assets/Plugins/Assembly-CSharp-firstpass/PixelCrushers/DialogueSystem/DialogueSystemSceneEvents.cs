using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class DialogueSystemSceneEvents : MonoBehaviour
	{
		[HelpBox("Do not remove this GameObject. It contains UnityEvents referenced by a dialogue database. This GameObject should not be a child of the Dialogue Manager or marked as Don't Destroy On Load.", HelpBoxMessageType.Info)]
		public List<DialogueEntrySceneEvent> dialogueEntrySceneEvents = new List<DialogueEntrySceneEvent>();

		private static DialogueSystemSceneEvents m_sceneInstance;

		public static DialogueSystemSceneEvents sceneInstance
		{
			get
			{
				if (m_sceneInstance == null)
				{
					m_sceneInstance = UnityEngine.Object.FindObjectOfType<DialogueSystemSceneEvents>();
				}
				return m_sceneInstance;
			}
			set
			{
				m_sceneInstance = value;
			}
		}

		private void Awake()
		{
			m_sceneInstance = this;
		}

		public static int AddNewDialogueEntrySceneEvent(out string guid)
		{
			guid = string.Empty;
			if (sceneInstance == null)
			{
				return -1;
			}
			guid = Guid.NewGuid().ToString();
			DialogueEntrySceneEvent dialogueEntrySceneEvent = new DialogueEntrySceneEvent();
			dialogueEntrySceneEvent.guid = guid;
			sceneInstance.dialogueEntrySceneEvents.Add(dialogueEntrySceneEvent);
			return sceneInstance.dialogueEntrySceneEvents.Count - 1;
		}

		public static void RemoveDialogueEntrySceneEvent(string guid)
		{
			if (!Application.isPlaying && !(sceneInstance == null))
			{
				sceneInstance.dialogueEntrySceneEvents.RemoveAll((DialogueEntrySceneEvent x) => x.guid == guid);
			}
		}

		public static DialogueEntrySceneEvent GetDialogueEntrySceneEvent(string guid)
		{
			if (sceneInstance == null)
			{
				return null;
			}
			return sceneInstance.dialogueEntrySceneEvents.Find((DialogueEntrySceneEvent x) => x.guid == guid);
		}

		public static int GetDialogueEntrySceneEventIndex(string guid)
		{
			if (sceneInstance == null)
			{
				return -1;
			}
			return sceneInstance.dialogueEntrySceneEvents.FindIndex((DialogueEntrySceneEvent x) => x.guid == guid);
		}
	}
}
