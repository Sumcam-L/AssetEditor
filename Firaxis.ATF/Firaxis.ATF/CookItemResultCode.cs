using Firaxis.Error;

namespace Firaxis.ATF;

public class CookItemResultCode
{
	public readonly ResultCode Result;

	public readonly string Item;

	public CookItemResultCode(ResultCode res, string item)
	{
		Result = res;
		Item = item;
	}
}
