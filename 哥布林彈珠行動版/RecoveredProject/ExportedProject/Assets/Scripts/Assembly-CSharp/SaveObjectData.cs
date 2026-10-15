using ToolBox.Serialization;
using UnityEngine;

public abstract class SaveObjectData : ISerializable
{
	[SerializeField]
	private bool _deleteOnLose;

	[SerializeField]
	private int _saveType;

	public abstract string Name { get; }

	public bool DeleteOnLose => _deleteOnLose;

	public SaveObjectData(bool deleteOnLose, DataSerializer.SaveType saveType)
	{
		_deleteOnLose = deleteOnLose;
		_saveType = (int)saveType;
	}

	public void Save()
	{
		DataSerializer.Save(Name, this, (DataSerializer.SaveType)_saveType);
	}

	public void Delete()
	{
		DataSerializer.DeleteKey(Name, (DataSerializer.SaveType)_saveType);
	}
}
