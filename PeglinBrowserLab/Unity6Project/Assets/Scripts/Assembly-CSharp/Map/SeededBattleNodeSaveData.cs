namespace Map
{
	public class SeededBattleNodeSaveData : SeededMapNodeSaveData
	{
		public float rainbowSlimeRoll;

		public float[] orbUpgradeRolls;

		public string easyBattleSaveData;

		public string regularBattleSaveData;

		public string minibossBattleSaveData;

		public string[] postBattleOrbNamesSaveData;

		public int[] postBattleOrbRaritiesSaveData;

		public float[] c19ExtraEnemyRolls;

		public float randomNavRoll;

		public SerializableRandomState deckOrderRandomState;

		public SerializableRandomState specialPegShufflingRandomState;

		public SerializableRandomState[] bossMechanicsRandomStatesPerAct;

		public SeededBattleNodeSaveData(float RainbowSlimeRoll, float[] OrbUpgradeRolls, string EasyBattleSaveData, string RegularBattleSaveData, string MinibossBattleSaveData, string[] PostBattleOrbNamesSaveData, int[] PostBattleOrbRaritiesSaveData, float[] C19ExtraEnemyRolls, float RandomNavRoll, SerializableRandomState DeckOrderRandomState, SerializableRandomState SpecialPegShufflingRandomState, SerializableRandomState[] BossMechanicsRandomStatesPerAct)
		{
			rainbowSlimeRoll = RainbowSlimeRoll;
			orbUpgradeRolls = OrbUpgradeRolls;
			easyBattleSaveData = EasyBattleSaveData;
			regularBattleSaveData = RegularBattleSaveData;
			minibossBattleSaveData = MinibossBattleSaveData;
			postBattleOrbNamesSaveData = PostBattleOrbNamesSaveData;
			postBattleOrbRaritiesSaveData = PostBattleOrbRaritiesSaveData;
			c19ExtraEnemyRolls = C19ExtraEnemyRolls;
			randomNavRoll = RandomNavRoll;
			deckOrderRandomState = DeckOrderRandomState;
			specialPegShufflingRandomState = SpecialPegShufflingRandomState;
			bossMechanicsRandomStatesPerAct = BossMechanicsRandomStatesPerAct;
		}
	}
}
