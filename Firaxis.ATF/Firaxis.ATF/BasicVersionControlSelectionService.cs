using Firaxis.CivTech;

namespace Firaxis.ATF;

public class BasicVersionControlSelectionService : VersionControlSelectionServiceBase
{
	public BasicVersionControlSelectionService(string[] projName, string p4user, string p4port, string p4client)
	{
		VersionControlInfo value = new VersionControlInfo
		{
			Uri = "perforce://" + p4user + "@" + p4port,
			Workspace = p4client
		};
		foreach (string key in projName)
		{
			VersionControlInfoMap[key] = value;
		}
	}
}
