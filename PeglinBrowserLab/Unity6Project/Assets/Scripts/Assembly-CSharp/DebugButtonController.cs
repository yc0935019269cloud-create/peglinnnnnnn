using Battle;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugButtonController : MonoBehaviour
{
	public PauseMenu _pauseMenu;

	public GameObject regularButtons;

	public GameObject debugButtons;

	public GameObject skipBattleButton;

	private int _numClicked;

	public void Awake()
	{
		if (!Debug.isDebugBuild)
		{
			base.enabled = false;
		}
		else
		{
			skipBattleButton.SetActive(SceneManager.GetActiveScene().name == "Battle");
		}
	}

	public void DebugButtonClicked()
	{
		if (Debug.isDebugBuild)
		{
			_numClicked++;
			if (_numClicked >= 3)
			{
				debugButtons.SetActive(_numClicked % 2 != 0);
				regularButtons.SetActive(_numClicked % 2 == 0);
			}
		}
	}

	public void SkipBattle()
	{
		if (Debug.isDebugBuild)
		{
			debugButtons.SetActive(value: false);
			_pauseMenu.Resume();
			GameObject.FindGameObjectWithTag("GameController").GetComponent<BattleController>().TriggerVictory();
			Peg[] array = Object.FindObjectsOfType<Peg>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: false);
			}
		}
	}
}
