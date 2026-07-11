using System.Collections.Generic;
using Firaxis.Error;

namespace Firaxis.VersionControl;

public class RequestResultCode
{
	public ResultCode Result { get; private set; }

	public IEnumerable<ItemResultCode> ItemResults { get; private set; }

	public RequestResultCode(IEnumerable<ItemResultCode> itemResults)
	{
		Result = PerforceRequestHelper.DetermineOverallResult(itemResults);
		ItemResults = itemResults;
	}

	public RequestResultCode(ResultCode overallResult, IEnumerable<ItemResultCode> itemResults)
	{
		Result = overallResult;
		ItemResults = itemResults;
	}
}
