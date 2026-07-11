using System.Text.RegularExpressions;

namespace Firaxis.VersionControl;

internal class HaveRevisionLineParser : BaseLineParser
{
	public HaveRevisionLineParser()
		: base("^\\.\\.\\.\\s+haveRev\\s+(\\d+)$")
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
		if (!int.TryParse(matchCollection[0].Groups[1].Value, out var result))
		{
			return false;
		}
		itemResults.Status.LocalRevision = result;
		return true;
	}
}
