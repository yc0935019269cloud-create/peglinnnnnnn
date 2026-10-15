using System.Collections;
using Battle.Enemies.Behaviours.EnemyStances;
using Battle.PegBehaviour;
using PeglinUtils;
using UnityEngine;

namespace Battle.Enemies.Behaviours
{
	public class StanceChangingReactiveBehaviour : MonoBehaviour
	{
		[SerializeField]
		private EnemyStance initialStance;

		[SerializeField]
		private EnemyStance meleeStance;

		[SerializeField]
		private EnemyStance hitByProjectileAttackStance;

		[SerializeField]
		private EnemyStance hitByTargetedAttackStance;

		private BattleController _battleController;

		private Enemy _enemy;

		private EnemyStance _currentStance;

		private PegLayoutAlternator _layoutAlternator;

		private void Start()
		{
			_battleController = Object.FindObjectOfType<BattleController>();
			_enemy = GetComponent<Enemy>();
			_currentStance = initialStance;
			_currentStance.ApplyStanceToEnemy(_enemy, initial: true);
			_layoutAlternator = Object.FindObjectOfType<PegLayoutAlternator>(includeInactive: true);
		}

		public bool ShouldApplyMeleeStance()
		{
			return false;
		}

		public IEnumerator ApplyMeleeStance()
		{
			if (!(_currentStance != meleeStance))
			{
				yield break;
			}
			_enemy.queuedAnimations.Enqueue(Enemy.AnimationTypes.STANCE_CHANGE);
			float timer = 0f;
			if (timer <= 300f && (_enemy.CurrentlyActing || _enemy.queuedAnimations.Peek() != Enemy.AnimationTypes.STANCE_CHANGE))
			{
				timer += Time.deltaTime;
				yield return WaitObjectAllocator.waitForEndOfFrame;
				if (timer > 300f)
				{
					Debug.LogWarning("State change animation wait timed out");
				}
			}
			_currentStance.RemoveStanceOnEnemy(_enemy);
			_currentStance = meleeStance;
			_currentStance.ApplyStanceToEnemy(_enemy);
			if (_layoutAlternator != null)
			{
				_layoutAlternator.AlternateLayouts();
			}
		}

		public IEnumerator HitByProjectileAttack()
		{
			if (_currentStance != hitByProjectileAttackStance && !_enemy.queuedAnimations.Contains(Enemy.AnimationTypes.STANCE_CHANGE))
			{
				_enemy.queuedAnimations.Enqueue(Enemy.AnimationTypes.STANCE_CHANGE);
				float timer = 0f;
				if (timer <= 300f && (_enemy.CurrentlyActing || _enemy.queuedAnimations.Peek() != Enemy.AnimationTypes.STANCE_CHANGE))
				{
					timer += Time.deltaTime;
					yield return WaitObjectAllocator.waitForEndOfFrame;
					if (timer > 300f)
					{
						Debug.LogWarning("State change animation wait timed out");
					}
				}
				_currentStance.RemoveStanceOnEnemy(_enemy);
				_currentStance = hitByProjectileAttackStance;
				_currentStance.ApplyStanceToEnemy(_enemy);
				if ((bool)_layoutAlternator)
				{
					_layoutAlternator.AlternateLayouts();
				}
				_enemy.SkipNextAction();
				_enemy.CurrentlyActing = true;
			}
			else
			{
				_battleController.pegManager.CreateBonusSpecialPegs(Peg.PegType.RESET, 1);
				_battleController.pegManager.CreateBonusSpecialPegs(Peg.PegType.CRIT, 1);
			}
		}

		public IEnumerator HitByTargetedAttack()
		{
			if (_currentStance != hitByTargetedAttackStance && !_enemy.queuedAnimations.Contains(Enemy.AnimationTypes.STANCE_CHANGE))
			{
				_enemy.queuedAnimations.Enqueue(Enemy.AnimationTypes.STANCE_CHANGE);
				float timer = 0f;
				if (timer <= 300f && (_enemy.CurrentlyActing || _enemy.queuedAnimations.Peek() != Enemy.AnimationTypes.STANCE_CHANGE))
				{
					timer += Time.deltaTime;
					yield return WaitObjectAllocator.waitForEndOfFrame;
					if (timer > 300f)
					{
						Debug.LogWarning("State change animation wait timed out");
					}
				}
				_currentStance.RemoveStanceOnEnemy(_enemy);
				_currentStance = hitByTargetedAttackStance;
				_currentStance.ApplyStanceToEnemy(_enemy);
				if ((bool)_layoutAlternator)
				{
					_layoutAlternator.AlternateLayouts();
				}
				_enemy.SkipNextAction();
				_enemy.CurrentlyActing = true;
			}
			else
			{
				_battleController.pegManager.CreateBonusSpecialPegs(Peg.PegType.RESET, 1);
				_battleController.pegManager.CreateBonusSpecialPegs(Peg.PegType.CRIT, 1);
			}
		}

		public void StanceAnimationIsFinished()
		{
			if (_enemy != null)
			{
				_enemy.CurrentlyActing = false;
				if (_enemy.queuedAnimations.Count > 0)
				{
					_enemy.queuedAnimations.Dequeue();
				}
				else
				{
					Debug.LogWarning("Found no animation to dequeue from stance change end");
				}
			}
		}
	}
}
