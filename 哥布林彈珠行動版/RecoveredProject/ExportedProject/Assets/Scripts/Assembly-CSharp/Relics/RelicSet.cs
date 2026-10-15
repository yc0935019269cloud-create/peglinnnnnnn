using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace Relics
{
	[CreateAssetMenu(fileName = "NewRelicSet", menuName = "Relics/RelicSet")]
	public class RelicSet : ScriptableObject
	{
		[ReorderableList]
		[SerializeField]
		private List<Relic> _relics;

		public List<Relic> relics => _relics;

		private void OnValidate()
		{
			if (_relics.Count() != _relics.Distinct().Count())
			{
				Debug.LogWarning("Relic Set " + base.name + " contains duplicate items.");
			}
		}

		[Button(null, EButtonEnableMode.Always)]
		private void ClearList()
		{
			_relics.Clear();
		}
	}
}
