using Peglin.ClassSystem;
using Saving;
using UnityEngine;

namespace PeglinUI.MainMenu
{
	public class CharacterXpDisplayManager : MonoBehaviour
	{
		public int BalladinStagingProgress;

		public int RoundrelStagingProgress;

		public int SpinventorStagingProgress;

		[Header("Scene refs")]
		public XpDisplay XpDisplay;

		private void OnEnable()
		{
		}

		public void SetupXpInfo(Class cl)
		{
			switch (cl)
			{
			case Class.Balladin:
				XpDisplay.SetupXpBar(PersistentPlayerData.Instance.BalladinProgress - BalladinStagingProgress, BalladinStagingProgress, PersistentPlayerData.BALLADIN_PROGRESS_MAX);
				BalladinStagingProgress = 0;
				break;
			case Class.Roundrel:
				XpDisplay.SetupXpBar(PersistentPlayerData.Instance.RoundrelProgress - RoundrelStagingProgress, RoundrelStagingProgress, PersistentPlayerData.ROUNDREL_PROGRESS_MAX);
				RoundrelStagingProgress = 0;
				break;
			case Class.Spinventor:
				XpDisplay.SetupXpBar(PersistentPlayerData.Instance.SpinventorProgress - SpinventorStagingProgress, SpinventorStagingProgress, PersistentPlayerData.SPINVENTOR_PROGRESS_MAX);
				SpinventorStagingProgress = 0;
				break;
			}
		}
	}
}
