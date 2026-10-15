using System;
using Battle.Enemies.Behaviours.EnemyStances;
using Battle.PegBehaviour;
using UnityEngine;

namespace Battle.Enemies.Behaviours
{
	public class StanceAlternatingBehaviour : MonoBehaviour
	{
		public EnemyStance[] enemyStances;

		public int initialStanceIndex;

		private Enemy _enemy;

		private int _currentStanceIndex;

		private PegLayoutAlternator[] _layoutAlternators;

		private void OnEnable()
		{
			BattleController.OnTurnComplete = (Action)Delegate.Combine(BattleController.OnTurnComplete, new Action(ChangeStance));
		}

		private void OnDisable()
		{
			BattleController.OnTurnComplete = (Action)Delegate.Remove(BattleController.OnTurnComplete, new Action(ChangeStance));
		}

		private void Start()
		{
			_enemy = GetComponent<Enemy>();
			_currentStanceIndex = initialStanceIndex;
			enemyStances[initialStanceIndex].ApplyStanceToEnemy(_enemy, initial: true);
			_layoutAlternators = UnityEngine.Object.FindObjectsOfType<PegLayoutAlternator>(includeInactive: true);
		}

		public void ChangeStance()
		{
			enemyStances[_currentStanceIndex].RemoveStanceOnEnemy(_enemy);
			_currentStanceIndex++;
			if (_currentStanceIndex >= enemyStances.Length)
			{
				_currentStanceIndex = 0;
			}
			enemyStances[_currentStanceIndex].ApplyStanceToEnemy(_enemy);
			if (_layoutAlternators != null && _layoutAlternators.Length != 0)
			{
				PegLayoutAlternator[] layoutAlternators = _layoutAlternators;
				for (int i = 0; i < layoutAlternators.Length; i++)
				{
					layoutAlternators[i].AlternateLayouts();
				}
			}
		}
	}
}
