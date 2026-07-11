using Firaxis.Error;

namespace Firaxis.VersionControl;

public class ItemResultCode
{
	public VersionControlPath File { get; internal set; }

	public ResultCode Result { get; internal set; }

	public ItemResultCode()
	{
		File = new VersionControlPath(string.Empty, string.Empty);
		Result = ResultCode.Success;
	}

	public ItemResultCode(VersionControlPath f, ResultCode r)
	{
		File = f;
		Result = r;
	}
}
