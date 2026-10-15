namespace Battle.PegBehaviour
{
	public interface IDummyPeg
	{
		void GenerateId(bool force = false);

		string GetId();

		void SetDummyStatus(bool status);

		void SetPegStatus(bool status);

		bool GetPegStatus();

		bool IsSpecialPeg();

		void SetSpecialCollider(bool value);

		Peg.PegType GetPegType();

		void SetPegType(Peg.PegType pegType);
	}
}
