using System;
using System.Text.RegularExpressions;

namespace Firaxis.VersionControl;

internal class HeadActionLineParser : BaseLineParser
{
	public HeadActionLineParser()
		: base("^\\.\\.\\.\\s+headAction\\s+(.+)$")
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
		VersionControlActionType result = VersionControlActionType.MoveAdd;
		if (!Enum.TryParse<VersionControlActionType>(matchCollection[0].Groups[1].Value, ignoreCase: true, out result) && matchCollection[0].Groups[1].Value != "move/add")
		{
			return false;
		}
		itemResults.Status.Head.Action = result;
		return true;
	}
}
