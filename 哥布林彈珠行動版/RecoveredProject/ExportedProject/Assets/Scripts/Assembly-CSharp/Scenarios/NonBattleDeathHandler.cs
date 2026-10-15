using System;
using UnityEngine;

namespace Scenarios
{
	public class NonBattleDeathHandler : MonoBehaviour
	{
		public FloatVariable _playerHealth;

		public GameObject gameOverObject;

		public static Action NonBattleDeath = () =>
		{
		};

		private static readonly int Dead = Animator.StringToHash("dead");

		private void OnEnable()
		{
			FloatVariable playerHealth = _playerHealth;
			playerHealth.onValueChanged = (FloatVariable.ValueChanged)Delegate.Combine(playerHealth.onValueChanged, new FloatVariable.ValueChanged(CheckForDeath));
		}

		private void OnDisable()
		{
			FloatVariable playerHealth = _playerHealth;
			playerHealth.onValueChanged = (FloatVariable.ValueChanged)Delegate.Remove(playerHealth.onValueChanged, new FloatVariable.ValueChanged(CheckForDeath));
		}

		private void CheckForDeath(float newVal, float oldVal)
		{
			if (!(newVal <= 0f))
			{
				return;
			}
			GameObject gameObject = GameObject.FindWithTag("Player");
			if (gameObject != null)
			{
				gameObject.GetComponent<Animator>().SetBool(Dead, value: true);
			}
			PachinkoBall[] array = UnityEngine.Object.FindObjectsOfType<PachinkoBall>();
			if (array != null && array.Length != 0)
			{
				PachinkoBall[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					UnityEngine.Object.Destroy(array2[i].gameObject);
				}
			}
			SaveManager.RequestDelete();
			NonBattleDeath?.Invoke();
			gameOverObject.SetActive(value: true);
		}
	}
}
