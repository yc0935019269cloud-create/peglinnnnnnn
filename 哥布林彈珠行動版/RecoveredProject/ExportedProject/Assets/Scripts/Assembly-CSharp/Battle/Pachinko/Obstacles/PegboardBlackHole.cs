using UnityEngine;

namespace Battle.Pachinko.Obstacles
{
	public class PegboardBlackHole : MonoBehaviour, IUpdateableBySimulation
	{
		private const float INCREASE_STENGTH_AFTER_SECONDS = 5f;

		private static readonly string[] AttractLayers = new string[2] { "PachinkoBall", "PachinkoBallTrigger(Ball)" };

		private LayerMask _pachinkoBallLayer;

		public float gravityRange = 4f;

		public float gravityStrength = 10f;

		private Collider2D[] _nearbyPachinkoBalls = new Collider2D[16];

		private bool _isDummy;

		private bool _disabledDuringShot;

		private bool _isBallInRange;

		private float _timeThatABallHasBeeInRange;

		private void Awake()
		{
			_pachinkoBallLayer = LayerMask.GetMask(AttractLayers);
		}

		private void FixedUpdate()
		{
			if (!_isDummy)
			{
				DoUpdate(Physics2D.defaultPhysicsScene);
				if (_isBallInRange)
				{
					_timeThatABallHasBeeInRange += Time.deltaTime;
				}
			}
		}

		public void DoUpdate(PhysicsScene2D physicsScene2D)
		{
			_isBallInRange = false;
			physicsScene2D.OverlapCircle(base.transform.position, gravityRange, _nearbyPachinkoBalls, _pachinkoBallLayer);
			if (_nearbyPachinkoBalls.Length != 0)
			{
				for (int i = 0; i < _nearbyPachinkoBalls.Length; i++)
				{
					Collider2D collider2D = _nearbyPachinkoBalls[i];
					if (!(collider2D != null))
					{
						continue;
					}
					PachinkoBall componentInParent = collider2D.gameObject.GetComponentInParent<PachinkoBall>();
					Rigidbody2D componentInParent2 = collider2D.gameObject.GetComponentInParent<Rigidbody2D>();
					bool flag = (!componentInParent.IsDummy && componentInParent.IsFiring()) || componentInParent.IsDummy;
					if (!componentInParent.IsDummy && componentInParent.IsFiring() && !_isBallInRange)
					{
						_isBallInRange = true;
					}
					if (!((componentInParent2 != null && componentInParent != null) & flag))
					{
						continue;
					}
					Vector2 vector = base.transform.position - collider2D.transform.position;
					float num = vector.magnitude / gravityRange;
					if (num > 0.005f)
					{
						Vector2 force = vector.normalized * (gravityStrength / (num * num));
						if (_timeThatABallHasBeeInRange > 5f)
						{
							force *= _timeThatABallHasBeeInRange - 5f + 1f;
						}
						componentInParent2.AddForce(force);
						_nearbyPachinkoBalls[i] = null;
					}
				}
			}
			if (!_isBallInRange)
			{
				_timeThatABallHasBeeInRange = 0f;
			}
		}

		public void EnableIfDisabledDuringShot()
		{
			if (_disabledDuringShot)
			{
				base.gameObject.SetActive(value: true);
				_disabledDuringShot = false;
			}
		}

		public void Disable()
		{
			base.gameObject.SetActive(value: false);
			_disabledDuringShot = true;
		}

		public void SetDummy(bool value)
		{
			_isDummy = value;
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawWireSphere(base.transform.position, gravityRange);
		}
	}
}
