using System.Collections.Generic;
using System.Text.RegularExpressions;
using I2.Loc;
using MoreMountains.Tools;
using UnityEngine;

namespace PeglinUI.OrbDisplay
{
	public class TooltipKeywordDescriptions : MonoBehaviour
	{
		[SerializeField]
		private GameObject _keywordDescriptionPrefab;

		[SerializeField]
		private GameObject _keywordDescriptionPrefabScreenSpace;

		[SerializeField]
		private KeywordDescriptionPair[] _keywordDescriptionMapping;

		private string keywordRegexPattern = "\\<style=(\\w+)";

		private string spriteRegexPattern = "\\<sprite name=\"(\\w+)\"";

		public int UpdateString(string englishDesc)
		{
			base.transform.MMDestroyAllChildren();
			Regex regex = new Regex(keywordRegexPattern, RegexOptions.IgnoreCase);
			List<string> list = new List<string>();
			Match match = regex.Match(englishDesc);
			while (match.Success)
			{
				Group obj = match.Groups[1];
				KeywordDescriptionPair[] keywordDescriptionMapping = _keywordDescriptionMapping;
				for (int i = 0; i < keywordDescriptionMapping.Length; i++)
				{
					KeywordDescriptionPair keywordDescriptionPair = keywordDescriptionMapping[i];
					if (obj.Captures[0].ToString() == keywordDescriptionPair.Keyword && !list.Contains(keywordDescriptionPair.DescriptionLoc))
					{
						list.Add(keywordDescriptionPair.DescriptionLoc);
					}
				}
				match = match.NextMatch();
			}
			match = new Regex(spriteRegexPattern, RegexOptions.IgnoreCase).Match(englishDesc);
			while (match.Success)
			{
				Group obj2 = match.Groups[1];
				KeywordDescriptionPair[] keywordDescriptionMapping = _keywordDescriptionMapping;
				for (int i = 0; i < keywordDescriptionMapping.Length; i++)
				{
					KeywordDescriptionPair keywordDescriptionPair2 = keywordDescriptionMapping[i];
					if (obj2.Captures[0].ToString() == keywordDescriptionPair2.Keyword && !list.Contains(keywordDescriptionPair2.DescriptionLoc))
					{
						list.Add(keywordDescriptionPair2.DescriptionLoc);
					}
				}
				match = match.NextMatch();
			}
			bool flag = GetComponentInParent<Canvas>().renderMode != RenderMode.WorldSpace;
			foreach (string item in list)
			{
				(flag ? Object.Instantiate(_keywordDescriptionPrefabScreenSpace, base.transform) : Object.Instantiate(_keywordDescriptionPrefab, base.transform)).GetComponentInChildren<Localize>().Term = "Statuses/" + item;
			}
			return list.Count;
		}
	}
}
