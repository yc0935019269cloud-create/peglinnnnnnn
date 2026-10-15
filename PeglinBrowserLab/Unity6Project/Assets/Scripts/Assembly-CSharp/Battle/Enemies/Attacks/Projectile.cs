using Battle.StatusEffects;
using UnityEngine;

namespace Battle.Enemies.Attacks
{
	public class Projectile : MonoBehaviour
	{
		public delegate void ProjectileFired(float firingLength = -1f);

		public ProjectileFired OnProjectileFired = (float firingLength) =>
		{
		};

		[Tooltip("Used for things like slime goop where it should match the slime.")]
		public bool applyEnemyColour;

		public StatusEffect[] appliedStatusEffects;

		public float statusEffectScaleByTurn;

		public float sfxStartDelay;

		[HideInInspector]
		public float damage;

		[HideInInspector]
		public int extraProjectileForAttackNumber;

		[HideInInspector]
		public int totalExtraProjectiles;

		[HideInInspector]
		public float extraProjectileTotalSpawnTime;

		public bool HasHitDestination { get; protected set; } = true;

		protected virtual void Update()
		{
			if (!base.gameObject.activeSelf && HasHitDestination)
			{
				Object.Destroy(base.gameObject);
			}
		}

		public void CheckForStatusScaling(int numTurns)
		{
			if (statusEffectScaleByTurn > 0f && appliedStatusEffects.Length != 0)
			{
				StatusEffect[] array = appliedStatusEffects;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Intensity += Mathf.CeilToInt(statusEffectScaleByTurn * (float)numTurns);
				}
			}
		}

		public virtual void Fire(Vector3 target, Vector3 projectileStartOffset)
		{
			Debug.LogWarning("Projectile::Fire(): Instantiated base projectile class, only subclasses should be created");
		}
	}
}
