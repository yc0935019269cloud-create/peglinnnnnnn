using System;
using DG.Tweening;
using Peglin.OdinSerializer.Utilities;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class PegMoveAndReturn : MonoBehaviour, IDummiableMovingPeg, IDummyPeg
	{
		public Vector2 startingPosition;

		public Vector2 targetOffsetA;

		public Vector2 targetOffsetB;

		public float moveToASpeed = 0.2f;

		public float moveToBSpeed = 0.2f;

		[HideInInspector]
		public float movementProgress;

		[HideInInspector]
		public string id = string.Empty;

		private Vector2 _targetA;

		private Vector2 _targetB;

		private bool _aToB;

		private bool _isDummy;

		private void Start()
		{
			_targetA = startingPosition + targetOffsetA;
			_targetB = startingPosition + targetOffsetB;
			if (!_isDummy)
			{
				movementProgress = Vector2.Distance(_targetA, startingPosition) / Vector2.Distance(_targetA, _targetB);
				if (id.IsNullOrWhitespace())
				{
					id = Guid.NewGuid().ToString();
				}
			}
		}

		private void FixedUpdate()
		{
			if (!_isDummy)
			{
				Move();
			}
		}

		private void Move()
		{
			if (!_isDummy)
			{
				float num = (_aToB ? (moveToBSpeed * Time.deltaTime) : (moveToASpeed * Time.deltaTime * -1f));
				movementProgress = Mathf.Clamp01(movementProgress + num);
			}
			base.transform.localPosition = Vector3.Lerp(_targetA, _targetB, movementProgress);
			if (movementProgress <= 0f)
			{
				_aToB = true;
			}
			else if (movementProgress >= 1f)
			{
				_aToB = false;
			}
		}

		private void OnDrawGizmos()
		{
			Peg[] componentsInChildren = GetComponentsInChildren<Peg>();
			foreach (Peg peg in componentsInChildren)
			{
				Gizmos.DrawSphere(peg.transform.position + (Vector3)targetOffsetA, 0.2f);
				if (targetOffsetB != Vector2.zero)
				{
					Gizmos.DrawSphere(peg.transform.position + (Vector3)targetOffsetB, 0.2f);
				}
			}
		}

		private void OnDisable()
		{
			base.transform.DOKill();
		}

		public string GetObjectName()
		{
			return base.gameObject.name;
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

		public Vector3 GetPosition()
		{
			return base.transform.position;
		}

		public void SetPosition(Vector3 position)
		{
			if (_isDummy)
			{
				base.transform.position = position;
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

		public Vector3 GetScale()
		{
			return base.transform.localScale;
		}

		public void SetScale(Vector3 scale)
		{
			if (_isDummy)
			{
				base.transform.localScale = scale;
			}
		}

		public void SetPegType(Peg.PegType pegType)
		{
		}
	}
}
