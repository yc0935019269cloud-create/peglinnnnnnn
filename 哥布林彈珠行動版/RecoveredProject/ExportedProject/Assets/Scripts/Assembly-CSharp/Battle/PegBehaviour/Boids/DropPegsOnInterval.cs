using UnityEngine;

namespace Battle.PegBehaviour.Boids
{
	public class DropPegsOnInterval : MonoBehaviour
	{
		public Peg pegToDrop;

		public float minDropTime;

		public float maxDropTime;

		private float randomDropTime;

		private float timeSinceDrop;

		private RegularPeg _myPeg;

		private void Start()
		{
			_myPeg = GetComponent<RegularPeg>();
			randomDropTime = Random.Range(minDropTime, maxDropTime);
		}

		private void Update()
		{
			if (!_myPeg.IsDisabled())
			{
				timeSinceDrop += Time.deltaTime;
				if (timeSinceDrop >= randomDropTime)
				{
					DropPeg();
					randomDropTime = Random.Range(minDropTime, maxDropTime);
					timeSinceDrop = 0f;
				}
			}
		}

		private void DropPeg()
		{
			Object.Instantiate(pegToDrop, base.transform.position, base.transform.rotation);
		}
	}
}
