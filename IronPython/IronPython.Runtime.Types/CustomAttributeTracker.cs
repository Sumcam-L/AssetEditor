using System;
using System.Dynamic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;

namespace IronPython.Runtime.Types;

internal class CustomAttributeTracker : PythonCustomTracker
{
	private readonly PythonTypeSlot _slot;

	private readonly Type _declType;

	private readonly string _name;

	public override string Name => _name;

	public override Type DeclaringType => _declType;

	public CustomAttributeTracker(Type declaringType, string name, PythonTypeSlot slot)
	{
		_declType = declaringType;
		_name = name;
		_slot = slot;
	}

	public override DynamicMetaObject GetValue(OverloadResolverFactory factory, ActionBinder binder, Type instanceType)
	{
		return GetBoundValue(factory, binder, instanceType, new DynamicMetaObject(Utils.Constant(null), BindingRestrictions.Empty));
	}

	public override PythonTypeSlot GetSlot()
	{
		return _slot;
	}
}
