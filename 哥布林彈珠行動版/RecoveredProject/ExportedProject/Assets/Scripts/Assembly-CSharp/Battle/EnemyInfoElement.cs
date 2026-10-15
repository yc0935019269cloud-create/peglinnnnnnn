using Battle.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace Battle
{
	public class EnemyInfoElement : MonoBehaviour
	{
		public delegate void EnemyInfoElementClicked(Enemy enemy);

		public static EnemyInfoElementClicked OnEnemyInfoElementClicked;

		[SerializeField]
		private Image _enemyImage;

		private Enemy _enemy;

		public void SetEnemy(Enemy enemy)
		{
			_enemy = enemy;
			if (_enemy.UpcommingEnemySpriteOverride != null)
			{
				_enemyImage.sprite = _enemy.UpcommingEnemySpriteOverride;
			}
			else
			{
				_enemyImage.sprite = _enemy.DefaultInfoSprite;
			}
			_enemyImage.color = _enemy.DefaultInfoSpriteColor;
			if (_enemy is FleeingEnemy)
			{
				GetComponentInChildren<RectTransform>().localScale = new Vector3(-1f, 1f, 1f);
			}
		}
	}
}
