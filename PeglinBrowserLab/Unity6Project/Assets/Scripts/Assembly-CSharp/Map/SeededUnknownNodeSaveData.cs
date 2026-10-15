namespace Map
{
	public class SeededUnknownNodeSaveData : SeededMapNodeSaveData
	{
		public float typeRoll;

		public float mimicChallengeChanceRoll;

		public SeededTreasureNodeSaveData treasureData;

		public SeededTextScenarioNodeSaveData textScenerioData;

		public SeededShopNodeSaveData shopData;

		public SeededBattleNodeSaveData battleData;

		public SeededBattleNodeSaveData minibossData;

		public SeededPegMinigameNodeSaveData pegMinigameData;

		public bool generateTextData = true;

		public SeededUnknownNodeSaveData(float TypeRoll, float MimicChallengeChanceRoll, SeededTreasureNodeSaveData TreasureData, SeededTextScenarioNodeSaveData TextScenerioData, SeededShopNodeSaveData ShopData, SeededBattleNodeSaveData BattleData, SeededBattleNodeSaveData MinibossData, SeededPegMinigameNodeSaveData PegMinigameData, bool GenerateTextData)
		{
			typeRoll = TypeRoll;
			mimicChallengeChanceRoll = MimicChallengeChanceRoll;
			treasureData = TreasureData;
			textScenerioData = TextScenerioData;
			shopData = ShopData;
			battleData = BattleData;
			minibossData = MinibossData;
			pegMinigameData = PegMinigameData;
			generateTextData = GenerateTextData;
		}
	}
}
