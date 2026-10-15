using Battle.Pachinko.BallBehaviours;
using UnityEngine;

namespace Battle.Attacks
{
	public class SnowballShotBehavior : ShotBehavior
	{
		public static readonly string SNOWBALL_SIZE_ANIM_INT_NAME = "snowball_size";

		private const float MID_LARGE_ATTACK_Y_OFFSET = 0.27f;

		private const float LARGE_ATTACK_Y_OFFSET = 0.69f;

		public int minSnowballSizeIndexToPierceAllGroundedEnemies = 2;

		public int minSnowballSizeIndexToAttacksAllEnemies = 4;

		private Vector3 _initialPositionOffset = Vector3.zero;

		public Vector3 initialPositionOffset => _initialPositionOffset;

		public Animator anim => _anim;

		public void SetAttackType()
		{
			if (SnowballDmgAndSize.allSnowballsCurrentSizeIndex >= minSnowballSizeIndexToAttacksAllEnemies)
			{
				_minSize = new Vector3(2f, 2f, 1f);
				_maxSize = new Vector3(2f, 2f, 1f);
				Object.Destroy(_collider);
				BoxCollider2D boxCollider2D = base.gameObject.AddComponent<BoxCollider2D>();
				boxCollider2D.isTrigger = true;
				boxCollider2D.offset = new Vector2(0.5553589f, 0.7930202f);
				boxCollider2D.size = new Vector2(1.710719f, 2.730023f);
				_collider = boxCollider2D;
				_initialPositionOffset = new Vector3(0f, 0.69f, 0f);
				_canAimUp = false;
				_shotType = ShotType.PIERCE;
				_enemiesToPierce = 99;
			}
			else if (SnowballDmgAndSize.allSnowballsCurrentSizeIndex >= minSnowballSizeIndexToPierceAllGroundedEnemies)
			{
				_minSize = new Vector3(1.4f, 1.4f, 1f);
				_maxSize = new Vector3(1.4f, 1.4f, 1f);
				_initialPositionOffset = new Vector3(0f, 0.27f, 0f);
				GetComponent<CircleCollider2D>().offset = new Vector2(0f, -0.19f);
				_canAimUp = false;
				_shotType = ShotType.PIERCE;
				_enemiesToPierce = 99;
			}
			else if (SnowballDmgAndSize.allSnowballsCurrentSizeIndex == 1)
			{
				_minSize = new Vector3(0.4f, 0.4f, 1f);
				_maxSize = new Vector3(0.4f, 0.4f, 1f);
			}
		}
	}
}
