using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugSimulatePhysics : MonoBehaviour
{
	private PhysicsScene2D _physicsScene2D;

	private void OnEnable()
	{
		_physicsScene2D = SceneManager.GetActiveScene().GetPhysicsScene2D();
	}

	private void FixedUpdate()
	{
		if (_physicsScene2D.IsValid())
		{
			_physicsScene2D.Simulate(Time.fixedDeltaTime);
		}
	}
}
