using Battle.Attacks.Interfaces;
using Battle.StatusEffects;
using UnityEngine;

namespace Battle.Attacks.AttackBehaviours
{
	public class GrantStatusAfterAttackPerShieldPeg : MonoBehaviour, PostAttackOperation
	{
		public StatusEffectType[] effectTypes;

		public float shieldAmountMultiplier = 1f;

		public float shieldPegBuffsPerExtraIntencity = 4f;

		public float shieldPegDebuffsPerExtraIntencity = -2f;

		private PegManager _pegManager;

		private void Start()
		{
			BattleController battleController = Object.FindObjectOfType<BattleController>();
			if (battleController != null)
			{
				_pegManager = battleController.pegManager;
			}
		}

		public void HandleAttackFinished(PlayerStatusEffectController statusController)
		{
			int intensity = 0;
			if (_pegManager != null)
			{
				(int, int, int) tuple = _pegManager.CountShieldPegsOnBoard();
				int item = tuple.Item1;
				int item2 = tuple.Item2;
				int item3 = tuple.Item3;
				intensity = Mathf.RoundToInt((float)item * shieldAmountMultiplier + (float)Mathf.FloorToInt((float)item2 / shieldPegBuffsPerExtraIntencity) + (float)Mathf.FloorToInt((float)(-item3) / (0f - shieldPegDebuffsPerExtraIntencity)));
			}
			StatusEffectType[] array = effectTypes;
			foreach (StatusEffectType effectType in array)
			{
				statusController.ApplyStatusEffect(new StatusEffect(effectType, intensity), StatusEffectSource.PLAYER);
			}
		}
	}
}
