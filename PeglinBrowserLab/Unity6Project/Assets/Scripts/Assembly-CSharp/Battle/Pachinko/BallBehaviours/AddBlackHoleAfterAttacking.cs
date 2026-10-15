using System;
using Battle.Attacks.Interfaces;
using Battle.Pachinko.Obstacles;
using Battle.StatusEffects;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class AddBlackHoleAfterAttacking : MonoBehaviour, PostAttackOperation
	{
		public GameObject blackHolePrefab;

		private static int _blackHolesCounter;

		private Vector2[] _blackHoleCoordinates = new Vector2[5]
		{
			new Vector2(-8f, -7f),
			new Vector2(8f, -7f),
			new Vector2(-8f, 4.08f),
			new Vector2(8f, 4.08f),
			new Vector2(-0.02f, -0.28f)
		};

		private PredictionManager _predictionManager;

		private void Awake()
		{
			_predictionManager = UnityEngine.Object.FindObjectOfType<PredictionManager>();
		}

		private void OnEnable()
		{
			BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Combine(BattleController.OnBattleEnded, new BattleController.BattleEvent(ResetStaticVariables));
		}

		private void OnDisable()
		{
			BattleController.OnBattleEnded = (BattleController.BattleEvent)Delegate.Remove(BattleController.OnBattleEnded, new BattleController.BattleEvent(ResetStaticVariables));
		}

		public static void ResetStaticVariables()
		{
			_blackHolesCounter = 0;
		}

		public void HandleAttackFinished(PlayerStatusEffectController statusController)
		{
			if (_blackHolesCounter < _blackHoleCoordinates.Length)
			{
				CreateBlackhole();
			}
		}

		private void CreateBlackhole()
		{
			PegboardBlackHole component = UnityEngine.Object.Instantiate(blackHolePrefab, _blackHoleCoordinates[_blackHolesCounter], Quaternion.identity).GetComponent<PegboardBlackHole>();
			component.gravityRange = 4f;
			component.gravityStrength = 4f;
			_blackHolesCounter++;
			if (_predictionManager == null)
			{
				_predictionManager = UnityEngine.Object.FindObjectOfType<PredictionManager>();
				if (_predictionManager == null)
				{
					return;
				}
			}
			_predictionManager.AddObjectToSimulationScene(component.gameObject);
		}
	}
}
