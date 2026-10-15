using System;
using UnityEngine;

namespace PeglinUtils
{
	public static class SeedUtils
	{
		public const int NUMBER_OF_SEED_SYMBOLS = 9;

		public const int NUMBER_OF_DIGITS_IN_SEED = 10;

		private static int maxSeedValue;

		public static string CreateNewRandomSeed(SecretSeedManager secretSeedManager)
		{
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(int.Parse(((int)DateTime.Now.Ticks).ToString()));
			string text = "";
			for (int i = 0; i < 10; i++)
			{
				text += UnityEngine.Random.Range(0, 9);
			}
			while (secretSeedManager.IsSecretSeed(text))
			{
				text = "";
				for (int j = 0; j < 10; j++)
				{
					text += UnityEngine.Random.Range(0, 9);
				}
			}
			UnityEngine.Random.state = state;
			return text;
		}

		public static int ConvertSeedFromSymbolsToInt(string symbolSeed)
		{
			double num = 0.0;
			for (int num2 = symbolSeed.Length - 1; num2 >= 0; num2--)
			{
				double num3 = Math.Pow(9.0, num2);
				int num4 = int.Parse(symbolSeed[num2].ToString());
				num += (double)num4 * num3;
			}
			if (maxSeedValue == 0)
			{
				SetMaxSeedValue();
			}
			return (int)(num - (double)maxSeedValue);
		}

		private static void SetMaxSeedValue()
		{
			maxSeedValue = (int)(((long)Math.Pow(9.0, 10.0) - 1) / 2);
		}

		public static bool IsStringAValidSeed(string symbolSeed)
		{
			if (symbolSeed.Length != 10)
			{
				return false;
			}
			for (int i = 0; i < symbolSeed.Length; i++)
			{
				if (!int.TryParse(symbolSeed[i].ToString(), out var result))
				{
					return false;
				}
				if (result > 9 || result < 1)
				{
					return false;
				}
			}
			return true;
		}
	}
}
