using Rewired;
using UnityEngine;

namespace PixelCrushers.RewiredSupport
{
	[AddComponentMenu("Pixel Crushers/Third Party/Rewired Support/Input Device Manager Rewired")]
	public class InputDeviceManagerRewired : MonoBehaviour
	{
		[Tooltip("Rewired player ID.")]
		public int playerId;

		private Player m_player;

		private void Start()
		{
			if (InputDeviceManager.instance == null)
			{
				Debug.LogWarning("The scene is missing an Input Device Manager. Can't set up InputDeviceManagerRewired.", this);
			}
			InputDeviceManager.instance.GetButtonDown = RewiredGetButtonDown;
			InputDeviceManager.instance.GetInputAxis = RewiredGetAxis;
			m_player = ReInput.players.GetPlayer(playerId);
			if (m_player == null)
			{
				Debug.LogWarning("Didn't find a Rewired player #" + playerId, this);
			}
		}

		public bool RewiredGetButtonDown(string buttonName)
		{
			if (m_player == null)
			{
				return Input.GetButtonDown(buttonName);
			}
			if (!m_player.GetButtonDown(buttonName))
			{
				return m_player.GetNegativeButtonDown(buttonName);
			}
			return true;
		}

		public float RewiredGetAxis(string axisName)
		{
			if (m_player == null)
			{
				return Input.GetAxis(axisName);
			}
			return m_player.GetAxis(axisName);
		}
	}
}
