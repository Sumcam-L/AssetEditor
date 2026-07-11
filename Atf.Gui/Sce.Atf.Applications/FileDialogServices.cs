namespace Sce.Atf.Applications;

public static class FileDialogServices
{
	public static FileDialogResult OpenFileName(this IFileDialogService service, ref string pathName, string filter, string directory)
	{
		string forcedInitialDirectory = service.ForcedInitialDirectory;
		try
		{
			service.ForcedInitialDirectory = directory;
			return service.OpenFileName(ref pathName, filter);
		}
		finally
		{
			service.ForcedInitialDirectory = forcedInitialDirectory;
		}
	}

	public static FileDialogResult OpenFileNames(this IFileDialogService service, ref string[] pathNames, string filter, string directory)
	{
		string forcedInitialDirectory = service.ForcedInitialDirectory;
		try
		{
			service.ForcedInitialDirectory = directory;
			return service.OpenFileNames(ref pathNames, filter);
		}
		finally
		{
			service.ForcedInitialDirectory = forcedInitialDirectory;
		}
	}

	public static FileDialogResult SaveFileName(this IFileDialogService service, ref string pathName, string filter, string directory)
	{
		string forcedInitialDirectory = service.ForcedInitialDirectory;
		try
		{
			service.ForcedInitialDirectory = directory;
			return service.SaveFileName(ref pathName, filter);
		}
		finally
		{
			service.ForcedInitialDirectory = forcedInitialDirectory;
		}
	}
}
