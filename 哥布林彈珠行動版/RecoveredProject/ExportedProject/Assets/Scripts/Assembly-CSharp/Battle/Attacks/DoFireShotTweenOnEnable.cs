using UnityEngine;

namespace Battle.Attacks
{
	public abstract class DoFireShotTweenOnEnable : MonoBehaviour
	{
		protected readonly Vector3 peglinArmOffset = new Vector3(0.44f, 0.49f, 0f);

		protected readonly Vector3 peglinNormalPos = new Vector3(-9.5f, 6f, -0.1f);

		protected GameObject peglinGameObject;

		protected Vector3 peglinArmPos;

		private void OnEnable()
		{
			peglinGameObject = GameObject.FindWithTag("Player");
			peglinArmPos = peglinGameObject.transform.position + peglinArmOffset;
			StartTween();
		}

		protected abstract void StartTween();
	}
}
