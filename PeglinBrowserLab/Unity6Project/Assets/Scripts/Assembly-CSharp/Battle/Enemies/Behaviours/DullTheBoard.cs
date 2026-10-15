using System;
using System.Collections;
using System.Collections.Generic;
using Battle.EnemyBehaviours;
using Battle.StatusEffects;
using DG.Tweening;
using PeglinUtils;
using UnityEngine;

namespace Battle.Enemies.Behaviours
{
	public class DullTheBoard : AlternateEnemyAction
	{
		private const float TOP_MASK_POS = 2.023f;

		private const float DULL_EFFECT_LENGTH = 1f;

		public int numberOfPegsToDull = 3;

		public bool canDullPoppedPegs = true;

		public string dullTheBoardAnimation = "attack";

		public string dullTheBoardAnimationEnd = "attack";

		public GameObject pegDullingEffect;

		public float convertToRegularEffectPreDelay;

		public float convertToRegularEffectLength = 1.5f;

		public float convertToRegularEffectPostDelay = 0.2f;

		public Vector3 dullingEffectEnemyPosOffset;

		public AudioClip dullSFX;

		public GameObject spriteMaskObject;

		private Animator _anim;

		private PegManager _pegManager;

		protected List<RegularPeg> _sectionOfPegsToDull = new List<RegularPeg>();

		private List<Peg> _allDulledPegs = new List<Peg>();

		private int _enemySlot;

		private PaintingBoardSection[] _boardSections;

		private float _maskBottomPosition;

		private CreateSpinfectionPegsOnDeath _createSpinfectionPegsOnDeath;

		public List<RegularPeg> sectionOfPegsToDull
		{
			get
			{
				GrabBoardSectionIfNull();
				return _sectionOfPegsToDull;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			_anim = GetComponent<Animator>();
			_pegManager = UnityEngine.Object.FindObjectOfType<BattleController>().pegManager;
			_createSpinfectionPegsOnDeath = GetComponent<CreateSpinfectionPegsOnDeath>();
			_maskBottomPosition = spriteMaskObject.transform.position.y;
		}

		private void Start()
		{
			_enemySlot = _enemy.enemyManager.GetSlotIndexForEnemy(_enemy, out var _);
			_boardSections = UnityEngine.Object.FindObjectsOfType<PaintingBoardSection>();
			PainterBossUpgrade painterBossUpgrade = UnityEngine.Object.FindObjectOfType<PainterBossUpgrade>();
			if (painterBossUpgrade != null && painterBossUpgrade.isUpgraded)
			{
				numberOfPegsToDull += 2;
			}
		}

		private void OnEnable()
		{
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyDestroyed));
		}

