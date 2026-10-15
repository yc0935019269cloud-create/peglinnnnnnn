using System;

namespace PixelCrushers.DialogueSystem
{
	[Serializable]
	public class QuestCondition
	{
		public string questName = string.Empty;

		[BitMask(typeof(QuestState))]
		[QuestState]
		public QuestState questState;

		public bool IsTrue
		{
			get
			{
				if (!string.IsNullOrEmpty(questName))
				{
					return QuestLog.IsQuestInStateMask(questName, questState);
				}
				return true;
			}
		}
	}
}
