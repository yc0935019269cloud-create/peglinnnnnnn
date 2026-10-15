using System.Collections;
using Challenges;
using PeglinUI;
using UnityEngine;

public class SkipWinSceneIfCoreSkipped : MonoBehaviour
{
	public FadeCurtain fadeCurtain;

	public GameObject menuButton;

	public SpriteRenderer blackoutPanel;

	private void Awake()
	{
		blackoutPanel.enabled = false;
		if (ChallengeManager.Instance != null && ChallengeManager.Instance.chosenToSkipCore)
		{
			blackoutPanel.enabled = true;
			menuButton.SetActive(value: false);
			StartCoroutine(WaitAndLoadRunSummary());
		}
	}

	private IEnumerator WaitAndLoadRunSummary()
	{
		yield return null;
		fadeCurtain.FadeInAndLoadRunSummary();
	}
}
