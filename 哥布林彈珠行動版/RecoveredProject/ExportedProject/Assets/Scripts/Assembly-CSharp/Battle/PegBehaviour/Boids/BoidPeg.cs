using System;
using Peglin.OdinSerializer.Utilities;
using UnityEngine;

namespace Battle.PegBehaviour.Boids
{
	public class BoidPeg : MonoBehaviour, IDummiableMovingPeg, IDummyPeg
	{
		public Vector2 direction = Vector2.right;

		public float travelSpeed = 0.1f;

		public float turnSpeed = 0.25f;

		public float viewDist = 3f;

		public float wallWeight = 0.7f;

		public float separationWeight = 0.4f;

		public float cohesionWeight = 0.1f;

		public float alignmentWeight = 0.1f;

		public ContactFilter2D boidCollisionContactFilter;

		public Vector2 bottomLeft;

		public Vector2 topRight;

		[HideInInspector]
		public string id = string.Empty;

		private bool _isDummy;

		public void RecalculateVelocity()
		{
			if (_isDummy)
			{
				return;
			}
			Collider2D[] array = new Collider2D[8];
			Physics2D.OverlapCircle(base.transform.position, viewDist, boidCollisionContactFilter, array);
			Vector2 zero = Vector2.zero;
			Vector2 zero2 = Vector2.zero;
			Vector2 zero3 = Vector2.zero;
			Vector2 zero4 = Vector2.zero;
			if (array.Length == 0)
			{
				return;
			}
			int num = 0;
			Collider2D[] array2 = array;
			foreach (Collider2D collider2D in array2)
			{
				if (!(collider2D != null) || !(collider2D.transform != base.transform))
				{
					continue;
				}
				BoidPeg component = collider2D.GetComponent<BoidPeg>();
				if (component != null)
				{
					num++;
					Vector2 vector = component.transform.position - base.transform.position;
					if (vector.magnitude > 0.0001f)
					{
						zero += (vector - zero) / num;
						zero2 += (component.direction - zero2) / num;
						zero3 -= vector / Vector2.Dot(vector, vector);
					}
				}
			}
			if (base.transform.position.x < bottomLeft.x + viewDist)
			{
				zero4.x += Mathf.Abs(viewDist / base.transform.position.x - bottomLeft.x);
			}
			else if (base.transform.position.x > topRight.x - viewDist)
			{
				zero4.x -= Mathf.Abs(viewDist / topRight.x - base.transform.position.x);
			}
			if (base.transform.position.y < bottomLeft.y + viewDist)
			{
				zero4.y += Mathf.Abs(viewDist / base.transform.position.y - bottomLeft.y);
			}
			else if (base.transform.position.y > topRight.y - viewDist)
			{
				zero4.y -= Mathf.Abs(viewDist / topRight.y - base.transform.position.y);
			}
			Vector2 b = zero4.normalized * wallWeight + (zero3.normalized * separationWeight + zero.normalized * cohesionWeight + zero2.normalized * alignmentWeight).normalized;
			if (b.magnitude >= 0.001f)
			{
				direction = Vector2.Lerp(direction, b, turnSpeed);
			}
		}

		public void PerformMove()
		{
			if (!_isDummy)
			{
				base.transform.Translate(direction * travelSpeed);
			}
		}

		public void GenerateId(bool force = false)
		{
			if (id.IsNullOrWhitespace() | force)
			{
				id = Guid.NewGuid().ToString();
			}
		}

		public string GetId()
		{
			return id;
		}

		public void SetDummyStatus(bool status)
		{
			_isDummy = status;
		}

		public void SetPegStatus(bool status)
		{
		}

		public bool GetPegStatus()
		{
			return true;
		}

		public bool IsSpecialPeg()
		{
			return false;
		}

		public void SetSpecialCollider(bool value)
		{
		}

		public Peg.PegType GetPegType()
		{
			return Peg.PegType.NONE;
		}

		public void SetPegType(Peg.PegType pegType)
		{
		}

		public Vector3 GetPosition()
		{
			return base.transform.position;
		}

		public void SetPosition(Vector3 position)
		{
			base.transform.position = position;
		}

		public Vector3 GetScale()
		{
			return base.transform.localScale;
		}

		public void SetScale(Vector3 scale)
		{
			base.transform.localScale = scale;
		}

		public string GetObjectName()
		{
			return base.gameObject.name;
		}
	}
}
