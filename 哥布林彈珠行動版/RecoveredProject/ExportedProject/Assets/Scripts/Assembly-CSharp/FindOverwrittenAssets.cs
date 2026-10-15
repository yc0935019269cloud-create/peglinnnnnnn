using System.Collections.Generic;
using UnityEngine;

public class FindOverwrittenAssets : MonoBehaviour
{
	private void Start()
	{
		new Dictionary<string, string>
		{
			{ "EntryEncounter", "148116e3b21ec604a913250e38a98868" },
			{ "Everything", "d59cd5d045c7cac40bce5360f4fe6a44" },
			{ "MuchoSlimeEncounter", "488d10627de768744bffb7dd27bcfc99" },
			{ "MuchoSlimeEncounterHARD", "c60a36986ea7e74439baf9c0e6b612cf" },
			{ "PlantEncounter1", "c66bc8456999cf042a3699297bf2f3de" },
			{ "PlantEncounter2", "cfefaf4a6d254fb48817492510661ac2" },
			{ "SlimeEncounter2", "cc04ee1970f90d441b98608595274386" },
			{ "SlimeEncounter3", "d519a24d2ee3fff44a0358863f17e0ff" },
			{ "SlimeEncounter4", "2d833e48c50a8f944bcfe0b596ce0f73" },
			{ "SlimeFinalBoss", "37afe0ba98504c04ba8c3f2ae36a17ff" },
			{ "SlimeMiniBossEncounter_HARD", "2d833e48c50a8f944bcfe0b596ce0f73" }
		};
	}

	private void Update()
	{
	}
}