		private void OnDisable()
		{
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyDestroyed));
		}

		private void GrabBoardSectionIfNull()
		{
			if (_sectionOfPegsToDull != null && _sectionOfPegsToDull.Count != 0)
			{
				return;
			}
			if (_boardSections == null || _boardSections.Length == 0)
			{
				_boardSections = UnityEngine.Object.FindObjectsOfType<PaintingBoardSection>();
			}
			bool flag = false;
			PaintingBoardSection[] boardSections = _boardSections;
			foreach (PaintingBoardSection paintingBoardSection in boardSections)
			{
				if (!paintingBoardSection.isDummy && paintingBoardSection.forPaintingInSlotNumbers.Contains(_enemySlot))
				{
					_sectionOfPegsToDull = paintingBoardSection.section;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_sectionOfPegsToDull = new List<RegularPeg>(UnityEngine.Object.FindObjectsOfType<RegularPeg>());
				for (int num = _sectionOfPegsToDull.Count - 1; num >= 0; num--)
				{
					if (_sectionOfPegsToDull[num].GetComponent<SpriteRenderer>() == null)
					{
						_sectionOfPegsToDull.RemoveAt(num);
					}
				}
				_enemy.DamagePerRangedAttack = Mathf.Clamp(Mathf.RoundToInt((float)_sectionOfPegsToDull.Count / 10f), 1f, 4f);
			}
			(int, int) activeDullPegsInSection = GetActiveDullPegsInSection();
			UpdateGreyScalePercentage(activeDullPegsInSection.Item1, activeDullPegsInSection.Item2);
		}

		public override bool ConditionsMet()
		{
			GrabBoardSectionIfNull();
			(int, int) activeDullPegsInSection = GetActiveDullPegsInSection();
			UpdateGreyScalePercentage(activeDullPegsInSection.Item1, activeDullPegsInSection.Item2);
			var (num, num2) = activeDullPegsInSection;
			return num != num2;
		}

		private (int, int) GetActiveDullPegsInSection()
		{
			int num = 0;
			int num2 = 0;
			foreach (RegularPeg item in _sectionOfPegsToDull)
			{
				if ((canDullPoppedPegs || !item.IsDisabled()) && item.pegType != Peg.PegType.CRIT && item.pegType != Peg.PegType.RESET && item.pegType != Peg.PegType.SPINFECTION && item.pegType != Peg.PegType.DESTROYED)
				{
					num2++;
				}
				if ((canDullPoppedPegs || !item.IsDisabled()) && item.pegType == Peg.PegType.DULL && item.pegType != Peg.PegType.DESTROYED)
				{
					num++;
				}
			}
			return (num, num2);
		}

		public override IEnumerator TakeAction(Vector3 target)
		{
			_audioSource.pitch = 1f + UnityEngine.Random.Range(-0.2f, 0.2f);
			_audioSource.PlayOneShot(dullSFX);
			GrabBoardSectionIfNull();
			_enemy.CurrentlyActing = true;
			if (_sectionOfPegsToDull == null)
			{
				Debug.LogError("Enemy " + base.gameObject.name + " has no section to dull.");
			}
			_anim.SetTrigger(dullTheBoardAnimation);
			yield return WaitObjectAllocator.waitForEndOfFrame;
			float timer = 0f;
			AnimatorStateInfo stateInfo = _anim.GetCurrentAnimatorStateInfo(0);
			while (timer <= stateInfo.length)
			{
				timer += Time.deltaTime;
				yield return WaitObjectAllocator.waitForEndOfFrame;
			}
			List<Peg> pegsToTurnIntoDullPegs = _pegManager.GetPegsToTurnIntoDullPegs(numberOfPegsToDull + _enemy.GetEffectIntensity(StatusEffectType.Strength), canDullPoppedPegs, _sectionOfPegsToDull);
			_allDulledPegs.AddRange(pegsToTurnIntoDullPegs);
			foreach (Peg item in pegsToTurnIntoDullPegs)
			{
				PegEffectAnimator newPegDullingEffect = UnityEngine.Object.Instantiate(pegDullingEffect, item.transform.position, Quaternion.identity).GetComponent<PegEffectAnimator>();
				PegEffectAnimator pegEffectAnimator = newPegDullingEffect;
				pegEffectAnimator.OnFlyToOrFromPegAnimationFinished = (PegEffectAnimator.EffectAnimationFinished)Delegate.Combine(pegEffectAnimator.OnFlyToOrFromPegAnimationFinished, (PegEffectAnimator.EffectAnimationFinished)(() =>
				{
					UnityEngine.Object.Destroy(newPegDullingEffect.gameObject);
				}));
				newPegDullingEffect.DoFlyToOrFromPegEffect(item.transform.position, _enemy.transform.position + dullingEffectEnemyPosOffset, 0f, 1f, 0f);
			}
			(int, int) activeDullPegsInSection = GetActiveDullPegsInSection();
			UpdateGreyScalePercentage(activeDullPegsInSection.Item1, activeDullPegsInSection.Item2);
			yield return new WaitForSeconds(1f);
			_anim.SetTrigger(dullTheBoardAnimationEnd);
			timer = 0f;
			stateInfo = _anim.GetCurrentAnimatorStateInfo(0);
			while (timer <= stateInfo.length)
			{
				timer += Time.deltaTime;
				yield return WaitObjectAllocator.waitForEndOfFrame;
			}
			_audioSource.pitch = 1f;
			_enemy.CurrentlyActing = false;
		}

		private void UpdateGreyScalePercentage(int activeDullPegs, int totalActivePegs)
		{
			float num;
			if (totalActivePegs == 0 && _sectionOfPegsToDull != null && _sectionOfPegsToDull.Count > 0)
			{
				num = 1f;
			}
			else if (totalActivePegs == 0)
			{
				num = 0f;
			}
			else
			{
				num = (float)activeDullPegs / (float)totalActivePegs;
				if ((double)num > 0.8 && (double)num != 1.0)
				{
					num = 0.8f;
				}
			}
			float endValue = Mathf.Lerp(_maskBottomPosition, _maskBottomPosition + 2.023f, num);
			spriteMaskObject.transform.DOMoveY(endValue, 1.2f).SetEase(Ease.OutSine);
		}

		private void OnEnemyDestroyed(Enemy enemy)
		{
			GrabBoardSectionIfNull();
			if (!(enemy == _enemy))
			{
				return;
			}
			if (_createSpinfectionPegsOnDeath != null)
			{
				_createSpinfectionPegsOnDeath.CheckAndSetPegsToConvert();
			}
			foreach (Peg allDulledPeg in _allDulledPegs)
			{
				if (allDulledPeg.pegType != Peg.PegType.DESTROYED && (_createSpinfectionPegsOnDeath == null || !_createSpinfectionPegsOnDeath.pegsToConvertList.Contains(allDulledPeg)))
				{
					PegEffectAnimator newPegDullingEffect = UnityEngine.Object.Instantiate(pegDullingEffect, _enemy.transform.position, Quaternion.identity).GetComponent<PegEffectAnimator>();
					PegEffectAnimator pegEffectAnimator = newPegDullingEffect;
					pegEffectAnimator.OnFlyToOrFromPegAnimationFinished = (PegEffectAnimator.EffectAnimationFinished)Delegate.Combine(pegEffectAnimator.OnFlyToOrFromPegAnimationFinished, (PegEffectAnimator.EffectAnimationFinished)(() =>
					{
						UnityEngine.Object.Destroy(newPegDullingEffect.gameObject);
					}));
					newPegDullingEffect.DoFlyToOrFromPegEffect(_enemy.transform.position + dullingEffectEnemyPosOffset, allDulledPeg.transform.position, convertToRegularEffectPreDelay, convertToRegularEffectLength, convertToRegularEffectPostDelay, allDulledPeg, Peg.PegType.REGULAR);
				}
			}
		}
	}
}
