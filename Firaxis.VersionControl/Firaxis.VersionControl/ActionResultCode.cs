using System.Collections.Generic;
using Firaxis.Error;

namespace Firaxis.VersionControl;

public class ActionResultCode : ItemResultCode
{
	public int Revision { get; internal set; }

	public IList<string> AdditionalInfo { get; private set; }

	public ActionResultCode()
	{
		AdditionalInfo = new List<string>();
	}

	public ActionResultCode(VersionControlPath f, ResultCode r, int revision)
		: base(f, r)
	{
		Revision = revision;
		AdditionalInfo = new List<string>();
	}
}
