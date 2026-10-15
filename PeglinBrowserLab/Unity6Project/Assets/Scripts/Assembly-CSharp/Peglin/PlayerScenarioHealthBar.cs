using System;
using Scenarios;
using TMPro;
using UnityEngine;

namespace Peglin
{
	public class PlayerScenarioHealthBar : MonoBehaviour
	{
		public FloatVariable playerHealth;

		public FloatVariable playerMaxHealth;

		public FloatVariable reductionToPlayerMaxHealth;

		public TMP_Text healthText;

		private UpdateSlider _updateSlider;

		public DialogueScriptableObjectInteractions _soInteractions;

		private void OnEnable()
		{
			FloatVariable floatVariable = playerHealth;
			floatVariable.onValueChanged = (FloatVariable.ValueChanged)Delegate.Combine(floatVariable.onValueChanged, new FloatVariable.ValueChanged(UpdateHealth));
			FloatVariable floatVariable2 = playerMaxHealth;
			floatVariable2.onValueChanged = (FloatVariable.ValueChanged)Delegate.Combine(floatVariable2.onValueChanged, new FloatVariable.ValueChanged(UpdateHealth));
			FloatVariable floatVariable3 = reductionToPlayerMaxHealth;
			floatVariable3.onValueChanged = (FloatVariable.ValueChanged)Delegate.Combine(floatVariable3.onValueChanged, new FloatVariable.ValueChanged(UpdateHealth));
		}

		private void Start()
		{
			_updateSlider = GetComponent<UpdateSlider>();
			_updateSlider.UpdateSize(playerHealth.Value, playerMaxHealth.Value, reductionToPlayerMaxHealth.Value);
			healthText.text = playerHealth.Value + "/" + playerMaxHealth.Value;
		}

		private void OnDisable()
		{
			FloatVariable floatVariable = playerHealth;
			floatVariable.onValueChanged = (FloatVariable.ValueChanged)Delegate.Remove(floatVariable.onValueChanged, new FloatVariable.ValueChanged(UpdateHealth));
			FloatVariable floatVariable2 = playerMaxHealth;
			floatVariable2.onValueChanged = (FloatVariable.ValueChanged)Delegate.Remove(floatVariable2.onValueChanged, new FloatVariable.ValueChanged(UpdateHealth));
			FloatVariable floatVariable3 = reductionToPlayerMaxHealth;
			floatVariable3.onValueChanged = (FloatVariable.ValueChanged)Delegate.Remove(floatVariable3.onValueChanged, new FloatVariable.ValueChanged(UpdateHealth));
		}

		private void UpdateHealth(float newVar, float oldVar)
		{
			if (_updateSlider == null)
			{
				_updateSlider = GetComponent<UpdateSlider>();
			}
			if (playerHealth.Value > playerMaxHealth.Value)
			{
				playerHealth.Set(playerMaxHealth.Value);
			}
			_updateSlider.UpdateSize(playerHealth.Value, playerMaxHealth.Value, reductionToPlayerMaxHealth.Value);
			healthText.text = playerHealth.Value + "/" + playerMaxHealth.Value;
		}
	}
}
