using Relics;
using UnityEngine;

public class AcceptRelicOnClick : MonoBehaviour
{
	public RelicManager relicManager;

	public void AddRelic()
	{
		relicManager.AddRelic(GetComponent<RelicIcon>().relic);
	}
}
