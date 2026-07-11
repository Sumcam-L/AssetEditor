using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Binding;

public sealed class MetaExpandable<T> : DynamicMetaObject where T : IPythonExpandable
{
	private static readonly object _getFailed = new object();

	public new T Value => (T)base.Value;

	public MetaExpandable(Expression parameter, IPythonExpandable value)
		: base(parameter, BindingRestrictions.Empty, value)
	{
	}

	public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
	{
		return DynamicTryGetMember(binder.Name, binder.FallbackGetMember(this).Expression, (Expression res) => res);
	}

	public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
	{
		return DynamicTryGetMember(binder.Name, binder.FallbackInvokeMember(this, args).Expression, (Expression res) => binder.FallbackInvoke(new DynamicMetaObject(res, BindingRestrictions.Empty), args, null).Expression);
	}

	private DynamicMetaObject DynamicTryGetMember(string name, Expression fallback, Func<Expression, Expression> transform)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object));
		return new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(Expression.NotEqual(Expression.Assign(parameterExpression, Expression.Invoke(Expression.Constant(new Func<T, string, object>(TryGetMember)), Convert(base.Expression, typeof(T)), Expression.Constant(name))), Expression.Constant(_getFailed)), Utils.Convert(transform(parameterExpression), typeof(object)), Utils.Convert(fallback, typeof(object)))), GetRestrictions());
	}

	public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
	{
		return new DynamicMetaObject(Expression.Block(Expression.Condition(Convert(Expression.Invoke(Expression.Constant(new Func<T, string, object, bool>(TrySetMember)), Convert(base.Expression, typeof(T)), Expression.Constant(binder.Name), Convert(value.Expression, typeof(object))), typeof(bool)), Expression.Empty(), binder.FallbackSetMember(this, value).Expression, typeof(void)), value.Expression), GetRestrictions());
	}

	public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
	{
		return new DynamicMetaObject(Expression.Condition(Convert(Expression.Invoke(Expression.Constant(new Func<T, string, bool>(TryDeleteMember)), Convert(base.Expression, typeof(T)), Expression.Constant(binder.Name)), typeof(bool)), Expression.Empty(), binder.FallbackDeleteMember(this).Expression, typeof(void)), GetRestrictions());
	}

	public override IEnumerable<string> GetDynamicMemberNames()
	{
		IDictionary<string, object> customAttributes = Value.CustomAttributes;
		if (customAttributes == null)
		{
			return base.GetDynamicMemberNames();
		}
		IList<object> memberNames = DynamicHelpers.GetPythonType(Value).GetMemberNames(Value.Context);
		List<string> list = new List<string>(customAttributes.Keys.Count + memberNames.Count);
		list.AddRange(customAttributes.Keys);
		for (int i = 0; i < memberNames.Count; i++)
		{
			string text = memberNames[i] as string;
			if (text == null)
			{
				if (!(memberNames[i] is Extensible<string> extensible))
				{
					continue;
				}
				text = extensible.Value;
			}
			list.Add(text);
		}
		return list;
	}

	private BindingRestrictions GetRestrictions()
	{
		return BindingRestrictions.GetTypeRestriction(base.Expression, typeof(T));
	}

	private static Expression Convert(Expression expression, Type type)
	{
		if (!(expression.Type != type))
		{
			return expression;
		}
		return Expression.Convert(expression, type);
	}

	private static object TryGetMember(T target, string name)
	{
		IDictionary<string, object> customAttributes = target.CustomAttributes;
		if (customAttributes != null && customAttributes.TryGetValue(name, out var value))
		{
			return value;
		}
		return _getFailed;
	}

	private static bool TrySetMember(T target, string name, object value)
	{
		MemberInfo memberInfo = (MemberInfo)(((object)typeof(T).GetProperty(name)) ?? ((object)typeof(T).GetField(name)));
		if (memberInfo != null && !memberInfo.IsDefined(typeof(PythonHiddenAttribute), inherit: false))
		{
			return false;
		}
		target.EnsureCustomAttributes()[name] = value;
		return true;
	}

	private static bool TryDeleteMember(T target, string name)
	{
		return target.CustomAttributes?.Remove(name) ?? false;
	}
}
