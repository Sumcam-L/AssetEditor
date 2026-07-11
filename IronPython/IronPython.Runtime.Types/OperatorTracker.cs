using System;
using System.Collections.Generic;
using System.Reflection;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;

namespace IronPython.Runtime.Types;

internal class OperatorTracker : PythonCustomTracker
{
	private MethodTracker[] _trackers;

	private bool _reversed;

	private string _name;

	private Type _declType;

	public override Type DeclaringType => _declType;

	public override string Name => _name;

	public OperatorTracker(Type declaringType, string name, bool reversed, params MethodTracker[] members)
	{
		_declType = declaringType;
		_reversed = reversed;
		_trackers = members;
		_name = name;
	}

	public override PythonTypeSlot GetSlot()
	{
		List<MethodBase> list = new List<MethodBase>();
		MethodTracker[] trackers = _trackers;
		foreach (MethodTracker methodTracker in trackers)
		{
			list.Add(methodTracker.Method);
		}
		MethodBase[] array = list.ToArray();
		FunctionType functionType = (PythonTypeOps.GetMethodFunctionType(DeclaringType, array) | FunctionType.Method) & ~FunctionType.Function;
		functionType = ((!_reversed) ? (functionType & ~FunctionType.ReversedOperator) : (functionType | FunctionType.ReversedOperator));
		MethodBase[] array2 = array;
		for (int j = 0; j < array2.Length; j++)
		{
			MethodInfo methodInfo = (MethodInfo)array2[j];
			if (!methodInfo.IsDefined(typeof(PythonHiddenAttribute), inherit: false))
			{
				functionType |= FunctionType.AlwaysVisible;
				break;
			}
		}
		return PythonTypeOps.GetFinalSlotForFunction(PythonTypeOps.GetBuiltinFunction(DeclaringType, Name, functionType, list.ToArray()));
	}
}
