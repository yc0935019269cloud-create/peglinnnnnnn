using DG.Tweening;
using UnityEngine;

namespace Battle.Enemies.Behaviours
{
	public class FallToGroundOnDeath : MonoBehaviour
	{
		public float fallDuration = 0.5f;

		public Ease ease = Ease.InExpo;

		public float fallToYOffset;

		private Enemy enemy;

		private bool _deathTriggered;

		private void Awake()
		{
			enemy = GetComponent<Enemy>();
		}

		private void Update()
		{
			if (enemy.CurrentHealth <= 0f && !_deathTriggered)
			{
				_deathTriggered = true;
				base.gameObject.transform.DOMoveY(enemy.enemyManager.groundEnemiesHeight + fallToYOffset, fallDuration).SetEase(ease);
			}
		}
	}
}
