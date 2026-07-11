using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Sce.Atf.Controls.PropertyEditing;

internal static class PropertyChangedExtensions
{
	public static bool NotifyIfChanged<T>(this PropertyChangedEventHandler handler, ref T field, T value, Expression<Func<T>> memberExpression)
	{
		if (memberExpression == null)
		{
			throw new ArgumentNullException("memberExpression");
		}
		if (!(memberExpression.Body is MemberExpression memberExpression2))
		{
			throw new ArgumentException("Lambda must return a property.");
		}
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return false;
		}
		field = value;
		if (memberExpression2.Expression is ConstantExpression body)
		{
			LambdaExpression lambdaExpression = Expression.Lambda(body);
			Delegate obj = lambdaExpression.Compile();
			object sender = obj.DynamicInvoke();
			handler?.Invoke(sender, new PropertyChangedEventArgs(memberExpression2.Member.Name));
		}
		return true;
	}
}
