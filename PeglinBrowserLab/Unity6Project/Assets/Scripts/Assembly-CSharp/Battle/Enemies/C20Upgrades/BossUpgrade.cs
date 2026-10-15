using UnityEngine;

namespace Battle.Enemies.C20Upgrades
{
	public class BossUpgrade : MonoBehaviour
	{
		[HideInInspector]
		public bool isUpgraded;

		protected Enemy _enemy;

		protected virtual void Awake()
		{
			_enemy = GetComponent<Enemy>();
		}

		public virtual void Upgrade()
		{
		}

		public virtual float GetUpgradedMaxHealth()
		{
			return _enemy.maxHealth;
		}
	}
}
