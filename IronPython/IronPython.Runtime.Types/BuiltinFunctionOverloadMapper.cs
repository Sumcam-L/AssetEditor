using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types;

public class BuiltinFunctionOverloadMapper : ICodeFormattable
{
	private BuiltinFunction _function;

	private object _instance;

	private PythonTuple _allOverloads;

	public object this[params Type[] types] => GetOverload(types, Targets);

	public BuiltinFunction Function => _function;

	public virtual IList<MethodBase> Targets => _function.Targets;

	public PythonTuple Functions
	{
		get
		{
			if (_allOverloads == null)
			{
				object[] array = new object[Targets.Count];
				int num = 0;
				foreach (MethodBase target in Targets)
				{
					ParameterInfo[] parameters = target.GetParameters();
					Type[] array2 = new Type[parameters.Length];
					for (int i = 0; i < parameters.Length; i++)
					{
						array2[i] = parameters[i].ParameterType;
					}
					array[num++] = GetOverload(array2, Targets, wrapCtors: false);
				}
				Interlocked.CompareExchange(ref _allOverloads, PythonTuple.MakeTuple(array), null);
			}
			return _allOverloads;
		}
	}

	public BuiltinFunctionOverloadMapper(BuiltinFunction builtinFunction, object instance)
	{
		_function = builtinFunction;
		_instance = instance;
	}

	protected object GetOverload(Type[] sig, IList<MethodBase> targets)
	{
		return GetOverload(sig, targets, wrapCtors: true);
	}

	private object GetOverload(Type[] sig, IList<MethodBase> targets, bool wrapCtors)
	{
		BuiltinFunction.TypeList key = new BuiltinFunction.TypeList(sig);
		BuiltinFunction value;
		lock (_function.OverloadDictionary)
		{
			if (!_function.OverloadDictionary.TryGetValue(key, out value))
			{
				MethodBase[] originalTargets = FindMatchingTargets(sig, targets);
				if (targets == null)
				{
					throw ScriptingRuntimeHelpers.SimpleTypeError(string.Format("No match found for the method signature {0}", sig));
				}
				value = (_function.OverloadDictionary[key] = new BuiltinFunction(_function.Name, originalTargets, Function.DeclaringType, _function.FunctionType));
			}
		}
		if (_instance != null)
		{
			return value.BindToInstance(_instance);
		}
		if (wrapCtors)
		{
			return GetTargetFunction(value);
		}
		return value;
	}

	private static MethodBase[] FindMatchingTargets(Type[] sig, IList<MethodBase> targets)
	{
		int num = sig.Length;
		List<MethodBase> list = new List<MethodBase>();
		foreach (MethodBase target in targets)
		{
			ParameterInfo[] parameters = target.GetParameters();
			if (parameters.Length != num)
			{
				continue;
			}
			bool flag = true;
			for (int i = 0; i < num; i++)
			{
				if (parameters[i].ParameterType != sig[i])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				list.Add(target);
			}
		}
		return list.ToArray();
	}

	protected virtual object GetTargetFunction(BuiltinFunction bf)
	{
		return bf;
	}

	public virtual string __repr__(CodeContext context)
	{
		PythonDictionary pythonDictionary = new PythonDictionary();
		foreach (MethodBase target in Targets)
		{
			string key = DocBuilder.CreateAutoDoc(target);
			pythonDictionary[key] = Function;
		}
		return pythonDictionary.__repr__(context);
	}
}
