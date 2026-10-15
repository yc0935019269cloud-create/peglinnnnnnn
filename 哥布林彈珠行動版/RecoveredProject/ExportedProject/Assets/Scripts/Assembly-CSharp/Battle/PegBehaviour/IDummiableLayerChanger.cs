namespace Battle.PegBehaviour
{
	public interface IDummiableLayerChanger : IDummyPeg
	{
		int GetLayer();

		void SetLayer(int layer);
	}
}
