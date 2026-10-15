namespace Map
{
	public class SeededTextScenarioNodeSaveData : SeededMapNodeSaveData
	{
		public SerializableRandomState randomState;

		public SeededUnknownNodeSaveData unknownNodeSaveData;

		public string textScenarioNameSaveData;

		public float mimicChallengeChanceRoll;

		public bool generateUnknownData = true;

		public SeededTextScenarioNodeSaveData(SerializableRandomState RandomState, SeededUnknownNodeSaveData UnknownNodeData, string TextScenarioNameSaveData, float MimicChallengeChanceRoll, bool GenerateUnknownData)
		{
			randomState = RandomState;
			unknownNodeSaveData = UnknownNodeData;
			textScenarioNameSaveData = TextScenarioNameSaveData;
			mimicChallengeChanceRoll = MimicChallengeChanceRoll;
			generateUnknownData = GenerateUnknownData;
		}
	}
}
