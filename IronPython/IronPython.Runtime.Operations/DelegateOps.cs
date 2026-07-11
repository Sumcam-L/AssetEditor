using System;
using System.Collections.Generic;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class DelegateOps
{
	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType type, object function)
	{
		if (type == null)
		{
			throw PythonOps.TypeError("expected type for 1st param, got null");
		}
		if (function is IDelegateConvertible delegateConvertible)
		{
			Delegate obj = delegateConvertible.ConvertToDelegate(type.UnderlyingSystemType);
			if ((object)obj != null)
			{
				return obj;
			}
		}
		return context.LanguageContext.DelegateCreator.GetDelegate(function, type.UnderlyingSystemType);
	}

	public static Delegate InPlaceAdd(Delegate self, Delegate other)
	{
		ContractUtils.RequiresNotNull(self, "self");
		ContractUtils.RequiresNotNull(other, "other");
		return Delegate.Combine(self, other);
	}

	public static Delegate InPlaceSubtract(Delegate self, Delegate other)
	{
		ContractUtils.RequiresNotNull(self, "self");
		ContractUtils.RequiresNotNull(other, "other");
		return Delegate.Remove(self, other);
	}

	public static object Call(CodeContext context, Delegate @delegate, params object[] args)
	{
		return PythonContext.GetContext(context).CallSplat(@delegate, args);
	}

	public static object Call(CodeContext context, Delegate @delegate, [ParamDictionary] IDictionary<object, object> dict, params object[] args)
	{
		return PythonContext.GetContext(context).CallWithKeywords(@delegate, args, dict);
	}
}
