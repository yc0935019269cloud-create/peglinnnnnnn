using Battle.Enemies;
using Loading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PeglinUtils
{
	public class TestBestiary : MonoBehaviour
	{
		private void OnEnable()
		{
			EnemyData[] enemyDataList = AssetLoading.Instance.Bestiary.enemyDataList;
			foreach (EnemyData enemyData in enemyDataList)
			{
				Enemy component = Addressables.LoadAssetAsync<GameObject>(enemyData.enemyPrefabAssetReference).WaitForCompletion().GetComponent<Enemy>();
				if (component == null)
				{
					Debug.LogError($"Found no enemy script for {enemyData.LocKey}, asset {enemyData.enemyPrefabAssetReference}");
				}
				else if (component.locKey.Equals(enemyData.LocKey + "_name"))
				{
					Debug.Log("Checked " + enemyData.LocKey + " successfully");
				}
				else
				{
					Debug.LogError("Data file loc key for " + enemyData.LocKey + " does not match " + component.locKey);
				}
			}
		}
	}
}
