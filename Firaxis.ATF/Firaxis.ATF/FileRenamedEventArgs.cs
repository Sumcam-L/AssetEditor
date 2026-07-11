namespace Firaxis.ATF;

public class FileRenamedEventArgs : FileChangedEventArgs
{
	public readonly string OldPath;

	public FileRenamedEventArgs(string path, string oldPath)
		: base(path)
	{
		OldPath = oldPath;
	}
}
