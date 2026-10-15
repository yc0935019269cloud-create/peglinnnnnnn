using Relics;
using UnityEngine;

namespace Battle.Pachinko.Obstacles
{
	public class Bouncer : MonoBehaviour
	{
		[SerializeField]
		private RelicManager _relicManager;

		public PhysicsMaterial2D bouncyMaterial;

		[Tooltip("Used for the pegboard parser, for which prefab to use when loading")]
		public bool useMinesPrefab;

		private CircleCollider2D _circleCollider;

		private void Awake()
		{
			_circleCollider = GetComponent<CircleCollider2D>();
		}

		private void OnEnable()
		{
			if (_relicManager.AttemptUseRelic(RelicEffect.BOUNCY_WALLS_AND_BOUNCERS))
			{
				_circleCollider.sharedMaterial = bouncyMaterial;
			}
		}
	}
}
