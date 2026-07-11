using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Sce.Atf;

public static class ObservableUtil
{
	public static readonly PropertyChangedEventArgs AllChangedEventArgs = new PropertyChangedEventArgs(string.Empty);

	public static PropertyChangedEventArgs CreateArgs<T>(Expression<Func<T, object>> expression)
	{
		return new PropertyChangedEventArgs(TypeUtil.GetProperty(expression).Name);
	}
}
