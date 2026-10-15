using UnityEngine;

namespace PeglinUtils
{
	public class DestroyObjectIfDuplicateBehaviorExists : MonoBehaviour
	{
		public MonoBehaviour TargetType;

		private void Awake()
		{
			Object[] array = Object.FindObjectsOfType(TargetType.GetType());
			foreach (Object obj in array)
			{
				if (!(obj != null))
				{
					continue;
				}
				MonoBehaviour monoBehaviour = obj as MonoBehaviour;
				if (monoBehaviour != null && monoBehaviour.gameObject != base.gameObject)
				{
					if (Debug.isDebugBuild)
					{
						Debug.Log("Destroying " + base.gameObject.name + ", duplicate found...");
					}
					Object.Destroy(base.gameObject);
				}
			}
		}
	}
}
