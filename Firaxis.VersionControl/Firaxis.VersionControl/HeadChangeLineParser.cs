using System.Text.RegularExpressions;

namespace Firaxis.VersionControl;

internal class HeadChangeLineParser : BaseLineParser
{
	public HeadChangeLineParser()
		: base("^\\.\\.\\.\\s+headChange\\s+(.+)$")
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
		if (matchCollection[0].Groups[1].Value == "default")
		{
			itemResults.Status.Head.Change = 0;
			return true;
		}
		if (!int.TryParse(matchCollection[0].Groups[1].Value, out var result))
		{
			return false;
		}
		itemResults.Status.Head.Change = result;
		return true;
	}
}
