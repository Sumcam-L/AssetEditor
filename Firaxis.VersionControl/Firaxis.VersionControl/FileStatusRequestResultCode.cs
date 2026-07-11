using System.Collections.Generic;
using Firaxis.Error;
using Firaxis.Utility;

namespace Firaxis.VersionControl;

public class FileStatusRequestResultCode
{
	private static PathComparer s_comparer = new PathComparer();

	public ResultCode Result { get; private set; }

	public IDictionary<string, FileStatusResultCode> Status { get; private set; }

	public FileStatusRequestResultCode(ResultCode result)
	{
		Result = result;
		Status = new Dictionary<string, FileStatusResultCode>(s_comparer);
	}

	public FileStatusRequestResultCode(IDictionary<string, FileStatusResultCode> status)
	{
		Result = PerforceRequestHelper.DetermineOverallResult(status.Values);
		Status = status;
	}

	public FileStatusRequestResultCode(ResultCode result, IDictionary<string, FileStatusResultCode> status)
	{
		Result = result;
		Status = status;
	}
}
