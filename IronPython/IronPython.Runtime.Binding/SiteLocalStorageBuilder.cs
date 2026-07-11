using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;

namespace IronPython.Runtime.Binding;

public sealed class SiteLocalStorageBuilder : ArgBuilder
{
	public override int Priority => -1;

	public override int ConsumedArgumentCount => 0;

	public SiteLocalStorageBuilder(ParameterInfo info)
		: base(info)
	{
	}

	protected override Expression ToExpression(OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed)
	{
		return Utils.Constant(Activator.CreateInstance(base.ParameterInfo.ParameterType));
	}
}
