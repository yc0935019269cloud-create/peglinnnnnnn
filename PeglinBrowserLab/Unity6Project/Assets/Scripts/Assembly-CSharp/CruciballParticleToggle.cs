using PeglinUI.MainMenu.Cruciball;
using UnityEngine;

public class CruciballParticleToggle : MonoBehaviour
{
	private ParticleSystem _cruciballParticleSystem;

	public CruciballLevelSelector CruciballLevelSelector;

	private void OnEnable()
	{
		_cruciballParticleSystem = GetComponent<ParticleSystem>();
	}

	public void EnableSystem()
	{
		if (CruciballLevelSelector.GetCruciballActive())
		{
			_cruciballParticleSystem.Play();
			_cruciballParticleSystem.GetComponent<Renderer>().enabled = true;
		}
	}

	public void DisableSystem()
	{
		if (!(_cruciballParticleSystem == null))
		{
			_cruciballParticleSystem.Stop();
			_cruciballParticleSystem.GetComponent<Renderer>().enabled = false;
		}
	}
}
