using System.Text.RegularExpressions;

namespace Firaxis.VersionControl;

internal class HeadTypeLineParser : BaseLineParser
{
	public HeadTypeLineParser()
		: base("^\\.\\.\\.\\s+headType\\s+(.+)$")
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
		itemResults.Status.Head.FileType = matchCollection[0].Groups[1].Value;
		return true;
	}
}
