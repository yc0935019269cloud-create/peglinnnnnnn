using System;
using System.Collections.Generic;
using Battle.Pachinko.BallBehaviours;
using UnityEngine;

namespace Battle.Attacks.AttackModifications
{
	[CreateAssetMenu(menuName = "Battle/Attack Modifier")]
	public class AttackModifier : ScriptableObject
	{
		[Flags]
		public enum AttackModifierTypes
		{
			NONE = 0,
			ADD_STATS = 1,
			ADD_ATTACK_ALL = 4,
			ADD_START_WITH_CRIT = 8,
			ADD_MULTIBALL_ON_PREV_FIRED_PATH = 0x10,
			ADD_RUBBER = 0x20,
			ADD_RUBBER_TWICE = 0x40,
			ADD_RUBBER_THREE_TIMES = 0x80
		}

		[Flags]
		public enum AttackModifierPhase
		{
			NONE = 0,
			ON_ATTACK = 1,
			ON_DISCARD = 2
		}

		private const float MAX_BOUNCINESS = 0.99f;

		private const float ABSOLUTE_MAX_BOUNCINESS = 0.999f;

		public bool onlyApplyToNextOrbDrawn;

		[Header("Modifiers")]
		public AttackModifierTypes modifierTypes;

		public AttackModifierPhase modifierPhase;

		[Header("Modifier Data")]
		public int baseDamageModification;

		public int critDamageModification;

		public List<float> multiballFiredForceMultipliers;

		[Header("BaseData")]
		public BaseAttackModifierData baseData;

		public void ApplyStatModificationsToOrb(GameObject orb)
		{
			if (modifierTypes.HasFlag(AttackModifierTypes.ADD_STATS))
			{
				Attack component = orb.GetComponent<Attack>();
				if (component != null)
				{
					component.AddPreviousOrbDamageModifier(baseDamageModification, critDamageModification);
				}
			}
		}

		public void ApplyOtherModificationsToOrb(GameObject orb)
		{
			if (modifierTypes.HasFlag(AttackModifierTypes.ADD_RUBBER))
			{
				ApplyRubberModificationToOrb(orb);
			}
			if (modifierTypes.HasFlag(AttackModifierTypes.ADD_RUBBER_TWICE))
			{
				ApplyRubberModificationToOrb(orb);
				ApplyRubberModificationToOrb(orb);
			}
			if (modifierTypes.HasFlag(AttackModifierTypes.ADD_RUBBER_THREE_TIMES))
			{
				ApplyRubberModificationToOrb(orb);
				ApplyRubberModificationToOrb(orb);
				ApplyRubberModificationToOrb(orb);
			}
			if (modifierTypes.HasFlag(AttackModifierTypes.ADD_ATTACK_ALL))
			{
				ApplyAttackAll(orb);
			}
			if (modifierTypes.HasFlag(AttackModifierTypes.ADD_START_WITH_CRIT))
			{
				ApplyStartWithCrit(orb);
			}
			if (modifierTypes.HasFlag(AttackModifierTypes.ADD_MULTIBALL_ON_PREV_FIRED_PATH))
			{
				ApplyMultiballOnPrevPathFired(orb);
			}
		}

		private void ApplyRubberModificationToOrb(GameObject orb)
		{
			Rigidbody2D component = orb.GetComponent<Rigidbody2D>();
			if (component == null)
			{
				return;
			}
			PhysicsMaterial2D sharedMaterial = component.sharedMaterial;
			if (sharedMaterial == null)
			{
				component.sharedMaterial = baseData.rubberMaterial;
				return;
			}
			PhysicsMaterial2D physicsMaterial2D = new PhysicsMaterial2D();
			physicsMaterial2D.friction = sharedMaterial.friction;
			if (sharedMaterial.bounciness >= 0.99f)
			{
				physicsMaterial2D.bounciness = sharedMaterial.bounciness + 0.002f;
				physicsMaterial2D.bounciness = Mathf.Min(physicsMaterial2D.bounciness, 0.999f);
				component.sharedMaterial = physicsMaterial2D;
			}
			else
			{
				float num = (float)Mathf.CeilToInt((1f - sharedMaterial.bounciness) * 100f / 15f) * 0.05f;
				physicsMaterial2D.bounciness = sharedMaterial.bounciness + num;
				physicsMaterial2D.bounciness = Mathf.Min(physicsMaterial2D.bounciness, 0.99f);
				component.sharedMaterial = physicsMaterial2D;
			}
		}

		private void ApplyAttackAll(GameObject orb)
		{
			ProjectileAttack component = orb.GetComponent<ProjectileAttack>();
			TargetedAttack component2 = orb.GetComponent<TargetedAttack>();
			if (component != null)
			{
				component.SetShotPrefab(baseData.attackAllShot, baseData.attackAllShot);
			}
			else if (component2 != null)
			{
				component2.modifiedToAttackAll = true;
			}
		}

		private void ApplyStartWithCrit(GameObject orb)
		{
			orb.AddComponent<ActivateCritOnDraw>();
		}

		private void ApplyMultiballOnPrevPathFired(GameObject orb)
		{
			orb.AddComponent<MultiballOnPrevFiredPath>().multiballFireForceMultipliers = multiballFiredForceMultipliers;
		}
	}
}
