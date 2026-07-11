using System;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

public static class DynamicHelpers
{
	public static PythonType GetPythonTypeFromType(Type type)
	{
		ContractUtils.RequiresNotNull(type, "type");
		return PythonType.GetPythonType(type);
	}

	public static PythonType GetPythonType(object o)
	{
		if (o is IPythonObject pythonObject)
		{
			return pythonObject.PythonType;
		}
		return GetPythonTypeFromType(CompilerHelpers.GetType(o));
	}

	public static ReflectedEvent.BoundEvent MakeBoundEvent(ReflectedEvent eventObj, object instance, Type type)
	{
		return new ReflectedEvent.BoundEvent(eventObj, instance, GetPythonTypeFromType(type));
	}
}
