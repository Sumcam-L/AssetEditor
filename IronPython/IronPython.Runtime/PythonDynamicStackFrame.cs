using System;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

[Serializable]
internal sealed class PythonDynamicStackFrame : DynamicStackFrame, ISerializable
{
	private readonly CodeContext _context;

	private readonly FunctionCode _code;

	public CodeContext CodeContext => _context;

	public FunctionCode Code => _code;

	public PythonDynamicStackFrame(CodeContext context, FunctionCode funcCode, int line)
		: base(GetMethod(context, funcCode), funcCode.co_name, funcCode.co_filename, line)
	{
		_context = context;
		_code = funcCode;
	}

	private static MethodBase GetMethod(CodeContext context, FunctionCode funcCode)
	{
		if (!context.LanguageContext.EnableTracing || (object)funcCode._tracingDelegate == null)
		{
			return funcCode._normalDelegate.GetMethod();
		}
		return funcCode._tracingDelegate.GetMethod();
	}

	private PythonDynamicStackFrame(SerializationInfo info, StreamingContext context)
		: base((MethodBase)info.GetValue("method", typeof(MethodBase)), (string)info.GetValue("funcName", typeof(string)), (string)info.GetValue("filename", typeof(string)), (int)info.GetValue("line", typeof(int)))
	{
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("method", GetMethod());
		info.AddValue("funcName", GetMethodName());
		info.AddValue("filename", GetFileName());
		info.AddValue("line", GetFileLineNumber());
	}
}
