using System.Collections.Generic;
using UnityEngine;

namespace PeglinUI.Encirclepedia
{
	public class DoublePagedCarouselManager : MonoBehaviour
	{
		public int objectsPerPage = 9;

		public GameObject pageOne;

		public GameObject pageTwo;

		public void SetIcons(List<GameObject> icons)
		{
			for (int i = 0; i < icons.Count; i++)
			{
				GameObject gameObject = ((i % 2 == 0) ? pageOne : pageTwo);
				icons[i].transform.parent = gameObject.transform;
			}
		}
	}
}
