using System;
using Battle.Enemies;
using UnityEngine;

public class CoreBackgroundController : MonoBehaviour
{
	public GameObject backgroundCrystals;

	public GameObject shadowParticlesParent;

	public GameObject blobsParent;

	private void OnEnable()
	{
		SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Combine(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(OnPhase2Transition));
	}

	private void OnDisable()
	{
		SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Remove(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(OnPhase2Transition));
	}

	private void OnPhase2Transition()
	{
		backgroundCrystals.SetActive(value: false);
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.SetActive(value: false);
		}
		blobsParent.SetActive(value: true);
	}
}
