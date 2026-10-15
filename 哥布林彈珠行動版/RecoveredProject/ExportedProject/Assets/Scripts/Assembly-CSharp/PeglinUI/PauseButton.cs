using UnityEngine;
using UnityEngine.UI;

namespace PeglinUI
{
	public class PauseButton : MonoBehaviour
	{
		private Button _button;

		private PauseMenu _pauseMenu;

		private void OnEnable()
		{
			if (_button == null)
			{
				_button = GetComponent<Button>();
			}
			if (_pauseMenu == null)
			{
				_pauseMenu = Object.FindObjectOfType<PauseMenu>();
			}
			_button.onClick.AddListener(Pause);
		}

		private void Pause()
		{
			if (PauseMenu.Paused)
			{
				_pauseMenu.Resume();
			}
			else
			{
				_pauseMenu.Pause();
			}
		}
	}
}
