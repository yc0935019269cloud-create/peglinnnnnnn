using System;
using System.Collections.Generic;
using Battle.Enemies;
using Battle.Enemies.Behaviours;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class CreateSpinfectionPegsOnDeath : MonoBehaviour
	{
		public int amount = 1;

		public GameObject spinfectionPegCreationEffect;

		public float explodeOutDistance = 0.3f;

		public float preDelay = 0.3f;

		public float effectLength = 1.8f;

		public float postDelay;

		private Enemy _enemy;

		private PegManager _pegManager;

		private List<Peg> _pegsToConvertList = new List<Peg>();

		private DullTheBoard _dullTheBoard;

		public List<Peg> pegsToConvertList => _pegsToConvertList;

		private void Awake()
		{
			_enemy = GetComponent<Enemy>();
			_pegManager = UnityEngine.Object.FindObjectOfType<BattleController>().pegManager;
			_dullTheBoard = GetComponent<DullTheBoard>();
		}

		private void OnEnable()
		{
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Combine(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyDestroyed));
			_pegsToConvertList.Clear();
		}

		private void OnDisable()
		{
			Enemy.OnEnemyDestroyed = (Enemy.EnemyDestroyed)Delegate.Remove(Enemy.OnEnemyDestroyed, new Enemy.EnemyDestroyed(OnEnemyDestroyed));
		}

		private void OnEnemyDestroyed(Enemy enemy)
		{
			if (enemy == _enemy)
			{
				DoAnimation();
			}
		}

		public void CheckAndSetPegsToConvert()
		{
			if (_pegsToConvertList == null || _pegsToConvertList.Count <= 0)
			{
				List<Peg> pegsList = new List<Peg>();
				_pegsToConvertList = _pegManager.GetPegsToTurnIntoSpinfectionPegs(amount, pegsList);
			}
		}

		private void DoAnimation()
		{
			new List<Peg>();
			if (_dullTheBoard != null)
			{
				preDelay = _dullTheBoard.convertToRegularEffectPreDelay;
				effectLength = _dullTheBoard.convertToRegularEffectLength;
				postDelay = _dullTheBoard.convertToRegularEffectPostDelay;
			}
			CheckAndSetPegsToConvert();
			foreach (Peg p in _pegsToConvertList)
			{
				PegEffectAnimator newEffect = UnityEngine.Object.Instantiate(spinfectionPegCreationEffect, _enemy.transform.position, Quaternion.identity).GetComponent<PegEffectAnimator>();
				newEffect.DoExplodeOutFromPoint(_enemy.GetCenterPosition(), explodeOutDistance, 0f, preDelay * 0.7f, preDelay * 0.3f);
				PegEffectAnimator pegEffectAnimator = newEffect;
				pegEffectAnimator.OnExplodeAnimationFinished = (PegEffectAnimator.EffectAnimationFinished)Delegate.Combine(pegEffectAnimator.OnExplodeAnimationFinished, (PegEffectAnimator.EffectAnimationFinished)(() =>
				{
					newEffect.DoFlyToOrFromPegEffect(newEffect.transform.position, p.transform.position, 0f, effectLength, postDelay, p, Peg.PegType.SPINFECTION);
				}));
				PegEffectAnimator pegEffectAnimator2 = newEffect;
				pegEffectAnimator2.OnFlyToOrFromPegAnimationFinished = (PegEffectAnimator.EffectAnimationFinished)Delegate.Combine(pegEffectAnimator2.OnFlyToOrFromPegAnimationFinished, (PegEffectAnimator.EffectAnimationFinished)(() =>
				{
					UnityEngine.Object.Destroy(newEffect.gameObject);
				}));
			}
		}
	}
}
