using System.Text.RegularExpressions;

namespace Firaxis.VersionControl;

internal abstract class BaseLineParser : IFileStatusParser
{
	protected readonly Regex Matcher;

	public BaseLineParser(string rex)
	{
		Matcher = new Regex(rex, RegexOptions.Compiled);
	}

	public bool MatchesLine(string line)
	{
		return Matcher.IsMatch(line);
	}

	public abstract bool ParseStatusLine(IVersionControlWorkspace workspace, string line, ref FileStatusResultCode itemResults);
}
