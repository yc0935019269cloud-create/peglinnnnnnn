using System.IO;
using UnityEngine;

namespace ToolBox.Serializer.Platforms
{
	public class StandardFileSystemInterface : IFileSystemInterface
	{
		public void SaveFile(string filepath, byte[] bytes)
		{
			File.WriteAllBytes(filepath, bytes);
		}

		public bool LoadFile(string filepath, ref byte[] bytes)
		{
			if (!File.Exists(filepath))
			{
				File.Create(filepath)?.Close();
			}
			bytes = File.ReadAllBytes(filepath);
			return true;
		}

		public void CreateBackupSave(string filepath)
		{
			Debug.Log("Backup save called");
			File.Copy(filepath, filepath + ".bak", overwrite: true);
		}

		public void RestoreBackupSave(string filepath)
		{
			if (File.Exists(filepath + ".bak"))
			{
				File.Copy(filepath + ".bak", filepath, overwrite: true);
				return;
			}
			Debug.LogWarning("Found no backup save. We're forced to delete the base file and start over");
			File.Delete(filepath);
		}

		public void DeleteFile(string filepath)
		{
			File.Delete(filepath);
		}
	}
}
