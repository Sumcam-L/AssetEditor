using System.Collections.Generic;
using System.IO;

namespace Sce.Atf.Applications;

public interface IDirectoryWatcherService
{
	event FileSystemEventHandler FileChanged;

	void Register(string directory, IEnumerable<string> extensions, bool includeSubdirectories);

	void Unregister(string directory);
}
