using System;
using System.Text.RegularExpressions;

namespace Firaxis.VersionControl;

internal class LocalActionLineParser : BaseLineParser
{
	public LocalActionLineParser()
		: base("^\\.\\.\\.\\s+action\\s+(.+)$")
	{
	}

	public override bool ParseStatusLine(IVersionControlWorkspace workspace, string line, ref FileStatusResultCode itemResults)
	{
		MatchCollection matchCollection = Matcher.Matches(line);
		if (matchCollection.Count == 0)
		{
			return false;
		}
		if (matchCollection[0].Groups.Count != 2)
		{
			return false;
		}
		if (!Enum.TryParse<VersionControlActionType>(matchCollection[0].Groups[1].Value, ignoreCase: true, out var result))
		{
			return false;
		}
		itemResults.Status.Working.Action = result;
		return true;
	}
}
