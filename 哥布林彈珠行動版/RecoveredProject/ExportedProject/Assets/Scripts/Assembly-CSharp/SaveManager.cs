using System;
using Loading;
using Saving;
using ToolBox.Serialization;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
	public delegate void SaveRequested();

	public delegate void LoadRequested();

	public delegate void DeleteRequested();

	public class SaveFileCorruptionDetector
	{
		private static SaveFileCorruptionDetector _instance;

		public bool SaveFileHasBeenCorrupted;

		public bool RunFileHasBeenCorrupted;

		public bool CloudSavesDisabled;

		public static SaveFileCorruptionDetector Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new SaveFileCorruptionDetector();
				}
				return _instance;
			}
		}
	}

	private class SaveVersion : SaveObjectData
	{
		[SerializeField]
		private int _majorVersion;

		[SerializeField]
		private int _minorVersion;

		public override string Name => SaveVersionKey;

		public int MajorVersion => _majorVersion;

		public int MinorVersion => _minorVersion;

		public SaveVersion(int majorVersion, int minorVersion)
			: base(deleteOnLose: false, DataSerializer.SaveType.BASE)
		{
			_majorVersion = majorVersion;
			_minorVersion = minorVersion;
		}
	}

	public static readonly int VersionMajor = 1;

	public static readonly int VersionMinor = 3;

	public static readonly string SaveVersionKey = "SAVE_VERSION";

	public static SaveRequested OnSaveRequested;

	public static LoadRequested OnLoadRequested;

	public static DeleteRequested OnDeleteRequested;

	private static SaveManager _instance;

	public static SaveManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new GameObject("SaveManager").AddComponent<SaveManager>();
				_instance.CheckSaveVersion();
			}
			return _instance;
		}
	}

	public void Awake()
	{
		CheckSaveVersion();
		OnDeleteRequested = (DeleteRequested)Delegate.Combine(OnDeleteRequested, new DeleteRequested(DeleteRunData));
	}

	public bool HasRunSaveData()
	{
		return DataSerializer.HasKey("MapController", DataSerializer.SaveType.RUN);
	}

	private void CheckSaveVersion()
	{
		Version version = new Version(VersionMajor, VersionMinor);
		SaveVersion saveVersion = (SaveVersion)DataSerializer.Load<SaveObjectData>(SaveVersionKey, DataSerializer.SaveType.BASE);
		if (saveVersion != null)
		{
			Version value = new Version(saveVersion.MajorVersion, saveVersion.MinorVersion);
			switch (version.CompareTo(value))
			{
			case -1:
				Debug.LogError("We've found save data with a future save version. We're deleting just in case, but the game will break");
				DeleteRunData();
				SaveCurrentVersion();
				break;
			case 1:
				Debug.Log("Found save data of an earlier version. Running migration...");
				MigrationManager.CheckAndRunMigrations(version);
				SaveCurrentVersion();
				break;
			case 0:
				break;
			}
		}
		else
		{
			Debug.Log("Found no save version data, so creating new one...");
			DeleteRunData();
			SaveCurrentVersion();
		}
	}

	private static void SaveCurrentVersion()
	{
		DataSerializer.Save(SaveVersionKey, (SaveObjectData)new SaveVersion(VersionMajor, VersionMinor), DataSerializer.SaveType.BASE);
		DataSerializer.Save(SaveVersionKey, (SaveObjectData)new SaveVersion(VersionMajor, VersionMinor), DataSerializer.SaveType.RUN);
		SaveBase();
		SaveRun();
	}

	public static void SaveRun()
	{
		DataSerializer.SaveFile(DataSerializer.SaveType.RUN);
	}

	public static void SaveBase()
	{
		DataSerializer.SaveFile(DataSerializer.SaveType.BASE);
	}

	public static void SaveStats()
	{
		DataSerializer.SaveFile(DataSerializer.SaveType.STATS);
	}

	private void DeleteRunData()
	{
		if (StaticGameData.CurrentRunStats != null)
		{
			StaticGameData.CurrentRunStats.endDate = DateTime.Now;
			AssetLoading.Instance.AddRunStats(StaticGameData.CurrentRunStats);
		}
		SaveObjectData[] array = DataSerializer.FetchAllData<SaveObjectData>(DataSerializer.SaveType.RUN);
		foreach (SaveObjectData saveObjectData in array)
		{
			if (saveObjectData != null && saveObjectData.DeleteOnLose)
			{
				saveObjectData.Delete();
			}
		}
		SaveRun();
	}

	public static void RequestSave()
	{
		OnSaveRequested?.Invoke();
	}

	public static void RequestLoad()
	{
		OnLoadRequested?.Invoke();
	}

	public static void RequestDelete()
	{
		OnDeleteRequested?.Invoke();
	}

	public void OnDisable()
	{
		OnDeleteRequested = (DeleteRequested)Delegate.Remove(OnDeleteRequested, new DeleteRequested(DeleteRunData));
	}
}
