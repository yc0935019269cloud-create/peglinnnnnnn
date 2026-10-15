using System.Collections;
using System.Globalization;
using Battle.Enemies;
using Battle.StatusEffects;
using I2.Loc;
using UnityEngine;

namespace Battle.Attacks
{
	public class ApplyStatusEffectPerPegOnSelf : ProjectileAttack
	{
		[Header("Status Effect Parameteres")]
		public StatusEffectType effectToApply;

		public float pegCountPerStack = 8f;

		public bool shouldAlsoShoot;

		private void OnValidate()
		{
			LocalizationParamsManager component = GetComponent<LocalizationParamsManager>();
			if (component != null)
			{
				component.SetParameterValue("BALLWARK_GRANT", pegCountPerStack.ToString(CultureInfo.InvariantCulture));
			}
		}

		public override void Fire(AttackManager attackManager, Enemy target, int pegMultipliersTally, int numPegsHit, float dmgMult, long bonus, int critCount = 0)
		{
			if (shouldAlsoShoot)
			{
				base.Fire(attackManager, target, pegMultipliersTally, numPegsHit, dmgMult, bonus, critCount);
			}
			PlayerStatusEffectController playerStatusEffectController = GameObject.FindWithTag("Player")?.GetComponent<PlayerStatusEffectController>();
			if (playerStatusEffectController != null)
			{
				float num = 1f / pegCountPerStack;
				StatusEffect statusEffect = new StatusEffect(effectToApply, Mathf.FloorToInt((float)numPegsHit * num));
				playerStatusEffectController.ApplyStatusEffect(statusEffect, StatusEffectSource.PLAYER);
			}
			StartCoroutine(WaitAndEndAnim());
		}

		private IEnumerator WaitAndEndAnim()
		{
			yield return new WaitForSeconds(1f);
			_attackManager.AttackAnimationEnded();
		}
	}
}
