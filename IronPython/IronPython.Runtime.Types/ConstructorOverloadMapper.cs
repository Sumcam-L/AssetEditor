using System.Collections.Generic;
using System.Reflection;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types;

public class ConstructorOverloadMapper : BuiltinFunctionOverloadMapper
{
	public override IList<MethodBase> Targets => ((ConstructorFunction)base.Function).ConstructorTargets;

	public ConstructorOverloadMapper(ConstructorFunction builtinFunction, object instance)
		: base(builtinFunction, instance)
	{
	}

	protected override object GetTargetFunction(BuiltinFunction bf)
	{
		if (bf.Targets[0].DeclaringType != typeof(InstanceOps))
		{
			return new ConstructorFunction(InstanceOps.OverloadedNew, bf.Targets).BindToInstance(bf);
		}
		return base.GetTargetFunction(bf);
	}
}
