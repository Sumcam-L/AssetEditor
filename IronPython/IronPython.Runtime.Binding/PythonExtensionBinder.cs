using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class PythonExtensionBinder : PythonBinder
{
	private readonly ExtensionMethodSet _extMethodSet;

	public PythonExtensionBinder(PythonBinder binder, ExtensionMethodSet extensionMethods)
		: base(binder)
	{
		_extMethodSet = extensionMethods;
	}

	public override MemberGroup GetMember(MemberRequestKind actionKind, Type type, string name)
	{
		MemberGroup member = base.GetMember(actionKind, type, name);
		if (member.Count == 0)
		{
			List<MemberTracker> list = new List<MemberTracker>();
			foreach (MethodInfo extensionMethod in _extMethodSet.GetExtensionMethods(name))
			{
				ParameterInfo[] parameters = extensionMethod.GetParameters();
				if (parameters.Length != 0)
				{
					Type parameterType = parameters[0].ParameterType;
					if (IsApplicableExtensionMethod(type, parameterType))
					{
						list.Add(MemberTracker.FromMemberInfo(extensionMethod, parameterType));
					}
				}
			}
			if (list.Count > 0)
			{
				return new MemberGroup(list.ToArray());
			}
		}
		return member;
	}

	internal static bool IsApplicableExtensionMethod(Type instanceType, Type extensionMethodThisType)
	{
		if (extensionMethodThisType.ContainsGenericParameters())
		{
			Dictionary<Type, Type> binding = new Dictionary<Type, Type>();
			if (extensionMethodThisType.IsArray)
			{
				if (instanceType.IsArray)
				{
					extensionMethodThisType = extensionMethodThisType.GetElementType();
					instanceType = instanceType.GetElementType();
					Type inferedType = TypeInferer.GetInferedType(extensionMethodThisType, extensionMethodThisType, instanceType, instanceType, binding);
					return inferedType != null;
				}
				return false;
			}
			Type[] array = (extensionMethodThisType.IsGenericParameter ? new Type[1] { extensionMethodThisType } : extensionMethodThisType.GetGenericArguments());
			Type[] array2 = new Type[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				if ((array2[i] = TypeInferer.GetInferedType(array[i], extensionMethodThisType, instanceType, instanceType, binding)) == null)
				{
					array2 = null;
					break;
				}
			}
			if (array2 != null)
			{
				if (extensionMethodThisType.IsGenericParameter)
				{
					extensionMethodThisType = array2[0];
				}
				else if (array2.Length > 0)
				{
					extensionMethodThisType = extensionMethodThisType.GetGenericTypeDefinition().MakeGenericType(array2);
				}
			}
		}
		return extensionMethodThisType.IsAssignableFrom(instanceType);
	}
}
