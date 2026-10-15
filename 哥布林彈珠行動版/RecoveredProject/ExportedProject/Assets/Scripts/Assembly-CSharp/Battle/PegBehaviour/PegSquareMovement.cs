using System;
using DG.Tweening;
using Peglin.OdinSerializer.Utilities;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class PegSquareMovement : MonoBehaviour, IDummiableMovingPeg, IDummyPeg
	{
		public int startingCorner;

		public Vector2 oppositeCornerOffset;

		private Vector2 _oppositeCornerPosition;

		public float edgeMoveTime = 2f;

		public float cornerDelay = 1.2f;

		private Vector2 _startingPosition;

		private Vector2 _edgeStartingPosition;

		[HideInInspector]
		public string id = string.Empty;

		[HideInInspector]
		public bool finishedMovement = true;

		private Vector2 _corner1;

		private Vector2 _corner2;

		private Vector2 _corner3;

		private bool _paused = true;

		private float _timePaused;

		private int _currentCornerTarget = 1;

		private bool _isDummy;

		private Rigidbody2D _rb;

		private void Start()
		{
			_rb = GetComponent<Rigidbody2D>();
			_startingPosition = base.transform.position;
			_edgeStartingPosition = _startingPosition;
			_oppositeCornerPosition = _startingPosition + oppositeCornerOffset;
			_corner2 = _oppositeCornerPosition;
			switch (startingCorner)
			{
			case 0:
			case 2:
				_corner1 = new Vector2(_oppositeCornerPosition.x, _startingPosition.y);
				_corner3 = new Vector2(_startingPosition.x, _oppositeCornerPosition.y);
				break;
			case 1:
			case 3:
				_corner1 = new Vector2(_startingPosition.x, _oppositeCornerPosition.y);
				_corner3 = new Vector2(_oppositeCornerPosition.x, _startingPosition.y);
				break;
			}
			if (!_isDummy && id.IsNullOrWhitespace())
			{
				id = Guid.NewGuid().ToString();
			}
		}

		private void FixedUpdate()
		{
			if (_isDummy)
			{
				return;
			}
			if (_paused)
			{
				_timePaused += Time.fixedDeltaTime;
				if (_timePaused > cornerDelay)
				{
					_paused = false;
					_timePaused = 0f;
				}
			}
			else
			{
				MoveWithVelocity();
			}
		}

		private void MoveWithVelocity()
		{
			Vector2 vector = _startingPosition;
			Vector2 vector2 = base.transform.position;
			switch (_currentCornerTarget)
			{
			case 1:
				vector = _corner1;
				break;
			case 2:
				vector = _corner2;
				break;
			case 3:
				vector = _corner3;
				break;
			}
			if (Vector3.Distance(vector2, vector) >= 0.085f)
			{
				Vector2 normalized = (vector - vector2).normalized;
				float num = (vector - _edgeStartingPosition).magnitude / edgeMoveTime;
				_rb.MovePosition(vector2 + normalized * (num * Time.fixedDeltaTime));
			}
			else
			{
				_edgeStartingPosition = vector;
				_currentCornerTarget = (_currentCornerTarget + 1) % 4;
				_paused = true;
				finishedMovement = true;
			}
		}

		public void StartPause()
		{
			finishedMovement = false;
		}

		public void DoMove()
		{
			finishedMovement = false;
		}

		private void OnDrawGizmos()
		{
			Peg[] componentsInChildren = GetComponentsInChildren<Peg>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Gizmos.DrawSphere(componentsInChildren[i].transform.position + (Vector3)oppositeCornerOffset, 0.2f);
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
