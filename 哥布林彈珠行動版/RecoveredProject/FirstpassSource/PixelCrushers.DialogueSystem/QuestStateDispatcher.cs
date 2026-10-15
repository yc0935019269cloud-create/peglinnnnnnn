using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class QuestStateDispatcher : MonoBehaviour
	{
		private List<QuestStateListener> m_listeners = new List<QuestStateListener>();

		public void AddListener(QuestStateListener listener)
		{
			if (!(listener == null))
			{
				m_listeners.Add(listener);
			}
		}

		public void RemoveListener(QuestStateListener listener)
		{
			m_listeners.Remove(listener);
		}

		public void OnQuestStateChange(string questName)
		{
			for (int i = 0; i < m_listeners.Count; i++)
			{
				QuestStateListener questStateListener = m_listeners[i];
				if (string.Equals(questName, questStateListener.questName))
				{
					questStateListener.OnChange();
				}
			}
		}
	}
}
