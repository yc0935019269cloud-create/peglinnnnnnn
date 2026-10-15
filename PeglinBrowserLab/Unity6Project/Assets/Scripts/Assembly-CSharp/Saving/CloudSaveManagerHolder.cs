using UnityEngine;

namespace Saving
{
	public class CloudSaveManagerHolder : MonoBehaviour
	{
		private ICloudSaveManager _saveManager;

		private static CloudSaveManagerHolder _instance;

		public ICloudSaveManager SaveManager => _saveManager;

		public static CloudSaveManagerHolder Instance => _instance;

		private void OnEnable()
		{
			if (_instance == null)
			{
				_instance = this;
				Object.DontDestroyOnLoad(this);
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}

		public void SetSaveManager(ICloudSaveManager manager)
		{
			_saveManager = manager;
		}
	}
}
