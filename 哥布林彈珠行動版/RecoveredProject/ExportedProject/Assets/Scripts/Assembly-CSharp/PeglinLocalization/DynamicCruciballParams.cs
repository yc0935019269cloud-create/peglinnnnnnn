using System;
using System.Globalization;
using Cruciball;
using I2.Loc;
using UnityEngine;

namespace PeglinLocalization
{
	[RequireComponent(typeof(LocalizationParamsManager))]
	public class DynamicCruciballParams : MonoBehaviour
	{
		public CruciballManager cruciballManager;

		private LocalizationParamsManager _paramsMan;

		private void Awake()
		{
			_paramsMan = GetComponent<LocalizationParamsManager>();
		}

		private void OnEnable()
		{
			CruciballManager.CruciballLevelChanged = (Action)Delegate.Combine(CruciballManager.CruciballLevelChanged, new Action(UpdateParameters));
		}

		private void OnDisable()
		{
			CruciballManager.CruciballLevelChanged = (Action)Delegate.Remove(CruciballManager.CruciballLevelChanged, new Action(UpdateParameters));
		}

		private void Start()
		{
			UpdateParameters();
		}

		public void UpdateParameters()
		{
			if (cruciballManager.DecreasedBombDamage())
			{
				_paramsMan.SetParameterValue("CRUC_BOMB_DMG", 40.ToString(CultureInfo.InvariantCulture));
				_paramsMan.SetParameterValue("CRUC_RIGGED_DMG", 150.ToString(CultureInfo.InvariantCulture));
			}
			else if (!cruciballManager.DecreasedBombDamage())
			{
				_paramsMan.SetParameterValue("CRUC_BOMB_DMG", 50.ToString(CultureInfo.InvariantCulture));
				_paramsMan.SetParameterValue("CRUC_RIGGED_DMG", 200.ToString(CultureInfo.InvariantCulture));
			}
			_paramsMan.SetParameterValue("CRUC_RIGGED_SELF_DMG", (cruciballManager.IncreasedRiggedDamage() ? 5 : 3).ToString(CultureInfo.InvariantCulture));
		}
	}
}
