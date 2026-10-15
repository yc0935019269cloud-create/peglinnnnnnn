namespace PixelCrushers.DialogueSystem
{
	public static class CharacterTypeUtility
	{
		public static CharacterType OtherType(CharacterType characterType)
		{
			if (characterType != 0)
			{
				return CharacterType.PC;
			}
			return CharacterType.NPC;
		}
	}
}
