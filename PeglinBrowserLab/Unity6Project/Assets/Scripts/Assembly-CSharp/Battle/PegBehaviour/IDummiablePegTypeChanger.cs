namespace Battle.PegBehaviour
{
	public interface IDummiablePegTypeChanger : IDummyPeg
	{
		new Peg.PegType GetPegType();

		new void SetPegType(Peg.PegType pegType);
	}
}
