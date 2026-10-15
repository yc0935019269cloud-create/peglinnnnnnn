using System;
using System.Collections;
using Battle;
using Battle.Enemies;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpiritOfRadiaPhaseTransitionController : MonoBehaviour
{
	private static readonly int Eaten = Animator.StringToHash("eaten");

	public GameObject peglin;

	public GameObject deckDisplay;

	public GameObject upcomingEnemyDisplay;

	public GameObject playerHealthBar;

	public GameObject orbStatsDisplay;

	public GameObject discardButton;

	public GameObject bossHealthBarCanvas;

	public TextMeshPro phase1CountdownText;

	public Image countdownBackground;

	public GameObject[] fallingParticles;

	private Vector2 _peglinFallingPosition = new Vector2(-12.52f, 1.92f);

	private Vector2 _peglinHealthBarFallingPosition = new Vector2(-12.5f, -0.48f);

	private SpiritOfRadiaBoss _spiritBoss;

	private PegBoardFrame[] _pegBoardFrames;

	private void Start()
	{
		_spiritBoss = UnityEngine.Object.FindObjectOfType<SpiritOfRadiaBoss>();
	}

	private void OnEnable()
	{
		SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Combine(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(StartPhase2Transition));
		BattleController.OnRoundCountIncremented = (BattleController.RoundCountIncremented)Delegate.Combine(BattleController.OnRoundCountIncremented, new BattleController.RoundCountIncremented(OnRoundCountIncremented));
	}

	private void OnDisable()
	{
		SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted = (SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted)Delegate.Remove(SpiritOfRadiaBoss.OnSpiritOfRadiaPhaseTransitionStarted, new SpiritOfRadiaBoss.SpiritOfRadiaPhaseTransitionStarted(StartPhase2Transition));
		BattleController.OnRoundCountIncremented = (BattleController.RoundCountIncremented)Delegate.Remove(BattleController.OnRoundCountIncremented, new BattleController.RoundCountIncremented(OnRoundCountIncremented));
	}

	private void OnRoundCountIncremented(int roundCount)
	{
		if (_spiritBoss != null && SpiritOfRadiaBoss.CurrentPhase == 1)
		{
			int num = _spiritBoss.transitionOnTurnNumber - roundCount + 1;
			if (num != _spiritBoss.transitionOnTurnNumber)
			{
				phase1CountdownText.text = num.ToString();
				phase1CountdownText.DOKill();
				countdownBackground.DOKill();
				Sequence s = DOTween.Sequence();
				s.Append(phase1CountdownText.DOFade(1f, 0.5f).From(0f).SetEase(Ease.OutQuad)
					.SetUpdate(isIndependentUpdate: true));
				s.Append(phase1CountdownText.DOFade(0f, 3f).SetDelay(1f).SetEase(Ease.InOutQuad)
					.SetUpdate(isIndependentUpdate: true));
				Sequence s2 = DOTween.Sequence();
				s2.Append(countdownBackground.DOFade(0.8f, 0.5f).From(0f).SetEase(Ease.OutQuad)
					.SetUpdate(isIndependentUpdate: true));
				s2.Append(countdownBackground.DOFade(0f, 3f).SetDelay(1f).SetEase(Ease.InOutQuad)
					.SetUpdate(isIndependentUpdate: true));
			}
		}
	}

	private void StartPhase2Transition()
	{
		StartCoroutine(Phase2Transition());
	}

	private IEnumerator Phase2Transition()
	{
		deckDisplay.transform.position = new Vector3(0.43f, -4.7f, 0f);
		upcomingEnemyDisplay.SetActive(value: false);
		orbStatsDisplay.transform.position += new Vector3(0f, -20f, 0f);
		discardButton.transform.position += new Vector3(0.81299967f, 15.402f, 0f);
		GameObject[] array = fallingParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		peglin.transform.DOMove(new Vector3(_peglinFallingPosition.x, _peglinFallingPosition.y, peglin.transform.position.z), 4f);
		playerHealthBar.transform.DOMove(new Vector3(_peglinHealthBarFallingPosition.x, _peglinHealthBarFallingPosition.y, playerHealthBar.transform.position.z), 4f);
		peglin.GetComponent<Animator>().SetBool(Eaten, value: true);
		yield break;
	}
}
