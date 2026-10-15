using System;
using System.Collections;
using System.Collections.Generic;
using Achievements;
using Battle.Enemies;
using Battle.Enemies.Polish;
using DG.Tweening;
using I2.Loc;
using Peglin.Achievements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPopupHandler : MonoBehaviour
{
	[Header("Parameters")]
	public float pauseTime = 4f;

	public float endBufferTime;

	public bool DemoMode;

	public int DemoFrequency;

	public AchievementDatabase achievementDb;

	[Header("Scene refs")]
	public TextMeshProUGUI achievementTitle;

	public TextMeshProUGUI achievementDesc;

	public Image image;

	private RectTransform _transform;

	private AudioSource _sfx;

	private int _counter;

	private bool _animationPlaying;

	private Queue<Achievement> _queuedUnlocks = new Queue<Achievement>();

	private Queue<EnemyData> _queuedEnemyUnlocks = new Queue<EnemyData>();

	private Vector2 _startPos;

	private Sequence _currentSequence;

	private void OnEnable()
	{
		_counter = DemoFrequency;
		_transform = GetComponent<RectTransform>();
		_sfx = GetComponent<AudioSource>();
		AchievementManager.OnAchievementUnlocked = (AchievementManager.AchievementUnlockedEvent)Delegate.Combine(AchievementManager.OnAchievementUnlocked, new AchievementManager.AchievementUnlockedEvent(SetupMessage));
		CardDropAnimation.OnCardAchievementShouldPop = (CardDropAnimation.CardAnimationFinished)Delegate.Combine(CardDropAnimation.OnCardAchievementShouldPop, new CardDropAnimation.CardAnimationFinished(SetupMessage));
	}

	public bool IsAnimating()
	{
		return _animationPlaying;
	}

	private void OnDisable()
	{
		AchievementManager.OnAchievementUnlocked = (AchievementManager.AchievementUnlockedEvent)Delegate.Remove(AchievementManager.OnAchievementUnlocked, new AchievementManager.AchievementUnlockedEvent(SetupMessage));
		CardDropAnimation.OnCardAchievementShouldPop = (CardDropAnimation.CardAnimationFinished)Delegate.Remove(CardDropAnimation.OnCardAchievementShouldPop, new CardDropAnimation.CardAnimationFinished(SetupMessage));
	}

	private void Update()
	{
		if (DemoMode)
		{
			_counter--;
			if (_counter <= 0)
			{
				_counter = DemoFrequency;
				PlayAnimation();
			}
		}
	}

	private void SetupMessage(Achievement achievement)
	{
		if (!_animationPlaying)
		{
			achievementTitle.text = achievement.Name;
			achievementDesc.text = achievement.Description;
			image.sprite = achievementDb.GetAchievement(achievement.Id)?.UnlockedImage;
			PlayAnimation();
		}
		else
		{
			_queuedUnlocks.Enqueue(achievement);
		}
	}

	private void SetupMessage(EnemyData enemyData)
	{
		if (!_animationPlaying)
		{
			achievementTitle.text = LocalizationManager.GetTranslation(enemyData.Name);
			achievementDesc.text = LocalizationManager.GetTranslation("Menu/Battle/card_unlocked");
			image.sprite = enemyData.encirclepediaSprite;
			PlayAnimation();
		}
		else
		{
			_queuedEnemyUnlocks.Enqueue(enemyData);
		}
	}

	private void PlayAnimation()
	{
		if (_animationPlaying)
		{
			return;
		}
		_startPos = _transform.anchoredPosition;
		_animationPlaying = true;
		_sfx.Play();
		_sfx.pitch = 1f + UnityEngine.Random.Range(-0.1f, 0.1f);
		_currentSequence = DOTween.Sequence();
		_currentSequence.Append(_transform.DOLocalMoveY(1f, 0f)).Append(_transform.DOLocalMoveY(59f, 0.5f)).AppendInterval(pauseTime)
			.Append(_transform.DOLocalMoveY(-60f, 0.5f))
			.AppendInterval(endBufferTime)
			.SetRelative(isRelative: true)
			.OnKill(() =>
			{
				if (base.gameObject != null && base.gameObject.activeInHierarchy)
				{
					StartCoroutine(WaitAndThenDequeue());
				}
			});
		_currentSequence.Play();
	}

	private void AnimationEnd()
	{
		base.transform.position = _startPos;
		_animationPlaying = false;
		if (_queuedUnlocks.Count > 0)
		{
			SetupMessage(_queuedUnlocks.Dequeue());
		}
	}

	private IEnumerator WaitAndThenDequeue()
	{
		_transform.anchoredPosition = _startPos;
		_animationPlaying = false;
		yield return new WaitForSeconds(2f);
		if (_queuedUnlocks.Count > 0)
		{
			SetupMessage(_queuedUnlocks.Dequeue());
		}
		else if (_queuedEnemyUnlocks.Count > 0)
		{
			SetupMessage(_queuedEnemyUnlocks.Dequeue());
		}
	}
}
