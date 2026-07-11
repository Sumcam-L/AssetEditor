using IronPython.Runtime.Exceptions;

namespace IronPython.Runtime.Operations;

public struct FunctionStack
{
	public readonly CodeContext Context;

	public readonly FunctionCode Code;

	public TraceBackFrame Frame;

	internal FunctionStack(CodeContext context, FunctionCode code)
	{
		Context = context;
		Code = code;
		Frame = null;
	}

	internal FunctionStack(CodeContext context, FunctionCode code, TraceBackFrame frame)
	{
		Context = context;
		Code = code;
		Frame = frame;
	}

	internal FunctionStack(TraceBackFrame frame)
	{
		Context = frame.Context;
		Code = frame.f_code;
		Frame = frame;
	}
}
