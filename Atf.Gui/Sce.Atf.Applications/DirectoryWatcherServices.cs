namespace Sce.Atf.Applications;

public static class DirectoryWatcherServices
{
	public static void Register(this IDirectoryWatcherService service, string directory)
	{
		service.Register(directory, new string[1] { "*.*" }, includeSubdirectories: false);
	}

	public static void Register(this IDirectoryWatcherService service, string directory, bool includeSubdirectories)
	{
		service.Register(directory, new string[1] { "*.*" }, includeSubdirectories);
	}
}
