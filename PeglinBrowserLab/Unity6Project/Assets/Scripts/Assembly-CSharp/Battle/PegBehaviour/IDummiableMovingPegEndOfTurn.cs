using UnityEngine;

namespace Battle.PegBehaviour
{
	public interface IDummiableMovingPegEndOfTurn : IDummyPeg
	{
		Vector3 GetPosition();

		void SetPosition(Vector3 position);

		string GetObjectName();
	}
}
