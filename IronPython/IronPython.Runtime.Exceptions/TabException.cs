using System;
using Microsoft.Scripting;

namespace IronPython.Runtime.Exceptions;

[Serializable]
internal sealed class TabException : IndentationException
{
	public TabException(string message)
		: base(message)
	{
	}

	public TabException(string message, SourceUnit sourceUnit, SourceSpan span, int errorCode, Severity severity)
		: base(message, sourceUnit, span, errorCode, severity)
	{
	}
}
