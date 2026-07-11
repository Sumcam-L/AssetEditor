using System;
using Firaxis.Error;

namespace Firaxis.VersionControl;

public class ResultCodeException : Exception
{
	public readonly ResultCode Result;

	public ResultCodeException(ResultCode res, string extraInfo)
		: base(extraInfo)
	{
		Result = res;
	}
}
