using System.Collections.Generic;
using Challenges;
using Cruciball;
using Loading;
using UnityEngine;

namespace PeglinUI.RunSummary
{
	public class WinLoseImage : MonoBehaviour
	{
		public CruciballManager cruciballManager;

		public GameObject winScreen;

		public GameObject coreWinScreen;

		public GameObject[] classImages;

		public GameObject loseScreenForest;

		public GameObject loseScreenCastle;

		public GameObject loseScreenMines;

		public GameObject loseScreenCore;

		private Dictionary<PeglinSceneLoader.Scene, GameObject> mapNamesDict;

		private void OnEnable()
		{
			mapNamesDict = new Dictionary<PeglinSceneLoader.Scene, GameObject>
			{
				{
					PeglinSceneLoader.Scene.FOREST_MAP,
					loseScreenForest
				},
				{
					PeglinSceneLoader.Scene.CASTLE_MAP,
					loseScreenCastle
				},
				{
					PeglinSceneLoader.Scene.MINES_MAP,
					loseScreenMines
				},
				{
					PeglinSceneLoader.Scene.CORE_MAP,
					loseScreenCore
				}
			};
			GameObject value;
			if ((cruciballManager.VictoryThisRun && !ChallengeManager.Instance.chosenToGoToCore) || StaticGameData.CurrentRunStats.hasWonCore)
			{
				if (StaticGameData.CurrentRunStats != null && StaticGameData.CurrentRunStats.hasWonCore)
				{
					coreWinScreen.SetActive(value: true);
					winScreen.SetActive(value: false);
					return;
				}
				winScreen.SetActive(value: true);
				coreWinScreen.SetActive(value: false);
				if (classImages.Length <= (int)cruciballManager.currentClass)
				{
					Debug.LogError("Missing win silhouette for current class");
				}
				else
				{
					classImages[(int)cruciballManager.currentClass].SetActive(value: true);
				}
			}
			else if ((!cruciballManager.VictoryThisRun || !StaticGameData.CurrentRunStats.hasWonCore) && mapNamesDict.TryGetValue(StaticGameData.battleVictorySceneOverride, out value))
			{
				winScreen.SetActive(value: false);
				coreWinScreen.SetActive(value: false);
				value.SetActive(value: true);
			}
		}
	}
}
