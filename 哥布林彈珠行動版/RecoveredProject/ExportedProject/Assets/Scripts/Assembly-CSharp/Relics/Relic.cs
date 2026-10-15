using System;
using AssetIcons;
using NaughtyAttributes;
using PeglinUI;
using UnityEngine;

namespace Relics
{
	[CreateAssetMenu(fileName = "NewRelic", menuName = "Relics/Relic")]
	public class Relic : ScriptableObject, IEquatable<Relic>
	{
		public string locKey;

		[Tooltip("Used in case we need an alternate description. For example super_boots_desc2 since we're tweaking it so frequently")]
		public string descMod = "";

		[ReadOnly]
		public string englishDisplayName;

		[ReadOnly]
		public string englishDescription;

		[ReadOnly]
		public string englishLockedDescription;

		public Tooltip.LockedDescriptionPresets lockedDescriptionPreset;

		[AssetIcon("100%", "100%", "0", "0", 64, IconAnchor.Center, IconAspect.Fit, "true", "#ffffff", 0, FontStyle.Normal, IconAnchor.Center, IconProjection.Perspective, -1, null)]
		public Sprite sprite;

		public AudioClip useSfx;

		public RelicEffect effect;

		public RelicRarity globalRarity;

		public string nameKey => "Relics/" + locKey + "_name";

		public string descKey => "Relics/" + locKey + "_desc" + descMod;

		public string lockedInfoKey => "Relics/" + locKey + "_desc" + descMod + "_locked";

		public override bool Equals(object obj)
		{
			return Equals(obj as Relic);
		}

		public bool Equals(Relic other)
		{
			if (other != null && base.Equals((object)other))
			{
				return effect == other.effect;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (unchecked(-1818206230 * -1521134295) + base.GetHashCode()) * -1521134295 + effect.GetHashCode();
		}
	}
}
