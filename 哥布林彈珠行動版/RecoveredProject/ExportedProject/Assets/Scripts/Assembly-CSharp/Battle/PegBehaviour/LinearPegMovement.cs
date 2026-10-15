using System;
using Peglin.OdinSerializer.Utilities;
using UnityEngine;

namespace Battle.PegBehaviour
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class LinearPegMovement : MonoBehaviour, IDummiableMovingPeg, IDummyPeg
	{
		public float xMovement;

		public float yMovement;

		[Tooltip("If speed is granted dynamically (like skating pegs), should we have a minimum?")]
		public float minSpeed;

		public float maxSpeed;

		public float leftBoundary = -8.5f;

		public float rightBoundary = 8.5f;

		public float topBound = 5.5f;

		public float bottomBound = -9.75f;

		[HideInInspector]
		public string id = string.Empty;

		private bool _isDummy;

		private Rigidbody2D _rigid;

		private void Start()
		{
			SnapToGrid component = GetComponent<SnapToGrid>();
			if (component != null)
			{
				component.enabled = false;
			}
			_rigid = GetComponent<Rigidbody2D>();
			if (!_isDummy)
			{
				_rigid.velocity = new Vector2(xMovement, yMovement);
				if (id.IsNullOrWhitespace())
				{
					id = Guid.NewGuid().ToString();
				}
			}
			else
			{
				_rigid.isKinematic = true;
			}
		}

		public void CopyFrom(LinearPegMovement source)
		{
			xMovement = source.xMovement;
			yMovement = source.yMovement;
			leftBoundary = source.leftBoundary;
			rightBoundary = source.rightBoundary;
		}

		private void FixedUpdate()
		{
			if (!_isDummy)
			{
				if (_rigid.velocity.magnitude >= 0.001f && _rigid.velocity.magnitude <= minSpeed)
				{
					_rigid.velocity = _rigid.velocity.normalized * minSpeed;
				}
				else if (maxSpeed > 0f && _rigid.velocity.magnitude > maxSpeed)
				{
					_rigid.velocity = _rigid.velocity.normalized * maxSpeed;
				}
				Vector3 position = base.transform.position;
				bool flag = false;
				if (position.x <= leftBoundary)
				{
					position.x = rightBoundary - (leftBoundary - position.x);
					flag = true;
				}
				else if (position.x >= rightBoundary)
				{
					position.x = leftBoundary + (position.x - rightBoundary);
					flag = true;
				}
				if (position.y >= topBound)
				{
					position.y = bottomBound - (topBound - position.y);
					flag = true;
				}
				else if (position.y <= bottomBound)
				{
					position.y = topBound + (position.y - bottomBound);
					flag = true;
				}
				if (flag)
				{
					base.transform.position = position;
				}
			}
		}

		public void SetDummyStatus(bool status)
		{
			_isDummy = status;
		}

		public void SetPegStatus(bool status)
		{
		}

		public Vector3 GetPosition()
		{
			return base.transform.position;
		}

		public void SetPosition(Vector3 position)
		{
			if (!(_rigid == null))
			{
				_rigid.MovePosition(position);
			}
		}

		public string GetObjectName()
		{
			return base.gameObject.name;
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

		public void SetScale(Vector3 scale)
		{
			base.transform.localScale = scale;
		}

		public Vector3 GetScale()
		{
			return base.transform.localScale;
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
	}
}
