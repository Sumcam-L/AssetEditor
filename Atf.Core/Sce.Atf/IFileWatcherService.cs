using System.IO;

namespace Sce.Atf;

public interface IFileWatcherService
{
	bool UseSinglePantryMode { get; set; }

	event FileSystemEventHandler FileChanged;

	void Register(string filePath);

	void Unregister(string filePath);

	void Suspend(string filePath);

	void Unsuspend(string filePath);
}
