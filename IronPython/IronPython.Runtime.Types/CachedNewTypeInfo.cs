using System;
using System.Collections.Generic;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

public class CachedNewTypeInfo
{
	private readonly Type _type;

	private readonly Dictionary<string, string[]> _specialNames;

	private readonly Type[] _interfaceTypes;

	public IList<Type> InterfaceTypes => _interfaceTypes;

	public Type Type => _type;

	public Dictionary<string, string[]> SpecialNames => _specialNames;

	public CachedNewTypeInfo(Type type, Dictionary<string, string[]> specialNames, Type[] interfaceTypes)
	{
		_type = type;
		_specialNames = specialNames;
		_interfaceTypes = interfaceTypes ?? ReflectionUtils.EmptyTypes;
	}
}
