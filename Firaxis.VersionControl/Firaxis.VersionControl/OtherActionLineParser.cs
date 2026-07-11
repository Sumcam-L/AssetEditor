using System;

namespace Firaxis.VersionControl;

internal class OtherActionLineParser : BaseOtherLineParser
{
	public OtherActionLineParser()
		: base("otherAction")
	{
	}

	protected override bool DoParseStatusLine(int otherNo, string val, ref FileStatusResultCode itemResults)
	{
		if (!Enum.TryParse<VersionControlActionType>(val, ignoreCase: true, out var result))
		{
			return false;
		}
		itemResults.Status.Others[otherNo].Action = result;
		return true;
	}
}
