using System;
using ToolBox.Serialization;

public interface ICloudSaveManager
{
	void LoadSave(Action onLoadComplete);

	void UploadCloudSave(DataSerializer.SaveType saveType);

	void DeleteCloudSave(DataSerializer.SaveType saveType);
}
