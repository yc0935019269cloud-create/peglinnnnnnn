using UnityEngine;

namespace Battle.Pachinko.Obstacles
{
	public class PegboardSlotInhale : MonoBehaviour, IUpdateableBySimulation
	{
		private static readonly string[] AttractLayers = new string[2] { "PachinkoBall", "PachinkoBallTrigger(Ball)" };

		private LayerMask _pachinkoBallLayer;

		public Vector2 attractZoneSize = new Vector2(4f, 5f);

		public float gravityStrength = 10f;

		private Collider2D[] _nearbyPachinkoBalls = new Collider2D[16];

		private bool _isDummy;

		private bool _disabledDuringShot;

		private void Awake()
		{
			_pachinkoBallLayer = LayerMask.GetMask(AttractLayers);
		}

		private void FixedUpdate()
		{
			if (!_isDummy)
			{
				DoUpdate(Physics2D.defaultPhysicsScene);
			}
		}

		public void DoUpdate(PhysicsScene2D physicsScene2D)
		{
			Vector3 position = base.transform.position;
			Vector2 vector = new Vector2(position.x - attractZoneSize.x, position.y);
			for (int i = 0; i < _nearbyPachinkoBalls.Length; i++)
			{
				_nearbyPachinkoBalls[i] = null;
			}
			physicsScene2D.OverlapArea(vector, vector + attractZoneSize, _nearbyPachinkoBalls, _pachinkoBallLayer);
			if (_nearbyPachinkoBalls.Length == 0)
			{
				return;
			}
			Collider2D[] nearbyPachinkoBalls = _nearbyPachinkoBalls;
			foreach (Collider2D collider2D in nearbyPachinkoBalls)
			{
				if (!(collider2D != null))
				{
					continue;
				}
				PachinkoBall componentInParent = collider2D.gameObject.GetComponentInParent<PachinkoBall>();
				Rigidbody2D componentInParent2 = collider2D.gameObject.GetComponentInParent<Rigidbody2D>();
				bool flag = (!componentInParent.IsDummy && componentInParent.IsFiring()) || componentInParent.IsDummy;
				if ((componentInParent2 != null && componentInParent != null) & flag)
				{
					float num = (collider2D.transform.position.y - base.transform.position.y) / attractZoneSize.y;
					if (num > 0.005f)
					{
						componentInParent2.AddForce(Vector2.down * gravityStrength / num);
					}
				}
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
	}
}
