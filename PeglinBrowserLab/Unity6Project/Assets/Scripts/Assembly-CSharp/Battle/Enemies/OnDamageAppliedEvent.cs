using System;
using UnityEngine.Events;

namespace Battle.Enemies
{
	[Serializable]
	public class OnDamageAppliedEvent : UnityEvent<string>
	{
	}
}
