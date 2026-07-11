using System;
using System.IO;
using System.Text;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime;

internal sealed class OutputWriter : TextWriter
{
	private readonly PythonContext _context;

	private readonly bool _isErrorOutput;

	public object Sink
	{
		get
		{
			if (!_isErrorOutput)
			{
				return _context.SystemStandardOut;
			}
			return _context.SystemStandardError;
		}
	}

	public override Encoding Encoding
	{
		get
		{
			if (!(Sink is PythonFile pythonFile))
			{
				return null;
			}
			return pythonFile.Encoding;
		}
	}

	public OutputWriter(PythonContext context, bool isErrorOutput)
	{
		_context = context;
		_isErrorOutput = isErrorOutput;
	}

	public override void Write(string value)
	{
		try
		{
			PythonOps.PrintWithDestNoNewline(DefaultContext.Default, Sink, value);
		}
		catch (Exception exception)
		{
			PythonOps.PrintWithDest(DefaultContext.Default, _context.SystemStandardOut, _context.FormatException(exception));
		}
	}

	public override void Write(char value)
	{
		Write(value.ToString());
	}

	public override void Write(char[] value)
	{
		Write(new string(value));
	}

	public override void Flush()
	{
		if (Sink is PythonFile pythonFile)
		{
			pythonFile.flush();
		}
		else if (PythonOps.HasAttr(_context.SharedContext, Sink, "flush"))
		{
			PythonOps.Invoke(_context.SharedContext, Sink, "flush");
		}
	}
}
