using System.Text.RegularExpressions;

namespace Firaxis.VersionControl;

internal class HeadModTimeLineParser : BaseLineParser
{
	public HeadModTimeLineParser()
		: base("^\\.\\.\\.\\s+headModTime\\s+(\\d+)$")
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
		if (!double.TryParse(matchCollection[0].Groups[1].Value, out var result))
		{
			return false;
		}
		itemResults.Status.Head.Modified = TimeUtils.UnixTimeStampToDateTime(result);
		return true;
	}
}
