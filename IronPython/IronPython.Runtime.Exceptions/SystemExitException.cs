using System;
using System.Runtime.Serialization;
using IronPython.Modules;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Exceptions;

[Serializable]
public sealed class SystemExitException : Exception
{
	public SystemExitException()
	{
	}

	public SystemExitException(string msg)
		: base(msg)
	{
	}

	public SystemExitException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private SystemExitException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	[PythonHidden]
	public int GetExitCode(out object otherCode)
	{
		otherCode = null;
		object o = PythonExceptions.ToPython(this);
		if (!PythonOps.TryGetBoundAttr(o, "args", out var ret) || !(ret is PythonTuple pythonTuple) || pythonTuple.__len__() == 0)
		{
			return 0;
		}
		if (Builtin.isinstance(pythonTuple[0], TypeCache.Int32))
		{
			return Converter.ConvertToInt32(pythonTuple[0]);
		}
		otherCode = pythonTuple[0];
		return 1;
	}
}
