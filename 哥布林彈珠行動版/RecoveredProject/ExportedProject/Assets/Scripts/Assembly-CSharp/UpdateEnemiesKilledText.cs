using UnityEngine;
using UnityEngine.UI;

public class UpdateEnemiesKilledText : MonoBehaviour
{
	private void Start()
	{
		GetComponent<Text>().text = "Enemies Killed: " + GameObject.FindGameObjectWithTag("Stats").GetComponent<StatsController>().NumEnemiesKilled;
	}
}
