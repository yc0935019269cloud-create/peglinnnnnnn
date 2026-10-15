using UnityEngine;

namespace Battle.Enemies.Polish
{
	public class RandomizeAnimations : MonoBehaviour
	{
		public RuntimeAnimatorController[] animationController;

		private void OnEnable()
		{
			if (animationController != null)
			{
				int num = Random.Range(0, animationController.Length);
				GetComponent<Animator>().runtimeAnimatorController = animationController[num];
			}
		}
	}
}
