using System.Collections;
using System.Collections.Generic;
using PeglinUtils;
using UnityEngine;

namespace Battle.PegBehaviour
{
	public class FireworkMovement : MonoBehaviour
	{
		public GameObject[] gameObjects;

		public float timeBetweenToggle = 0.8f;

		public int prevActiveIndex;

		private List<IDummyPeg> _statusChangers = new List<IDummyPeg>();

		private IEnumerator Start()
		{
			yield return WaitObjectAllocator.waitForEndOfFrame;
			yield return WaitObjectAllocator.waitForEndOfFrame;
			GameObject[] array = gameObjects;
			for (int i = 0; i < array.Length; i++)
			{
				IDummyPeg component = array[i].GetComponent<IDummyPeg>();
				if (component != null && !((SimpleStatusChanger)component).GetDummyStatus())
				{
					_statusChangers.Add(component);
				}
			}
		}

		private void Update()
		{
			int num = Mathf.FloorToInt(Time.time / timeBetweenToggle) % (gameObjects.Length + 1) - 1;
			if (num != prevActiveIndex)
			{
				ToggleGameObjects(num);
				prevActiveIndex = num;
			}
		}

		private void ToggleGameObjects(int index)
		{
			for (int i = 0; i < gameObjects.Length; i++)
			{
				gameObjects[i].SetActive(i <= index);
			}
			if (_statusChangers.Count > 0)
			{
				PredictionManager.RequestSpecificPegsUpdate(_statusChangers);
			}
		}
	}
}
