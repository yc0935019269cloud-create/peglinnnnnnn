using Peglin.ClassSystem;
using Saving;
using UnityEngine;

namespace PeglinUI.RunSummary
{
	public class ClassWinPrefab : MonoBehaviour
	{
		public GameObject peglinSlice;

		public GameObject balladinSlice;

		public GameObject roundrelSlice;

		public GameObject spinventorSlice;

		public GameObject peglinSliceShiny;

		public GameObject balladinSliceShiny;

		public GameObject roundrelSliceShiny;

		public GameObject spinventorSliceShiny;

		private void Awake()
		{
			peglinSlice.SetActive(PersistentPlayerData.Instance.CruciballLevels[Class.Peglin] >= 0);
			roundrelSlice.SetActive(PersistentPlayerData.Instance.CruciballLevels[Class.Roundrel] >= 0);
			balladinSlice.SetActive(PersistentPlayerData.Instance.CruciballLevels[Class.Balladin] >= 0);
			spinventorSlice.SetActive(PersistentPlayerData.Instance.CruciballLevels[Class.Spinventor] >= 0);
			peglinSliceShiny.SetActive(value: false);
			roundrelSliceShiny.SetActive(value: false);
			balladinSliceShiny.SetActive(value: false);
			spinventorSliceShiny.SetActive(value: false);
		}

		public void SetShinySliceForClassWeJustWonWith(Class classJustWonWith)
		{
			switch (classJustWonWith)
			{
			case Class.Peglin:
				peglinSliceShiny.SetActive(value: true);
				break;
			case Class.Roundrel:
				roundrelSliceShiny.SetActive(value: true);
				break;
			case Class.Balladin:
				balladinSliceShiny.SetActive(value: true);
				break;
			case Class.Spinventor:
				spinventorSliceShiny.SetActive(value: true);
				break;
			}
		}
	}
}
