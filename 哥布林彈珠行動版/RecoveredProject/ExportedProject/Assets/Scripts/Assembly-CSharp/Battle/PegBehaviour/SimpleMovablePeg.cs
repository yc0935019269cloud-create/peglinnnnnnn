using System;
using Peglin.OdinSerializer.Utilities;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class SimpleMovablePeg : MonoBehaviour, IDummiableMovingPegEndOfTurn, IDummyPeg
	{
		[HideInInspector]
		public string id = string.Empty;

		private bool _isDummy;

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
