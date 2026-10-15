using UnityEngine;

namespace Battle.PegBehaviour
{
	public class FlickeringPegLoopCreator : MonoBehaviour
	{
		public int numRunners = 4;

		public float visTime;

		private void Start()
		{
			FlickeringPeg[] componentsInChildren = GetComponentsInChildren<FlickeringPeg>(includeInactive: true);
			if (componentsInChildren.Length == 0)
			{
				Debug.LogError("No flickering peg children on " + base.gameObject.name);
				return;
			}
			float num = componentsInChildren[0].fadeTime / (float)componentsInChildren.Length * (float)numRunners * 2f;
			int num2 = 0;
			FlickeringPeg[] array = componentsInChildren;
			foreach (FlickeringPeg flickeringPeg in array)
			{
				int num3 = num2 % (componentsInChildren.Length / numRunners);
				flickeringPeg.initialDelay = (float)num3 * num;
				flickeringPeg.minDelay = (flickeringPeg.maxDelay = visTime);
				if (visTime > 0f)
				{
					flickeringPeg.useDifferentMinAndMaxForInvisible = true;
					flickeringPeg.minDelayWhenInvisible = (flickeringPeg.maxDelayWhenInvisible = 0f);
				}
				num2++;
			}
		}
	}
}
