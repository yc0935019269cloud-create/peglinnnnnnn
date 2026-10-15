using System;
using UnityEngine;

namespace distriqt.plugins.cloudstorage
{
	public class CloudStorage : MonoBehaviour
	{
		[Serializable]
		private class EventData
		{
			public string code;

			public string data;
		}

		public delegate void KeyValueStoreEventHandler(KeyValueStoreEvent e);

		private const string version = "1.0.3";

		private const string MISSING_IMPLEMENTATION_ERROR_MESSAGE = "Check you have correctly included the library for this platform";

		private static bool _create;

		private static CloudStorage _instance;

		public static CloudStorage Instance
		{
			get
			{
				if (_instance == null)
				{
					_create = true;
					_instance = new GameObject().AddComponent<CloudStorage>();
					_instance.name = "CloudStorage";
				}
				return _instance;
			}
		}

		public static bool isSupported
		{
			get
			{
				try
				{
					platformSupported();
				}
				catch (Exception innerException)
				{
					throw new Exception("Check you have correctly included the library for this platform", innerException);
				}
				return false;
			}
		}

		public event KeyValueStoreEventHandler OnChanged;

		public event KeyValueStoreEventHandler OnOverQuota;

		public event KeyValueStoreEventHandler OnAccountChanged;

		public event KeyValueStoreEventHandler OnInitialSync;

		static CloudStorage()
		{
		}

		private CloudStorage()
		{
		}

		private static bool platformSupported()
		{
			return false;
		}

		public string Version()
		{
			return "1.0.3";
		}

		public string NativeVersion()
		{
			try
			{
				platformSupported();
			}
			catch (EntryPointNotFoundException innerException)
			{
				throw new Exception("Check you have correctly included the library for this platform", innerException);
			}
			return "0";
		}

		public string Implementation()
		{
			try
			{
				platformSupported();
			}
			catch (EntryPointNotFoundException innerException)
			{
				throw new Exception("Check you have correctly included the library for this platform", innerException);
			}
			return "default";
		}

		public void Initialise()
		{
			try
			{
				platformSupported();
			}
			catch (EntryPointNotFoundException innerException)
			{
				throw new Exception("Check you have correctly included the library for this platform", innerException);
			}
		}

		public bool HasKey(string key)
		{
			try
			{
				platformSupported();
			}
			catch (EntryPointNotFoundException innerException)
			{
				throw new Exception("Check you have correctly included the library for this platform", innerException);
			}
			return false;
		}

		public string[] getKeys()
		{
			try
			{
				if (platformSupported())
				{
					return "".Split(',');
				}
			}
			catch (EntryPointNotFoundException innerException)
			{
				throw new Exception("Check you have correctly included the library for this platform", innerException);
			}
			return new string[0];
		}

		public void SetString(string key, string value)
		{
			try
			{
				platformSupported();
			}
			catch (EntryPointNotFoundException innerException)
			{
				throw new Exception("Check you have correctly included the library for this platform", innerException);
			}
		}

		public string GetString(string key)
		{
			try
			{
				platformSupported();
			}
			catch (EntryPointNotFoundException innerException)
			{
				throw new Exception("Check you have correctly included the library for this platform", innerException);
			}
			return null;
		}

		public void SetInt(string key, int value)
		{
			try
			{
				platformSupported();
			}
			catch (EntryPointNotFoundException innerException)
			{
				throw new Exception("Check you have correctly included the library for this platform", innerException);
			}
		}

		public int GetInt(string key)
		{
			try
			{
				platformSupported();
			}
			catch (EntryPointNotFoundException innerException)
			{
				throw new Exception("Check you have correctly included the library for this platform", innerException);
			}
			return 0;
		}

		public void Remove(string key)
		{
			try
			{
				platformSupported();
			}
			catch (EntryPointNotFoundException innerException)
			{
				throw new Exception("Check you have correctly included the library for this platform", innerException);
			}
		}

		public void RemoveAll()
		{
			try
			{
				platformSupported();
			}
			catch (EntryPointNotFoundException innerException)
			{
				throw new Exception("Check you have correctly included the library for this platform", innerException);
			}
		}

		public bool Synchronise()
		{
			try
			{
				platformSupported();
			}
			catch (EntryPointNotFoundException innerException)
			{
				throw new Exception("Check you have correctly included the library for this platform", innerException);
			}
			return false;
		}

		public void Dispatch(string message)
		{
			Debug.Log("============================");
			try
			{
				EventData eventData = JsonUtility.FromJson<EventData>(message);
				Debug.Log(eventData.code);
				Debug.Log(eventData.data);
				switch (eventData.code)
				{
				case "keyvaluestore:accountchanged":
					if (OnAccountChanged != null)
					{
						OnAccountChanged(null);
					}
					break;
				case "keyvaluestore:changed":
				{
					KeyValueStoreEvent keyValueStoreEvent = JsonUtility.FromJson<KeyValueStoreEvent>(eventData.data);
					foreach (string changedKey in keyValueStoreEvent.changedKeys)
					{
						Debug.Log(changedKey);
					}
					if (OnChanged != null)
					{
						OnChanged(keyValueStoreEvent);
					}
					break;
				}
				case "keyvaluestore:initialsync":
					if (OnInitialSync != null)
					{
						OnInitialSync(null);
					}
					break;
				case "keyvaluestore:overquota":
					if (OnOverQuota != null)
					{
						OnOverQuota(null);
					}
					break;
				}
			}
			catch (Exception message2)
			{
				Debug.Log(message2);
			}
			Debug.Log("============================");
		}

		public void Awake()
		{
			Debug.Log("CloudStorage::Awake: " + _create);
			if (_create)
			{
				_create = false;
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		public void OnDestroy()
		{
			Debug.Log("CloudStorage::OnDestroy");
		}
	}
}
