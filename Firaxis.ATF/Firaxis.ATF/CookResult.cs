using System.Collections.Generic;
using System.Linq;
using Firaxis.Error;

namespace Firaxis.ATF;

public class CookResult
{
	public readonly ResultCode Result;

	public readonly IEnumerable<CookItemResultCode> ItemResults;

	public CookResult(IEnumerable<CookItemResultCode> itemRes)
	{
		Result = DetermineOverallResult(itemRes);
		ItemResults = itemRes;
	}

	public CookResult(ResultCode res, IEnumerable<CookItemResultCode> itemRes)
	{
		Result = res;
		ItemResults = itemRes;
	}

	public CookResult(string errMsg)
		: this(new ResultCode(errMsg), Enumerable.Empty<CookItemResultCode>())
	{
	}

	private static ResultCode DetermineOverallResult(IEnumerable<CookItemResultCode> itemRes)
	{
		int num = 0;
		foreach (CookItemResultCode itemRe in itemRes)
		{
			if (!itemRe.Result)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return ResultCode.Success;
		}
		if (num == itemRes.Count())
		{
			return new ResultCode("All files failed to cook");
		}
		return new ResultCode("One or more files failed to cook");
	}
}
