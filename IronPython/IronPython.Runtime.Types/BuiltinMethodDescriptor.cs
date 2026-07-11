using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;

namespace IronPython.Runtime.Types;

[PythonType("method_descriptor")]
[DontMapGetMemberNamesToDir]
public sealed class BuiltinMethodDescriptor : PythonTypeSlot, IDynamicMetaObjectProvider, ICodeFormattable
{
	internal readonly BuiltinFunction _template;

	internal override bool GetAlwaysSucceeds => true;

	internal BuiltinFunction Template => _template;

	public Type DeclaringType
	{
		[PythonHidden]
		get
		{
			return _template.DeclaringType;
		}
	}

	internal override bool IsAlwaysVisible => _template.IsAlwaysVisible;

	public string __name__ => Template.__name__;

	public string __doc__ => Template.__doc__;

	public PythonType __objclass__ => DynamicHelpers.GetPythonTypeFromType(_template.DeclaringType);

	internal BuiltinMethodDescriptor(BuiltinFunction function)
	{
		_template = function;
	}

	internal object UncheckedGetAttribute(object instance)
	{
		return _template.BindToInstance(instance);
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		if (instance != null || owner == TypeCache.Null)
		{
			CheckSelf(context, instance);
			value = UncheckedGetAttribute(instance);
			return true;
		}
		value = this;
		return true;
	}

	internal override void MakeGetExpression(PythonBinder binder, Expression codeContext, DynamicMetaObject instance, DynamicMetaObject owner, ConditionalBuilder builder)
	{
		if (instance != null)
		{
			builder.FinishCondition(Expression.Call(typeof(PythonOps).GetMethod("MakeBoundBuiltinFunction"), Utils.Constant(_template), instance.Expression));
		}
		else
		{
			builder.FinishCondition(Utils.Constant(this));
		}
	}

	internal static void CheckSelfWorker(CodeContext context, object self, BuiltinFunction template)
	{
		Type type = CompilerHelpers.GetType(self);
		if (type != template.DeclaringType && !template.DeclaringType.IsAssignableFrom(type))
		{
			context.LanguageContext.Binder.Convert(self, template.DeclaringType);
		}
	}

	private void CheckSelf(CodeContext context, object self)
	{
		if ((_template.FunctionType & FunctionType.FunctionMethodMask) == FunctionType.Method)
		{
			CheckSelfWorker(context, self, _template);
		}
	}

	public object __call__(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], IDictionary<object, object>, object>>> storage, [ParamDictionary] IDictionary<object, object> dictArgs, params object[] args)
	{
		return _template.__call__(context, storage, dictArgs, args);
	}

	public int __cmp__(object other)
	{
		if (!(other is BuiltinMethodDescriptor builtinMethodDescriptor))
		{
			throw PythonOps.TypeError("instancemethod.__cmp__(x,y) requires y to be a 'instancemethod', not a {0}", PythonTypeOps.GetName(other));
		}
		long num = PythonOps.Id(__objclass__) - PythonOps.Id(builtinMethodDescriptor.__objclass__);
		if (num != 0)
		{
			if (num <= 0)
			{
				return -1;
			}
			return 1;
		}
		return StringOps.Compare(__name__, builtinMethodDescriptor.__name__);
	}

	public string __repr__(CodeContext context)
	{
		return $"<method '{Template.Name}' of '{DynamicHelpers.GetPythonTypeFromType(DeclaringType).Name}' objects>";
	}

	DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
	{
		return new MetaBuiltinMethodDescriptor(parameter, BindingRestrictions.Empty, this);
	}
}
