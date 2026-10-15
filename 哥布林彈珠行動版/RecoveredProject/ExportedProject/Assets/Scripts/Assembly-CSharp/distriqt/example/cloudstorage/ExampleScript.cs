using UnityEngine;
using UnityEngine.UI;
using distriqt.plugins.cloudstorage;

namespace distriqt.example.cloudstorage
{
	public class ExampleScript : MonoBehaviour
	{
		public GameObject stateTextObject;

		public GameObject logTextObject;

		private Text stateText;

		private Text logText;

		private int count;

		private void Start()
		{
			if (stateTextObject != null)
			{
				stateText = stateTextObject.GetComponent<Text>();
			}
			if (logTextObject != null)
			{
				logText = logTextObject.GetComponent<Text>();
				logText.text = "";
			}
			UpdateState("CloudStorage.isSupported: " + CloudStorage.isSupported + "\nCloudStorage.version: " + CloudStorage.Instance.Version());
			if (CloudStorage.isSupported)
			{
				CloudStorage.Instance.OnInitialSync += Instance_OnInitialSync;
				CloudStorage.Instance.OnChanged += Instance_OnChanged;
				CloudStorage.Instance.Initialise();
				CloudStorage.Instance.SetString("userId", "some_value");
				count = CloudStorage.Instance.GetInt("count");
				Log("CloudStorage.getString(): " + CloudStorage.Instance.GetString("userId"));
				InvokeRepeating("UpdateCount", 0f, 1f);
			}
		}

		private void Instance_OnChanged(KeyValueStoreEvent e)
		{
			UpdateState("Changed");
			Log("OnChanged");
			foreach (string changedKey in e.changedKeys)
			{
				Log("OnChanged: " + changedKey);
			}
		}

		private void Instance_OnInitialSync(KeyValueStoreEvent e)
		{
			UpdateState("Initial Sync Received");
			Log("OnInitialSync");
		}

		private void Update()
		{
		}

		private void UpdateCount()
		{
			if (CloudStorage.isSupported)
			{
				count = CloudStorage.Instance.GetInt("count");
				Log("Updating Count: " + count);
				count++;
				CloudStorage.Instance.SetInt("count", count);
				CloudStorage.Instance.SetString("trigger", "value_" + count);
			}
		}

		private void UpdateState(string state)
		{
			if (stateText != null)
			{
				stateText.text = state;
			}
		}

		private void Log(string message)
		{
			if (logText != null)
			{
				logText.text = message + "\n" + logText.text;
			}
			Debug.Log(message);
		}
	}
}
