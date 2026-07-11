using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Sce.Atf;

public static class TypeUtil
{
	public static PropertyInfo GetProperty<T>(Expression<Func<T, object>> propertySelector)
	{
		Expression expression = propertySelector.Body;
		if (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
		{
			expression = ((UnaryExpression)expression).Operand;
		}
		MemberExpression memberExpression = expression as MemberExpression;
		if (memberExpression == null)
		{
			ThrowExpressionArgumentException("propertySelector");
		}
		expression = memberExpression.Expression;
		if (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
		{
			expression = ((UnaryExpression)expression).Operand;
		}
		if (expression.NodeType != ExpressionType.Parameter)
		{
			ThrowExpressionArgumentException("propertySelector");
		}
		PropertyInfo propertyInfo = memberExpression.Member as PropertyInfo;
		if (propertyInfo == null)
		{
			ThrowExpressionArgumentException("propertySelector");
		}
		return propertyInfo;
	}

	public static PropertyInfo GetPropertyInfo(Expression<Func<object>> propertySelector)
	{
		PropertyInfo propertyInfo = null;
		if (propertySelector.Body is MemberExpression memberExpression)
		{
			propertyInfo = memberExpression.Member as PropertyInfo;
		}
		else if (propertySelector.Body is UnaryExpression { Operand: MemberExpression operand })
		{
			propertyInfo = operand.Member as PropertyInfo;
		}
		if (propertyInfo == null)
		{
			throw new ArgumentException("lambda expression was not properly formed. Should be \"() => myObject.MyProperty\" or \"() => MyClass.MyProperty\"");
		}
		return propertyInfo;
	}

	public static PropertyDescriptor GetPropertyDescriptor<T>(Expression<Func<T, object>> propertySelector)
	{
		PropertyInfo property = GetProperty(propertySelector);
		return TypeDescriptor.GetProperties(typeof(T))[property.Name];
	}

	private static void ThrowExpressionArgumentException(string argumentName)
	{
		throw new ArgumentException("It's just the simple expression 'x => x.Property' allowed.", argumentName);
	}
}
