using System;
using System.Collections.Generic;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

internal class NewTypeInfo
{
	private readonly Type _baseType;

	private readonly IList<Type> _interfaceTypes;

	private int? _hash;

	public Type BaseType => _baseType;

	public IList<Type> InterfaceTypes => _interfaceTypes;

	public NewTypeInfo(Type baseType, IList<Type> interfaceTypes)
	{
		_baseType = baseType;
		_interfaceTypes = interfaceTypes;
	}

	public static NewTypeInfo GetTypeInfo(string typeName, PythonTuple bases)
	{
		List<Type> list = new List<Type>();
		Type type = typeof(object);
		PythonType pythonType = null;
		foreach (PythonType pythonType2 in GetPythonTypes(typeName, bases))
		{
			IList<Type> list2 = ReflectionUtils.EmptyTypes;
			Type type2 = pythonType2.ExtensionType;
			if (pythonType2.ExtensionType.IsInterface)
			{
				list2 = new Type[1] { type2 };
				type2 = typeof(object);
			}
			else if (NewTypeMaker.IsInstanceType(type2))
			{
				list2 = new List<Type>();
				type2 = GetBaseTypeFromUserType(pythonType2, list2, type2.BaseType);
			}
			if (type2 == null || typeof(BuiltinFunction).IsAssignableFrom(type2) || typeof(PythonFunction).IsAssignableFrom(type2))
			{
				throw PythonOps.TypeError(typeName + ": {0} is not an acceptable base type", pythonType2.Name);
			}
			if (type2.ContainsGenericParameters)
			{
				throw PythonOps.TypeError(typeName + ": cannot inhert from open generic instantiation {0}. Only closed instantiations are supported.", pythonType2);
			}
			foreach (Type item in list2)
			{
				if (item.ContainsGenericParameters)
				{
					throw PythonOps.TypeError(typeName + ": cannot inhert from open generic instantiation {0}. Only closed instantiations are supported.", item);
				}
				list.Add(item);
			}
			if (!type.IsSubclassOf(type2))
			{
				if (type != typeof(object) && type != type2 && !type.IsDefined(typeof(DynamicBaseTypeAttribute), inherit: false) && !type2.IsSubclassOf(type))
				{
					throw PythonOps.TypeError(typeName + ": can only extend one CLI or builtin type, not both {0} (for {1}) and {2} (for {3})", type.FullName, pythonType, type2.FullName, pythonType2);
				}
				type = type2;
				pythonType = pythonType2;
			}
		}
		return new NewTypeInfo(type, (list.Count == 0) ? ReflectionUtils.EmptyTypes : list.ToArray());
	}

	private static IEnumerable<PythonType> GetPythonTypes(string typeName, ICollection<object> bases)
	{
		foreach (object curBaseType in bases)
		{
			if (!(curBaseType is PythonType curBasePythonType))
			{
				if (!(curBaseType is OldClass))
				{
					throw PythonOps.TypeError(typeName + ": unsupported base type for new-style class " + curBaseType);
				}
			}
			else
			{
				yield return curBasePythonType;
			}
		}
	}

	private static Type GetBaseTypeFromUserType(PythonType curBasePythonType, IList<Type> baseInterfaces, Type curTypeToExtend)
	{
		Queue<PythonType> queue = new Queue<PythonType>();
		queue.Enqueue(curBasePythonType);
		do
		{
			PythonType pythonType = queue.Dequeue();
			foreach (PythonType baseType in pythonType.BaseTypes)
			{
				if (!(baseType.ExtensionType == curTypeToExtend) && !curTypeToExtend.IsSubclassOf(baseType.ExtensionType))
				{
					if (baseType.ExtensionType.IsInterface)
					{
						baseInterfaces.Add(baseType.ExtensionType);
					}
					else if (NewTypeMaker.IsInstanceType(baseType.ExtensionType))
					{
						queue.Enqueue(baseType);
					}
					else if (!baseType.IsOldClass)
					{
						curTypeToExtend = null;
						break;
					}
				}
			}
		}
		while (queue.Count > 0);
		return curTypeToExtend;
	}

	public override int GetHashCode()
	{
		if (!_hash.HasValue)
		{
			int num = _baseType.GetHashCode();
			for (int i = 0; i < _interfaceTypes.Count; i++)
			{
				num ^= _interfaceTypes[i].GetHashCode();
			}
			_hash = num;
		}
		return _hash.Value;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is NewTypeInfo newTypeInfo))
		{
			return false;
		}
		if (_baseType.Equals(newTypeInfo._baseType) && _interfaceTypes.Count == newTypeInfo._interfaceTypes.Count)
		{
			for (int i = 0; i < _interfaceTypes.Count; i++)
			{
				if (!_interfaceTypes[i].Equals(newTypeInfo._interfaceTypes[i]))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}
}
