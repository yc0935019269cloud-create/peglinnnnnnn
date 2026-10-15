using UnityEngine;

namespace PixelCrushers.DialogueSystem
{
	public class BarkHistory
	{
		public BarkOrder order;

		public int index;

		public BarkHistory(BarkOrder order)
		{
			this.order = order;
			index = 0;
		}

		public int GetNextIndex(int numEntries)
		{
			if (order == BarkOrder.Random)
			{
				return Random.Range(0, numEntries);
			}
			int result = index % numEntries;
			index = (index + 1) % numEntries;
			return result;
		}

		public void Reset()
		{
			index = 0;
		}
	}
}
