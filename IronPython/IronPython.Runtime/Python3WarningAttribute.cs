using System;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class Python3WarningAttribute : Attribute
{
	private readonly string _message;

	public string Message => _message;

	public Python3WarningAttribute(string message)
	{
		ContractUtils.RequiresNotNull(message, "message");
		_message = message;
	}
}
