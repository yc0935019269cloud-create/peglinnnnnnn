using System;
using Battle.PegBehaviour;
using Peglin.OdinSerializer.Utilities;
using UnityEngine;

public class PegLayoutHolder : MonoBehaviour, IDummyPeg
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
		base.gameObject.SetActive(status);
	}

	public bool GetPegStatus()
	{
		return base.gameObject.activeInHierarchy;
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
