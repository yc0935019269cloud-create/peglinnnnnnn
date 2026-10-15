using UnityEngine;

public interface IUpdateableBySimulation
{
	void DoUpdate(PhysicsScene2D physicsScene2D);

	void EnableIfDisabledDuringShot();

	void Disable();

	void SetDummy(bool value);
}
