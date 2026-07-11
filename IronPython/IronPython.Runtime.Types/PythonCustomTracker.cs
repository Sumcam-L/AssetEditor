using System;
using System.Dynamic;
using System.Linq.Expressions;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;

namespace IronPython.Runtime.Types;

public abstract class PythonCustomTracker : CustomTracker
{
	public abstract PythonTypeSlot GetSlot();

	public override DynamicMetaObject GetValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type)
	{
		return new DynamicMetaObject(Utils.Constant(GetSlot(), typeof(PythonTypeSlot)), BindingRestrictions.Empty);
	}

	public override MemberTracker BindToInstance(DynamicMetaObject instance)
	{
		return new BoundMemberTracker(this, instance);
	}

	public override DynamicMetaObject SetValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type, DynamicMetaObject value)
	{
		return SetBoundValue(resolverFactory, binder, type, value, new DynamicMetaObject(Utils.Constant(null), BindingRestrictions.Empty));
	}

	public override DynamicMetaObject SetValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
	{
		return base.SetValue(resolverFactory, binder, type, value, errorSuggestion);
	}

	protected override DynamicMetaObject GetBoundValue(OverloadResolverFactory factory, ActionBinder binder, Type instanceType, DynamicMetaObject instance)
	{
		return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("SlotGetValue"), ((PythonOverloadResolverFactory)factory)._codeContext, Utils.Constant(GetSlot(), typeof(PythonTypeSlot)), Utils.Convert(instance.Expression, typeof(object)), Utils.Constant(DynamicHelpers.GetPythonTypeFromType(instanceType))), BindingRestrictions.Empty);
	}

	protected override DynamicMetaObject SetBoundValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type, DynamicMetaObject value, DynamicMetaObject instance)
	{
		return SetBoundValue(resolverFactory, binder, type, value, instance, null);
	}

	protected override DynamicMetaObject SetBoundValue(OverloadResolverFactory factory, ActionBinder binder, Type type, DynamicMetaObject value, DynamicMetaObject instance, DynamicMetaObject errorSuggestion)
	{
		return new DynamicMetaObject(Expression.Condition(Expression.Call(typeof(PythonOps).GetMethod("SlotTrySetValue"), ((PythonOverloadResolverFactory)factory)._codeContext, Utils.Constant(GetSlot(), typeof(PythonTypeSlot)), Utils.Convert(instance.Expression, typeof(object)), Utils.Constant(DynamicHelpers.GetPythonTypeFromType(type)), value.Expression), Utils.Convert(value.Expression, typeof(object)), (errorSuggestion != null) ? errorSuggestion.Expression : Expression.Throw(Expression.Call(typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[2]
		{
			typeof(object),
			typeof(string)
		}), instance.Expression, Expression.Constant(Name)), typeof(object))), BindingRestrictions.Empty);
	}
}
