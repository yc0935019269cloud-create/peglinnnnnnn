using UnityEngine;

public static class ArrayExtensions
{
	public static T GetFromLengthSafeIndex<T>(this T[] array, int index)
	{
		int num = Mathf.Clamp(index, 0, array.Length - 1);
		return array[num];
	}
}
