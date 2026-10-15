using System;
using Loading;
using Map;
using Scenarios;
using UnityEngine;

namespace RNG.Scenarios
{
	public class ScenarioNavigationBonusChest : MonoBehaviour
	{
		public static bool StoreRobbed;

		[SerializeField]
		public AudioClip _bonusChestSound;

		private AudioSource _audioSource;

		private int _bombsRemaining;

		private void Awake()
		{
			_audioSource = GetComponent<AudioSource>();
		}

		private void OnEnable()
		{
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Combine(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegDestruction));
		}

		private void OnDisable()
		{
			Peg.OnPegActivated = (Peg.PegHitEvent)Delegate.Remove(Peg.OnPegActivated, new Peg.PegHitEvent(HandlePegDestruction));
		}

		public void SetNumberOfBombs(int bombs)
		{
			_bombsRemaining = bombs;
		}

		private void HandlePegDestruction(Peg.PegType type, Peg peg)
		{
			if (type != Peg.PegType.BOMB)
			{
				return;
			}
			_bombsRemaining--;
			if (_bombsRemaining == 0)
			{
				if (_audioSource == null)
				{
					_audioSource = GetComponent<AudioSource>();
				}
				_audioSource.PlayOneShot(_bonusChestSound);
				StoreRobbed = true;
				ChestScenarioController.isBonusChest = true;
				ChestScenarioController.rareChestRequested = true;
				GameObject.FindWithTag("MapController").GetComponent<MapController>().AssignTreasureDataToStatic();
				PachinkoBall[] array = UnityEngine.Object.FindObjectsOfType<PachinkoBall>();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].gameObject.SetActive(value: false);
				}
				UnityEngine.Object.FindObjectOfType<NavOnlyController>().FadeAndLoad(PeglinSceneLoader.Scene.TREASURE, _bonusChestSound.length + 0.1f);
			}
		}
	}
}
