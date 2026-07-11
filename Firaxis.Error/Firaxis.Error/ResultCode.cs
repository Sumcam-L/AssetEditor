namespace Firaxis.Error;

public class ResultCode
{
	public static readonly ResultCode Success;

	public string Message { get; private set; }

	static ResultCode()
	{
		Success = new ResultCode("OK");
	}

	public ResultCode(string msg)
	{
		Message = msg;
	}

	public ResultCode(string message, params object[] args)
	{
		Message = string.Format(message, args);
	}

	public static implicit operator bool(ResultCode rc)
	{
		return rc == Success;
	}
}
