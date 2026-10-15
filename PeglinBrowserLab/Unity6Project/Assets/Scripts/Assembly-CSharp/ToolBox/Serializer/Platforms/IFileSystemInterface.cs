namespace ToolBox.Serializer.Platforms
{
	public interface IFileSystemInterface
	{
		void SaveFile(string filepath, byte[] bytes);

		bool LoadFile(string filepath, ref byte[] bytes);

		void CreateBackupSave(string filepath);

		void RestoreBackupSave(string filepath);

		void DeleteFile(string filepath);
	}
}
