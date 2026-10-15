using System;
using System.Collections.Generic;

namespace PixelCrushers
{
	public static class ListExtensions
	{
		public static void AddSorted<T>(this List<T> @this, T item) where T : IComparable<T>
		{
			if (@this.Count == 0)
			{
				@this.Add(item);
				return;
			}
			if (@this[@this.Count - 1].CompareTo(item) <= 0)
			{
				@this.Add(item);
				return;
			}
			if (@this[0].CompareTo(item) >= 0)
			{
				@this.Insert(0, item);
				return;
			}
			int num = @this.BinarySearch(item);
			if (num < 0)
			{
				num = ~num;
			}
			@this.Insert(num, item);
		}
	}
}
