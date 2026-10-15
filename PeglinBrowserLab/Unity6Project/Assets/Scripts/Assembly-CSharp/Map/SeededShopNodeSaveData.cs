namespace Map
{
	public class SeededShopNodeSaveData : SeededMapNodeSaveData
	{
		public float rareRelicChanceRoll;

		public float shopRelicChanceRoll;

		public string[] shopOrbsSaveData;

		public int[] shopOrbsRaritySaveData;

		public SeededShopNodeSaveData(float RareRelicChanceRoll, float ShopRelicChanceRoll, string[] ShopOrbsSaveData, int[] ShopOrbsRaritySaveData)
		{
			rareRelicChanceRoll = RareRelicChanceRoll;
			shopRelicChanceRoll = ShopRelicChanceRoll;
			shopOrbsSaveData = ShopOrbsSaveData;
			shopOrbsRaritySaveData = ShopOrbsRaritySaveData;
		}
	}
}
