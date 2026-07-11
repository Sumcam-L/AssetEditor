namespace Firaxis.CivTech;

public class VersionControlInfo
{
	public string Uri { get; set; }

	public string Workspace { get; set; }

	public VersionControlInfo()
	{
		Uri = string.Empty;
		Workspace = string.Empty;
	}

	public VersionControlInfo(string name, string folder)
	{
		Uri = $"local://{name}={folder}";
		Workspace = name;
	}

	public VersionControlInfo(string server, string user, string workspace)
	{
		Uri = $"perforce://{user}@{server}";
		Workspace = workspace;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is VersionControlInfo versionControlInfo))
		{
			return false;
		}
		return versionControlInfo.Uri.Equals(Uri) && versionControlInfo.Workspace.Equals(Workspace);
	}

	public override int GetHashCode()
	{
		return Uri.GetHashCode() ^ Workspace.GetHashCode();
	}
}
