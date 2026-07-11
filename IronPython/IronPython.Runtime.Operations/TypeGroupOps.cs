using System;
using System.Runtime.CompilerServices;
using System.Text;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class TypeGroupOps
{
	public static string __repr__(TypeGroup self)
	{
		StringBuilder stringBuilder = new StringBuilder("<types ");
		bool flag = false;
		foreach (Type type in self.Types)
		{
			if (flag)
			{
				stringBuilder.Append(", ");
			}
			PythonType pythonTypeFromType = DynamicHelpers.GetPythonTypeFromType(type);
			stringBuilder.Append('\'');
			stringBuilder.Append(pythonTypeFromType.Name);
			stringBuilder.Append('\'');
			flag = true;
		}
		stringBuilder.Append(">");
		return stringBuilder.ToString();
	}

	[SpecialName]
	public static PythonType GetItem(TypeGroup self, params PythonType[] types)
	{
		return GetItemHelper(self, types);
	}

	[SpecialName]
	public static object Call(CodeContext context, TypeGroup self, params object[] args)
	{
		return PythonCalls.Call(context, DynamicHelpers.GetPythonTypeFromType(self.GetNonGenericType()), args ?? ArrayUtils.EmptyObjects);
	}

	[SpecialName]
	public static object Call(CodeContext context, TypeGroup self, [ParamDictionary] PythonDictionary kwArgs, params object[] args)
	{
		return PythonCalls.CallWithKeywordArgs(context, DynamicHelpers.GetPythonTypeFromType(self.GetNonGenericType()), args ?? ArrayUtils.EmptyObjects, kwArgs ?? new PythonDictionary());
	}

	[SpecialName]
	public static PythonType GetItem(TypeGroup self, params object[] types)
	{
		PythonType[] array = new PythonType[types.Length];
		for (int i = 0; i < types.Length; i++)
		{
			object obj = types[i];
			if (obj is PythonType)
			{
				array[i] = (PythonType)obj;
				continue;
			}
			if (obj is TypeGroup)
			{
				TypeGroup typeGroup = obj as TypeGroup;
				if (!typeGroup.TryGetNonGenericType(out var nonGenericType))
				{
					throw PythonOps.TypeError("cannot use open generic type {0} as type argument", typeGroup.Name);
				}
				array[i] = DynamicHelpers.GetPythonTypeFromType(nonGenericType);
				continue;
			}
			throw PythonOps.TypeErrorForTypeMismatch("type", obj);
		}
		return GetItemHelper(self, array);
	}

	[SpecialName]
	public static PythonType GetItem(TypeGroup self, PythonTuple tuple)
	{
		if (tuple.__len__() == 0)
		{
			return DynamicHelpers.GetPythonTypeFromType(self.GetNonGenericType());
		}
		return GetItem(self, tuple._data);
	}

	private static PythonType GetItemHelper(TypeGroup self, PythonType[] types)
	{
		TypeTracker typeForArity = self.GetTypeForArity(types.Length);
		if (typeForArity == null)
		{
			throw new ValueErrorException($"could not find compatible generic type for {types.Length} type arguments");
		}
		Type type = typeForArity.Type;
		if (types.Length != 0)
		{
			type = type.MakeGenericType(PythonTypeOps.ConvertToTypes(types));
		}
		return DynamicHelpers.GetPythonTypeFromType(type);
	}
}
