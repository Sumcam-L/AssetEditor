using System.Text.RegularExpressions;

namespace Firaxis.VersionControl;

internal abstract class BaseOtherLineParser : BaseLineParser
{
	public BaseOtherLineParser(string otherType)
		: base($"^\\.\\.\\.\\s+\\.\\.\\.\\s+{otherType}(\\d+)\\s+(.+)$")
	{
	}

	public sealed override bool ParseStatusLine(IVersionControlWorkspace workspace, string line, ref FileStatusResultCode itemResults)
	{
		MatchCollection matchCollection = Matcher.Matches(line);
		if (matchCollection.Count == 0)
		{
			return false;
		}
		if (matchCollection[0].Groups.Count != 3)
		{
			return false;
		}
		string s = (itemResults.Status.Working.Owner = matchCollection[0].Groups[1].Value);
		if (!int.TryParse(s, out var result))
		{
			return false;
		}
		if (!itemResults.Status.Others.ContainsKey(result))
		{
			itemResults.Status.Others[result] = new VersionControlAction();
		}
		return DoParseStatusLine(result, matchCollection[0].Groups[2].Value, ref itemResults);
	}

	protected abstract bool DoParseStatusLine(int otherNo, string val, ref FileStatusResultCode itemResults);
}
