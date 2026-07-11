using Firaxis.Error;

namespace Firaxis.VersionControl;

public class PerforceResultCode : ResultCode
{
	public static readonly ResultCode PartialSuccess;

	public static readonly ResultCode NotSignedIn;

	public static readonly ResultCode Timeout;

	public static readonly ResultCode FailedToConnect;

	static PerforceResultCode()
	{
		PartialSuccess = new ResultCode("Partial Success");
		NotSignedIn = new ResultCode("Not signed in");
		Timeout = new ResultCode("Operation timed out");
		FailedToConnect = new ResultCode("Failed to connect");
	}

	public PerforceResultCode(string msg)
		: base(msg)
	{
	}
}
