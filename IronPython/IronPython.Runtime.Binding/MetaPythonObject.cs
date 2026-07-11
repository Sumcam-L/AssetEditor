using System;
using System.Dynamic;
using System.Linq.Expressions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Binding;

internal class MetaPythonObject : DynamicMetaObject
{
	public PythonType PythonType => DynamicHelpers.GetPythonType(base.Value);

	public MetaPythonObject(Expression expression, BindingRestrictions restrictions)
		: base(expression, restrictions)
	{
	}

	public MetaPythonObject(Expression expression, BindingRestrictions restrictions, object value)
		: base(expression, restrictions, value)
	{
	}

	public DynamicMetaObject FallbackConvert(DynamicMetaObjectBinder binder)
	{
		if (binder is PythonConversionBinder pythonConversionBinder)
		{
			return pythonConversionBinder.FallbackConvert(binder.ReturnType, this, null);
		}
		return ((ConvertBinder)binder).FallbackConvert(this);
	}

	internal static MethodCallExpression MakeTryGetTypeMember(PythonContext PythonContext, PythonTypeSlot dts, Expression self, ParameterExpression tmp)
	{
		return MakeTryGetTypeMember(PythonContext, dts, tmp, self, Expression.Property(Expression.Convert(self, typeof(IPythonObject)), TypeInfo._IPythonObject.PythonType));
	}

	internal static MethodCallExpression MakeTryGetTypeMember(PythonContext PythonContext, PythonTypeSlot dts, ParameterExpression tmp, Expression instance, Expression pythonType)
	{
		return Expression.Call(TypeInfo._PythonOps.SlotTryGetBoundValue, Utils.Constant(PythonContext.SharedContext), Utils.Convert(Utils.WeakConstant(dts), typeof(PythonTypeSlot)), Utils.Convert(instance, typeof(object)), Utils.Convert(pythonType, typeof(PythonType)), tmp);
	}

	public DynamicMetaObject Restrict(Type type)
	{
		return MetaObjectExtensions.Restrict(this, type);
	}

	public static PythonType GetPythonType(DynamicMetaObject value)
	{
		if (value.HasValue)
		{
			return DynamicHelpers.GetPythonType(value.Value);
		}
		return DynamicHelpers.GetPythonTypeFromType(value.GetLimitType());
	}

	protected static DynamicMetaObject MakeDelegateTarget(DynamicMetaObjectBinder action, Type toType, DynamicMetaObject arg)
	{
		PythonContext pythonContext = PythonContext.GetPythonContext(action);
		return new DynamicMetaObject(Expression.Convert(Expression.Call(arg0: Utils.Constant((pythonContext == null) ? DefaultContext.Default : pythonContext.SharedContext), method: typeof(PythonOps).GetMethod("GetDelegate"), arg1: arg.Expression, arg2: Utils.Constant(toType)), toType), arg.Restrictions);
	}

	protected static DynamicMetaObject GetMemberFallback(DynamicMetaObject self, DynamicMetaObjectBinder member, DynamicMetaObject codeContext)
	{
		if (member is PythonGetMemberBinder pythonGetMemberBinder)
		{
			return pythonGetMemberBinder.Fallback(self, codeContext);
		}
		GetMemberBinder getMemberBinder = (GetMemberBinder)member;
		return getMemberBinder.FallbackGetMember(self.Restrict(self.GetLimitType()));
	}

	protected static string GetGetMemberName(DynamicMetaObjectBinder member)
	{
		if (member is PythonGetMemberBinder pythonGetMemberBinder)
		{
			return pythonGetMemberBinder.Name;
		}
		if (member is InvokeMemberBinder invokeMemberBinder)
		{
			return invokeMemberBinder.Name;
		}
		GetMemberBinder getMemberBinder = (GetMemberBinder)member;
		return getMemberBinder.Name;
	}
}
