using IronPython.Runtime.Operations;
using Microsoft.Scripting;

namespace IronPython.Runtime;

internal class ThrowingErrorSink : ErrorSink
{
	public new static readonly ThrowingErrorSink Default = new ThrowingErrorSink();

	private ThrowingErrorSink()
	{
	}

	public override void Add(SourceUnit sourceUnit, string message, SourceSpan span, int errorCode, Severity severity)
	{
		if (severity == Severity.Warning)
		{
			PythonOps.SyntaxWarning(message, sourceUnit, span, errorCode);
			return;
		}
		throw PythonOps.SyntaxError(message, sourceUnit, span, errorCode);
	}
}
