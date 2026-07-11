using System;
using System.Reflection;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types;

public class GenericBuiltinFunction : BuiltinFunction
{
	public BuiltinFunction this[PythonTuple tuple] => this[tuple._data];

	public BuiltinFunction this[params object[] key]
	{
		get
		{
			Type[] array = new Type[key.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Converter.ConvertToType(key[i]);
			}
			BuiltinFunction builtinFunction = MakeGenericMethod(array);
			if (builtinFunction == null)
			{
				bool flag = false;
				foreach (MethodBase target in base.Targets)
				{
					MethodInfo methodInfo = target as MethodInfo;
					if (methodInfo != null && methodInfo.ContainsGenericParameters)
					{
						flag = true;
					}
				}
				if (flag)
				{
					throw PythonOps.TypeError($"bad type args to this generic method {base.Name}");
				}
				throw PythonOps.TypeError($"{base.Name} is not a generic method and is unsubscriptable");
			}
			if (base.IsUnbound)
			{
				return builtinFunction;
			}
			return new BuiltinFunction(_instance, builtinFunction._data);
		}
	}

	internal override bool IsOnlyGeneric
	{
		get
		{
			foreach (MethodBase target in base.Targets)
			{
				if (!target.IsGenericMethod || !target.ContainsGenericParameters)
				{
					return false;
				}
			}
			return true;
		}
	}

	internal GenericBuiltinFunction(string name, MethodBase[] originalTargets, Type declaringType, FunctionType functionType)
		: base(name, originalTargets, declaringType, functionType)
	{
	}

	internal GenericBuiltinFunction(object instance, BuiltinFunctionData data)
		: base(instance, data)
	{
	}

	internal override BuiltinFunction BindToInstance(object instance)
	{
		return new GenericBuiltinFunction(instance, _data);
	}
}
