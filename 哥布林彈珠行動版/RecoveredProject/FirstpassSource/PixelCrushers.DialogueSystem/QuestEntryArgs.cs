namespace PixelCrushers.DialogueSystem
{
	public struct QuestEntryArgs
	{
		public string questName;

		public int entryNumber;

		public QuestEntryArgs(string questName, int entryNumber)
		{
			this.questName = questName;
			this.entryNumber = entryNumber;
		}
	}
}
