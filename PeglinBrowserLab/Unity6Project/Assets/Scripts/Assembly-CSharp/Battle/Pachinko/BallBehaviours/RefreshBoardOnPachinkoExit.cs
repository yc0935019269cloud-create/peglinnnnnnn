using PeglinUI.MainMenu;
using UnityEngine;

namespace Battle.Pachinko.BallBehaviours
{
	public class RefreshBoardOnPachinkoExit : MonoBehaviour
	{
		private BattleController _battleController;

		private PachinkoBall _pachinkoBall;

		private void Awake()
		{
			_battleController = Object.FindObjectOfType<BattleController>();
		}

		public void RequestRefresh()
		{
			if (_battleController != null)
			{
				_battleController.ResetField();
			}
			else
			{
				MainMenuRandomOrbDrop.RequestBoardReset?.Invoke();
			}
		}
	}
}
