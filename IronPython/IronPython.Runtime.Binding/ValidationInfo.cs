using System.Linq.Expressions;

namespace IronPython.Runtime.Binding;

internal class ValidationInfo
{
	public readonly Expression Test;

	public static readonly ValidationInfo Empty = new ValidationInfo(null);

	public ValidationInfo(Expression test)
	{
		Test = test;
	}
}
