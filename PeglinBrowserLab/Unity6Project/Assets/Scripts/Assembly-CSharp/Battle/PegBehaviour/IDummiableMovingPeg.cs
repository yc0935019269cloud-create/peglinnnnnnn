using UnityEngine;

namespace Battle.PegBehaviour
{
	public interface IDummiableMovingPeg : IDummyPeg
	{
		Vector3 GetPosition();

		void SetPosition(Vector3 position);

		Vector3 GetScale();

		void SetScale(Vector3 scale);

		string GetObjectName();
	}
}
