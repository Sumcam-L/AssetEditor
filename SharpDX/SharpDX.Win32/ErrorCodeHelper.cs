namespace SharpDX.Win32;

public class ErrorCodeHelper
{
	public static Result ToResult(ErrorCode errorCode)
	{
		return ToResult((int)errorCode);
	}

	public static Result ToResult(int errorCode)
	{
		return new Result((uint)((errorCode <= 0) ? errorCode : ((errorCode & 0xFFFF) | -2147024896)));
	}
}
