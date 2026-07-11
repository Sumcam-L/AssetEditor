namespace Firaxis.VersionControl;

internal class IgnoreLineParser : BaseLineParser
{
	public IgnoreLineParser(string rex)
		: base(rex)
	{
	}

	public override bool ParseStatusLine(IVersionControlWorkspace workspace, string line, ref FileStatusResultCode itemResults)
	{
		return true;
	}
}
