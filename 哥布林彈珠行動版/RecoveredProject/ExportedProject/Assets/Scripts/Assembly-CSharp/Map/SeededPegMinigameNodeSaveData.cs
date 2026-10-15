namespace Map
{
	public class SeededPegMinigameNodeSaveData : SeededMapNodeSaveData
	{
		public float pegMinigameChoiceRoll;

		public string[] orbRewardsNamesSaveData;

		public int[] orbRewardsRaritiesSaveData;

		public int[] relicRewardsSaveData;

		public float mimicChallengeChanceRoll;

		public SeededPegMinigameNodeSaveData(float PegMinigameChoiceRoll, string[] OrbRewardsNamesSaveData, int[] OrbRewardsRaritiesSaveData, float MimicChallengeChanceRoll)
		{
			pegMinigameChoiceRoll = PegMinigameChoiceRoll;
			orbRewardsNamesSaveData = OrbRewardsNamesSaveData;
			orbRewardsRaritiesSaveData = OrbRewardsRaritiesSaveData;
			mimicChallengeChanceRoll = MimicChallengeChanceRoll;
		}
	}
}
