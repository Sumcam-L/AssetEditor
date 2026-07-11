using System;
using System.Globalization;

namespace SharpDX;

public class SharpDXException : Exception
{
	private ResultDescriptor descriptor;

	public Result ResultCode => descriptor.Result;

	public ResultDescriptor Descriptor => descriptor;

	public SharpDXException()
		: base("A SharpDX exception occurred.")
	{
		descriptor = ResultDescriptor.Find(Result.Fail);
		base.HResult = (int)Result.Fail;
	}

	public SharpDXException(Result result)
		: this(ResultDescriptor.Find(result))
	{
		base.HResult = (int)result;
	}

	public SharpDXException(ResultDescriptor descriptor)
		: base(descriptor.ToString())
	{
		this.descriptor = descriptor;
		base.HResult = (int)descriptor.Result;
	}

	public SharpDXException(Result result, string message)
		: base(message)
	{
		descriptor = ResultDescriptor.Find(result);
		base.HResult = (int)result;
	}

	public SharpDXException(Result result, string message, params object[] args)
		: base(string.Format(CultureInfo.InvariantCulture, message, args))
	{
		descriptor = ResultDescriptor.Find(result);
		base.HResult = (int)result;
	}

	public SharpDXException(string message, params object[] args)
		: this(Result.Fail, message, args)
	{
	}

	public SharpDXException(string message, Exception innerException, params object[] args)
		: base(string.Format(CultureInfo.InvariantCulture, message, args), innerException)
	{
		descriptor = ResultDescriptor.Find(Result.Fail);
		base.HResult = (int)Result.Fail;
	}
}
