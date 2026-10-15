using System;
using Battle.Attacks;
using Relics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PeglinUI.LoadoutManager
{
	public class LoadoutEditorShowTooltip : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		private LoadoutOrbData _orbData;

		private Relic _relic;

		private Attack _attack;

		private bool _isRelic;

		private Vector2 _anchorPoint = new Vector2(1f, -1f);

		public void Initialize(LoadoutOrbData orbData)
		{
			_orbData = orbData;
			_attack = orbData.orbPrefab.GetComponent<Attack>();
			LoadoutOrbData orbData2 = _orbData;
			orbData2.OnLevelChanged = (LoadoutOrbData.AmountChanged)Delegate.Combine(orbData2.OnLevelChanged, new LoadoutOrbData.AmountChanged(OnLevelChanged));
		}

		public void Initialize(Relic relic)
		{
			_isRelic = true;
			_relic = relic;
		}

		private void OnLevelChanged(int amt)
		{
			Attack component = _orbData.orbPrefab.GetComponent<Attack>();
			if (component.Level > _orbData.Level)
			{
				while (component.Level > _orbData.Level)
				{
					component = component.PreviousLevelPrefab.GetComponent<Attack>();
				}
			}
			else
			{
				while (component.Level < _orbData.Level)
				{
					component = component.NextLevelPrefab.GetComponent<Attack>();
				}
			}
			_attack = component;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			ShowTooltip();
		}

		public void ShowTooltip()
		{
			if (_isRelic)
			{
				TooltipManager.Instance.ShowTooltipRelic(_relic, base.transform.position, _anchorPoint, followMousePos: true);
			}
			else
			{
				TooltipManager.Instance.ShowTooltipOrb(_attack, base.transform.position, _anchorPoint, followMousePos: true);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			TooltipManager.Instance.HideTooltip();
		}

		private void OnDisable()
		{
			TooltipManager.Instance.HideTooltip();
		}

		private void OnDestroy()
		{
			if (!_isRelic)
			{
				LoadoutOrbData orbData = _orbData;
				orbData.OnLevelChanged = (LoadoutOrbData.AmountChanged)Delegate.Remove(orbData.OnLevelChanged, new LoadoutOrbData.AmountChanged(OnLevelChanged));
			}
		}
	}
}
