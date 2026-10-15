namespace Map
{
	public class SeededTreasureNodeSaveData : SeededMapNodeSaveData
	{
		public float rareRelicChanceRoll;

		public float mimicChallengeChanceRoll;

		public SeededTreasureNodeSaveData(float RareRelicChanceRoll, float mimicChanceRoll)
		{
			rareRelicChanceRoll = RareRelicChanceRoll;
			mimicChallengeChanceRoll = mimicChanceRoll;
		}
	}
}
