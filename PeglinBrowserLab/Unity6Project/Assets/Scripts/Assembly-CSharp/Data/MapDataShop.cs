using UnityEngine;

namespace Data
{
	[CreateAssetMenu(fileName = "New Shop", menuName = "Scenarios/ShopRoom")]
	public class MapDataShop : MapData
	{
		public override SceneType GetSceneType()
		{
			return SceneType.STORE;
		}
	}
}
