using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

[PythonType("getset_descriptor")]
public class ReflectedProperty : ReflectedGetterSetter, ICodeFormattable
{
	private readonly PropertyInfo _info;

	internal override bool CanOptimizeGets => true;

	internal override Type DeclaringType => _info.DeclaringType;

	public override string __name__ => _info.Name;

	public PropertyInfo Info
	{
		[PythonHidden]
		get
		{
			return _info;
		}
	}

	public override PythonType PropertyType
	{
		[PythonHidden]
		get
		{
			return DynamicHelpers.GetPythonTypeFromType(_info.PropertyType);
		}
	}

	internal override bool GetAlwaysSucceeds => true;

	internal override bool IsAlwaysVisible => base.NameType == NameType.PythonProperty;

	public string __doc__ => DocBuilder.DocOneInfo(Info);

	public ReflectedProperty(PropertyInfo info, MethodInfo getter, MethodInfo setter, NameType nt)
		: base(new MethodInfo[1] { getter }, new MethodInfo[1] { setter }, nt)
	{
		_info = info;
	}

	public ReflectedProperty(PropertyInfo info, MethodInfo[] getters, MethodInfo[] setters, NameType nt)
		: base(getters, setters, nt)
	{
		_info = info;
	}

	internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value)
	{
		if (base.Setter.Length == 0)
		{
			return false;
		}
		if (instance == null)
		{
			MethodInfo[] setter = base.Setter;
			foreach (MethodInfo methodInfo in setter)
			{
				if (methodInfo.IsStatic && DeclaringType != owner.UnderlyingSystemType)
				{
					return false;
				}
				if (methodInfo.IsProtected())
				{
					throw PythonOps.TypeErrorForProtectedMember(owner.UnderlyingSystemType, _info.Name);
				}
			}
		}
		else if (instance != null)
		{
			MethodInfo[] setter2 = base.Setter;
			foreach (MethodInfo methodInfo2 in setter2)
			{
				if (methodInfo2.IsStatic)
				{
					return false;
				}
			}
		}
		return CallSetter(context, PythonContext.GetContext(context).GetGenericCallSiteStorage(), instance, ArrayUtils.EmptyObjects, value);
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		value = CallGetter(context, owner, PythonContext.GetContext(context).GetGenericCallSiteStorage0(), instance);
		return true;
	}

	private object CallGetter(CodeContext context, PythonType owner, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object>>> storage, object instance)
	{
		if (ReflectedGetterSetter.NeedToReturnProperty(instance, base.Getter))
		{
			return this;
		}
		if (base.Getter.Length == 0)
		{
			throw new MissingMemberException("unreadable property");
		}
		if (owner == null)
		{
			owner = DynamicHelpers.GetPythonType(instance);
		}
		MethodInfo[] mems = base.Getter;
		Type underlyingSystemType = owner.UnderlyingSystemType;
		if (base.Getter.Length > 1)
		{
			Type declaringType = base.Getter[0].DeclaringType;
			MethodInfo methodInfo = base.Getter[0];
			for (int i = 1; i < base.Getter.Length; i++)
			{
				MethodInfo mt = base.Getter[i];
				if (IsApplicableForType(underlyingSystemType, mt) && (base.Getter[i].DeclaringType.IsSubclassOf(declaringType) || !IsApplicableForType(underlyingSystemType, methodInfo)))
				{
					methodInfo = base.Getter[i];
					declaringType = base.Getter[i].DeclaringType;
				}
			}
			mems = new MethodInfo[1] { methodInfo };
		}
		BuiltinFunction builtinFunction = PythonTypeOps.GetBuiltinFunction(underlyingSystemType, __name__, mems);
		return builtinFunction.Call0(context, storage, instance);
	}

	private static bool IsApplicableForType(Type type, MethodInfo mt)
	{
		if (!(mt.DeclaringType == type))
		{
			return type.IsSubclassOf(mt.DeclaringType);
		}
		return true;
	}

	internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner)
	{
		__delete__(instance);
		return true;
	}

	internal override void MakeGetExpression(PythonBinder binder, Expression codeContext, DynamicMetaObject instance, DynamicMetaObject owner, ConditionalBuilder builder)
	{
		if (base.Getter.Length != 0 && !base.Getter[0].IsPublic)
		{
			base.MakeGetExpression(binder, codeContext, instance, owner, builder);
		}
		else if (ReflectedGetterSetter.NeedToReturnProperty(instance, base.Getter))
		{
			builder.FinishCondition(Utils.Constant(this));
		}
		else if (base.Getter[0].ContainsGenericParameters)
		{
			builder.FinishCondition(DefaultBinder.MakeError(binder.MakeContainsGenericParametersError(MemberTracker.FromMemberInfo(_info)), typeof(object)).Expression);
		}
		else if (instance != null)
		{
			builder.FinishCondition(Utils.Convert(binder.MakeCallExpression(new PythonOverloadResolverFactory(binder, codeContext), base.Getter[0], instance).Expression, typeof(object)));
		}
		else
		{
			builder.FinishCondition(Utils.Convert(binder.MakeCallExpression(new PythonOverloadResolverFactory(binder, codeContext), base.Getter[0]).Expression, typeof(object)));
		}
	}

	internal override bool IsSetDescriptor(CodeContext context, PythonType owner)
	{
		return base.Setter.Length != 0;
	}

	[PythonHidden]
	public object GetValue(CodeContext context, object instance)
	{
		if (TryGetValue(context, instance, DynamicHelpers.GetPythonType(instance), out var value))
		{
			return value;
		}
		throw new InvalidOperationException("cannot get property");
	}

	[PythonHidden]
	public void SetValue(CodeContext context, object instance, object value)
	{
		if (!TrySetValue(context, instance, DynamicHelpers.GetPythonType(instance), value))
		{
			throw new InvalidOperationException("cannot set property");
		}
	}

	public void __set__(CodeContext context, object instance, object value)
	{
		TrySetValue(context, instance, DynamicHelpers.GetPythonType(instance), value);
	}

	public void __delete__(object instance)
	{
		if (base.Setter.Length != 0)
		{
			throw PythonOps.AttributeErrorForReadonlyAttribute(DynamicHelpers.GetPythonTypeFromType(DeclaringType).Name, __name__);
		}
		throw PythonOps.AttributeErrorForBuiltinAttributeDeletion(DynamicHelpers.GetPythonTypeFromType(DeclaringType).Name, __name__);
	}

	public string __repr__(CodeContext context)
	{
		return $"<property# {__name__} on {DynamicHelpers.GetPythonTypeFromType(DeclaringType).Name}>";
	}
}
