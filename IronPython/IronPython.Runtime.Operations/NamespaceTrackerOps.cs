using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Operations;

public static class NamespaceTrackerOps
{
	[SpecialName]
	[PropertyMethod]
	public static object Get__file__(NamespaceTracker self)
	{
		if (self.PackageAssemblies.Count == 1)
		{
			return self.PackageAssemblies[0].FullName;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < self.PackageAssemblies.Count; i++)
		{
			if (i != 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(self.PackageAssemblies[i].FullName);
		}
		return stringBuilder.ToString();
	}

	public static string __repr__(NamespaceTracker self)
	{
		return __str__(self);
	}

	public static string __str__(NamespaceTracker self)
	{
		if (self.PackageAssemblies.Count != 1)
		{
			return $"<module '{Get__name__(self.Name)}' (CLS module, {self.PackageAssemblies.Count} assemblies loaded)>";
		}
		return $"<module '{Get__name__(self.Name)}' (CLS module from {self.PackageAssemblies[0].FullName})>";
	}

	[SpecialName]
	[PropertyMethod]
	public static PythonDictionary Get__dict__(CodeContext context, NamespaceTracker self)
	{
		PythonDictionary pythonDictionary = new PythonDictionary();
		foreach (KeyValuePair<string, object> item in self)
		{
			if (item.Value is TypeGroup || item.Value is NamespaceTracker)
			{
				pythonDictionary[item.Key] = item.Value;
			}
			else
			{
				pythonDictionary[item.Key] = DynamicHelpers.GetPythonTypeFromType(((TypeTracker)item.Value).Type);
			}
		}
		return pythonDictionary;
	}

	[SpecialName]
	[PropertyMethod]
	public static string Get__name__(CodeContext context, NamespaceTracker self)
	{
		return Get__name__(self.Name);
	}

	private static string Get__name__(string name)
	{
		int num = name.LastIndexOf('.');
		if (num == -1)
		{
			return name;
		}
		return name.Substring(num + 1);
	}

	[SpecialName]
	public static object GetCustomMember(CodeContext context, NamespaceTracker self, string name)
	{
		if (self.TryGetValue(name, out MemberTracker value))
		{
			if (value.MemberType == TrackerTypes.Namespace || value.MemberType == TrackerTypes.TypeGroup)
			{
				return value;
			}
			PythonTypeSlot slot = PythonTypeOps.GetSlot(new MemberGroup(value), name, PythonContext.GetContext(context).Binder.PrivateBinding);
			if (slot != null && slot.TryGetValue(context, null, TypeCache.PythonType, out var value2))
			{
				return value2;
			}
		}
		return OperationFailed.Value;
	}
}
