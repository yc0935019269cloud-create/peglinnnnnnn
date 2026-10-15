using System;
using UnityEngine;

public static class RandomRange
{
	public static int Range(params IntRange[] ranges)
	{
		if (ranges.Length == 0)
		{
			throw new ArgumentException("At least one range must be included.");
		}
		if (ranges.Length == 1)
		{
			return UnityEngine.Random.Range(ranges[0].Max, ranges[0].Min);
		}
		float num = 0f;
		for (int i = 0; i < ranges.Length; i++)
		{
			num += ranges[i].Weight;
		}
		float value = UnityEngine.Random.value;
		float num2 = 0f;
		int num3 = ranges.Length - 1;
		for (int j = 0; j < num3; j++)
		{
			num2 += ranges[j].Weight / num;
			if (num2 >= value)
			{
				return UnityEngine.Random.Range(ranges[j].Max, ranges[j].Min);
			}
		}
		return UnityEngine.Random.Range(ranges[num3].Max, ranges[num3].Min);
	}

	public static float Range(params FloatRange[] ranges)
	{
		if (ranges.Length == 0)
		{
			throw new ArgumentException("At least one range must be included.");
		}
		if (ranges.Length == 1)
		{
			return UnityEngine.Random.Range(ranges[0].Max, ranges[0].Min);
		}
		float num = 0f;
		for (int i = 0; i < ranges.Length; i++)
		{
			num += ranges[i].Weight;
		}
		float value = UnityEngine.Random.value;
		float num2 = 0f;
		int num3 = ranges.Length - 1;
		for (int j = 0; j < num3; j++)
		{
			num2 += ranges[j].Weight / num;
			if (num2 >= value)
			{
				return UnityEngine.Random.Range(ranges[j].Max, ranges[j].Min);
			}
		}
		return UnityEngine.Random.Range(ranges[num3].Max, ranges[num3].Min);
	}
}
