public interface IDummiableCollisionTogglePeg
{
	string GetId();

	void SetDummyStatus(bool status);

	bool GetCollisionEnabled();

	void SetCollisionEnabled(bool enabled);
}
