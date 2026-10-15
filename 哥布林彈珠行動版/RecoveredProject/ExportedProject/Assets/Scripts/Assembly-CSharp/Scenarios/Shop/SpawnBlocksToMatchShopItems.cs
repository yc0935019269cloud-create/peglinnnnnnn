using Battle.PegBehaviour;
using UnityEngine;

namespace Scenarios.Shop
{
	public class SpawnBlocksToMatchShopItems : MonoBehaviour
	{
		[Header("Prefabs")]
		public PegBlock orbPegBlockPrefab;

		public PegBlock relicPegBlockPrefab;

		[Header("References")]
		public GameObject removeOrbRow;

		[Header("Variables")]
		public float pegboardWidth = 16f;

		public float orbBlockYOffset;

		public float relicBlockYOffset = -2f;

		private float removeOrbYPos;

		private void Start()
		{
			removeOrbYPos = removeOrbRow.transform.position.y;
		}

		public void SpawnOrbBlocks(IPurchasableItem[] orbs)
		{
			SpawnBlocks(orbs, orbPegBlockPrefab, orbBlockYOffset, hugWalls: false);
		}

		public void SpawnRelicBlocks(IPurchasableItem[] relics)
		{
			SpawnBlocks(relics, relicPegBlockPrefab, relicBlockYOffset, hugWalls: true);
		}

		private void SpawnBlocks(IPurchasableItem[] items, PegBlock prefab, float yOffset, bool hugWalls)
		{
			int num = items.Length;
			float width = prefab.width;
			float num2;
			float num3;
			if (hugWalls)
			{
				num2 = (pegboardWidth + width) / (float)(num + 1);
				num3 = (pegboardWidth + width) * -0.5f + num2;
			}
			else
			{
				num2 = (pegboardWidth - width) / (float)(num - 1);
				num3 = (pegboardWidth - width) * -0.5f;
			}
			for (int i = 0; i < items.Length; i++)
			{
				if (items[i] != null)
				{
					Object.Instantiate(prefab, base.transform).transform.position = new Vector3(num3, removeOrbYPos + yOffset);
				}
				num3 += num2;
			}
		}

		public void ToggleRemoveOrbRow(bool orbRemoved)
		{
			removeOrbRow.SetActive(!orbRemoved);
		}
	}
}
