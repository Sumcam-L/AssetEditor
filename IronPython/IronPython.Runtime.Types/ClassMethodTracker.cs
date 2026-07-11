using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;

namespace IronPython.Runtime.Types;

internal class ClassMethodTracker : PythonCustomTracker
{
	private MethodTracker[] _trackers;

	public override Type DeclaringType => _trackers[0].DeclaringType;

	public override string Name => _trackers[0].Name;

	public ClassMethodTracker(MemberGroup group)
	{
		List<MethodTracker> list = new List<MethodTracker>(group.Count);
		foreach (MethodTracker item in group)
		{
			list.Add(item);
		}
		_trackers = list.ToArray();
	}

	public override PythonTypeSlot GetSlot()
	{
		List<MethodBase> list = new List<MethodBase>();
		MethodTracker[] trackers = _trackers;
		foreach (MethodTracker methodTracker in trackers)
		{
			list.Add(methodTracker.Method);
		}
		return PythonTypeOps.GetFinalSlotForFunction(PythonTypeOps.GetBuiltinFunction(DeclaringType, Name, list.ToArray()));
	}

	public override DynamicMetaObject GetValue(OverloadResolverFactory factory, ActionBinder binder, Type instanceType)
	{
		return GetBoundValue(factory, binder, instanceType, new DynamicMetaObject(Utils.Constant(null), BindingRestrictions.Empty));
	}
}
