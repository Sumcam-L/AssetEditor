using System;
using System.IO;

namespace Firaxis.VersionControl;

public class VersionControlPath : IComparable<VersionControlPath>, IEquatable<VersionControlPath>
{
	public string WorkspacePath { get; private set; }

	public string DepotPath { get; private set; }

	public VersionControlPath(string depotPath, string localPath)
	{
		DepotPath = depotPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		WorkspacePath = localPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
	}

	public int CompareTo(VersionControlPath other)
	{
		bool ignoreCase = true;
		return string.Compare(DepotPath, other.DepotPath, ignoreCase);
	}

	public bool Equals(VersionControlPath other)
	{
		return DepotPath.Equals(other.DepotPath, StringComparison.CurrentCultureIgnoreCase);
	}
}
