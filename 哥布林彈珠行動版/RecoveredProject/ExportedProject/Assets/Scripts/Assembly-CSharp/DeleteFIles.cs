using System.IO;
using ToolBox.Serialization;
using UnityEngine;

public class DeleteFIles : MonoBehaviour
{
	public void DeleteAllFilesManually()
	{
		File.Delete(DataSerializer.GetFilePath(DataSerializer.CurrentProfileIndex, DataSerializer.SaveType.RUN));
	}
}
