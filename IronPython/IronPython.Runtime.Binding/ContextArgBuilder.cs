using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Actions.Calls;

namespace IronPython.Runtime.Binding;

public sealed class ContextArgBuilder : ArgBuilder
{
	public override int Priority => -1;

	public override int ConsumedArgumentCount => 0;

	public ContextArgBuilder(ParameterInfo info)
		: base(info)
	{
	}

	protected override Expression ToExpression(OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed)
	{
		return ((PythonOverloadResolver)resolver).ContextExpression;
	}
}
