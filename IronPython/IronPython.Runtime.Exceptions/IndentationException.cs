using System;
using System.Runtime.Serialization;
using System.Security;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Exceptions;

[Serializable]
internal class IndentationException : SyntaxErrorException
{
	public IndentationException(string message)
		: base(message)
	{
	}

	public IndentationException(string message, SourceUnit sourceUnit, SourceSpan span, int errorCode, Severity severity)
		: base(message, sourceUnit, span, errorCode, severity)
	{
	}

	protected IndentationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	[SecurityCritical]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		ContractUtils.RequiresNotNull(info, "info");
		base.GetObjectData(info, context);
	}
}
