namespace Firaxis.VersionControl;

internal class OtherChangeLineParser : BaseOtherLineParser
{
	public OtherChangeLineParser()
		: base("otherChange")
	{
	}

	protected override bool DoParseStatusLine(int otherNo, string val, ref FileStatusResultCode itemResults)
	{
		if (val == "default")
		{
			itemResults.Status.Head.Change = 0;
			return true;
		}
		if (!int.TryParse(val, out var result))
		{
			return false;
		}
		itemResults.Status.Others[otherNo].Change = result;
		return true;
	}
}
