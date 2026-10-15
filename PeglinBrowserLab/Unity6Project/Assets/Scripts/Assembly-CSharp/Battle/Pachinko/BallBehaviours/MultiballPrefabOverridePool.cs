using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	[RequireComponent(typeof(Multiball))]
	public class MultiballPrefabOverridePool : MonoBehaviour
	{
		public bool copyToChildMultiballs = true;

		[SerializeField]
		private GameObject[] multiballOverridePool;

		public GameObject GetMultiballGameobject()
		{
			return multiballOverridePool[Random.Range(0, multiballOverridePool.Length)];
		}

		public void AddCopyOfComponentToOrb(GameObject orb)
		{
			if (copyToChildMultiballs)
			{
				MultiballPrefabOverridePool multiballPrefabOverridePool = orb.AddComponent<MultiballPrefabOverridePool>();
				multiballPrefabOverridePool.multiballOverridePool = multiballOverridePool;
				multiballPrefabOverridePool.copyToChildMultiballs = copyToChildMultiballs;
			}
		}
	}
}
