using System;

namespace Firaxis.ATF;

public class FileChangedEventArgs : EventArgs
{
	public readonly string FilePath;

	public FileChangedEventArgs(string path)
	{
		FilePath = path;
	}
}
