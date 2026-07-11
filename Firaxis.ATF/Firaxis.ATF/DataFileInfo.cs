using System;

namespace Firaxis.ATF;

public class DataFileInfo
{
	public readonly string ID;

	public readonly string RelativePath;

	public readonly Uri FullPath;

	public DataFileInfo(string id, string relPath, Uri fullPath)
	{
		ID = id;
		RelativePath = relPath;
		FullPath = fullPath;
	}
}
