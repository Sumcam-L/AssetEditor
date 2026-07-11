using System;

namespace Firaxis.VersionControl;

public class VersionControlAction
{
	public int Change { get; internal set; }

	public VersionControlActionType Action { get; internal set; }

	public string FileType { get; internal set; }

	public int Revision { get; internal set; }

	public DateTime Modified { get; internal set; }

	public string Owner { get; internal set; }

	public VersionControlAction()
	{
		Change = 0;
		Action = VersionControlActionType.None;
		FileType = string.Empty;
		Revision = -1;
	}
}
