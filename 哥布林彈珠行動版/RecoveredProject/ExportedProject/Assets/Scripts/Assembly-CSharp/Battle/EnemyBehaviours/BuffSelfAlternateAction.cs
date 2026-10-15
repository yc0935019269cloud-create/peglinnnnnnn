using System.Collections;
using Battle.Enemies;
using Battle.StatusEffects;
using PeglinUtils;
using UnityEngine;

namespace Battle.EnemyBehaviours
{
	public class BuffSelfAlternateAction : AlternateEnemyAction
	{
		public string animOverride = "";

		public StatusEffect[] statusEffectsToAdd;

		public int forSpiritOfRadiaPhase = -1;

		private Animator _anim;

		private void Start()
		{
			_anim = _enemy.GetComponent<Animator>();
		}

		public override bool ConditionsMet()
		{
			if (forSpiritOfRadiaPhase != -1)
			{
				return SpiritOfRadiaBoss.CurrentPhase == forSpiritOfRadiaPhase;
			}
			return true;
		}

		public override IEnumerator TakeAction(Vector3 target)
		{
			_enemy.CurrentlyActing = true;
			PlaySFX();
			if (animOverride != "")
			{
				_anim.SetTrigger(animOverride);
				yield return WaitObjectAllocator.waitForEndOfFrame;
				float timer = 0f;
				AnimatorStateInfo stateInfo = _anim.GetCurrentAnimatorStateInfo(0);
				while (timer <= stateInfo.length)
				{
					timer += Time.deltaTime;
					yield return WaitObjectAllocator.waitForEndOfFrame;
				}
			}
			StatusEffect[] array = statusEffectsToAdd;
			foreach (StatusEffect statusEffect in array)
			{
				if (statusEffect.Intensity > 0)
				{
					_enemy.ApplyStatusEffect(new StatusEffect(statusEffect.EffectType, statusEffect.Intensity), StatusEffectSource.ENEMY);
				}
				else if (statusEffect.Intensity < 0)
				{
					_enemy.ReduceEffectIntensity(statusEffect.EffectType, -statusEffect.Intensity);
				}
			}
			_enemy.CurrentlyActing = false;
		}
	}
}
