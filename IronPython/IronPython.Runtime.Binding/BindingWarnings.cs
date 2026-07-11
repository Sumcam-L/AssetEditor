using System;
using System.Linq.Expressions;
using System.Threading;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;

namespace IronPython.Runtime.Binding;

internal static class BindingWarnings
{
	public static bool ShouldWarn(PythonContext context, OverloadInfo method, out WarningInfo info)
	{
		ObsoleteAttribute[] array = (ObsoleteAttribute[])method.ReflectionInfo.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: true);
		if (array.Length > 0)
		{
			info = new WarningInfo(PythonExceptions.DeprecationWarning, $"{NameConverter.GetTypeName(method.DeclaringType)}.{method.Name} has been obsoleted.  {array[0].Message}");
			return true;
		}
		if (context.PythonOptions.WarnPython30)
		{
			Python3WarningAttribute[] array2 = (Python3WarningAttribute[])method.ReflectionInfo.GetCustomAttributes(typeof(Python3WarningAttribute), inherit: true);
			if (array2.Length > 0)
			{
				info = new WarningInfo(PythonExceptions.DeprecationWarning, array2[0].Message);
				return true;
			}
		}
		if (method.DeclaringType == typeof(Thread) && method.Name == "Sleep")
		{
			info = new WarningInfo(PythonExceptions.RuntimeWarning, "Calling Thread.Sleep on an STA thread doesn't pump messages.  Use Thread.CurrentThread.Join instead.", Expression.Equal(Expression.Call(Expression.Property(null, typeof(Thread).GetProperty("CurrentThread")), typeof(Thread).GetMethod("GetApartmentState")), Utils.Constant(ApartmentState.STA)));
			return true;
		}
		info = null;
		return false;
	}
}
