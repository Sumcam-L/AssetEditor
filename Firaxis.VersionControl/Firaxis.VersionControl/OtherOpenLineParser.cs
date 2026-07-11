namespace Firaxis.VersionControl;

internal class OtherOpenLineParser : BaseOtherLineParser
{
	public OtherOpenLineParser()
		: base("otherOpen")
	{
	}

	protected override bool DoParseStatusLine(int otherNo, string val, ref FileStatusResultCode itemResults)
	{
		itemResults.Status.Others[otherNo].Owner = val;
		return true;
	}
}
