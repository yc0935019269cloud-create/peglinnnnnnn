using Battle.Enemies.Attacks;
using UnityEngine;

namespace Battle
{
	public class DamagesEnemyOnTrigger : MonoBehaviour
	{
		[Tooltip("If < 0, will check for damage from an attached projectile.")]
		[SerializeField]
		private int _damage = -1;

		[SerializeField]
		private float _damageMod = 2f;

		public int Damage
		{
			get
			{
				if (_damage < 0)
				{
					return Mathf.RoundToInt(GetComponent<Projectile>().damage * _damageMod);
				}
				return Mathf.RoundToInt((float)_damage * _damageMod);
			}
		}
	}
}
