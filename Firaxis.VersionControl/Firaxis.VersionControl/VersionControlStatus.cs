using System.Collections.Generic;

namespace Firaxis.VersionControl;

public class VersionControlStatus
{
	public VersionControlAction Head { get; internal set; }

	public VersionControlAction Working { get; internal set; }

	public IDictionary<int, VersionControlAction> Others { get; internal set; }

	public int LocalRevision { get; internal set; }

	public VersionControlStatus()
	{
		Head = new VersionControlAction();
		Working = new VersionControlAction();
		Others = new Dictionary<int, VersionControlAction>();
		LocalRevision = -1;
	}
}
