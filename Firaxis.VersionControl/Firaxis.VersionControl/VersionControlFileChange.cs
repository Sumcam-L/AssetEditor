namespace Firaxis.VersionControl;

public struct VersionControlFileChange
{
	public readonly VersionControlChangeType Change;

	public readonly VersionControlPath Path;

	public VersionControlFileChange(VersionControlChangeType change, VersionControlPath path)
	{
		Change = change;
		Path = path;
	}
}
