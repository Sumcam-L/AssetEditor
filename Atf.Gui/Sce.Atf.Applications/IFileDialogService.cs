namespace Sce.Atf.Applications;

public interface IFileDialogService
{
	string InitialDirectory { get; set; }

	string ForcedInitialDirectory { get; set; }

	FileDialogResult OpenFileName(ref string pathName, string filter);

	FileDialogResult OpenFileNames(ref string[] pathNames, string filter);

	FileDialogResult SaveFileName(ref string pathName, string filter);

	FileDialogResult ConfirmFileClose(string message);

	bool PathExists(string pathName);
}
