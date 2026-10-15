using System.Collections.Generic;
using UnityEngine;

namespace PeglinUtils
{
	public static class ListExtensions
	{
		public static void Shuffle<T>(this IList<T> list)
		{
			int num = list.Count;
			while (num > 1)
			{
				num--;
				int num2 = Random.Range(0, num + 1);
				int index = num2;
				int index2 = num;
				T val = list[num];
				T val2 = list[num2];
				T val3 = (list[index] = val);
				val3 = (list[index2] = val2);
			}
		}

		public static int ConvertPercentageToListIndex(float randomNumber, int numItemsInList)
		{
			int num = Mathf.FloorToInt(randomNumber * (float)numItemsInList);
			if (num == numItemsInList && num > 0)
			{
				num--;
			}
			return num;
		}
	}
}
