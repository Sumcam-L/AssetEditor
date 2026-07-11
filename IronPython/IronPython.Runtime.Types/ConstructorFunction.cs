using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

[PythonType("builtin_function_or_method")]
public class ConstructorFunction : BuiltinFunction
{
	private MethodBase[] _ctors;

	internal IList<MethodBase> ConstructorTargets => _ctors;

	public override BuiltinFunctionOverloadMapper Overloads
	{
		[PythonHidden]
		get
		{
			return new ConstructorOverloadMapper(this, null);
		}
	}

	public new string __name__ => "__new__";

	public override string __doc__
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (MethodBase constructorTarget in ConstructorTargets)
			{
				if (constructorTarget != null)
				{
					stringBuilder.AppendLine(DocBuilder.DocOneInfo(constructorTarget, "__new__"));
				}
			}
			return stringBuilder.ToString();
		}
	}

	internal ConstructorFunction(BuiltinFunction realTarget, IList<MethodBase> constructors)
		: base("__new__", ArrayUtils.ToArray(GetTargetsValidateFunction(realTarget)), realTarget.DeclaringType, FunctionType.Function | FunctionType.AlwaysVisible)
	{
		base.Name = realTarget.Name;
		base.FunctionType = realTarget.FunctionType;
		_ctors = ArrayUtils.ToArray(constructors);
	}

	private static IList<MethodBase> GetTargetsValidateFunction(BuiltinFunction realTarget)
	{
		ContractUtils.RequiresNotNull(realTarget, "realTarget");
		return realTarget.Targets;
	}
}
