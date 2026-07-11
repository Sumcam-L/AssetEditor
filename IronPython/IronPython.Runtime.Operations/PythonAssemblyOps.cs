using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Operations;

public static class PythonAssemblyOps
{
	private static readonly object _key = new object();

	private static Dictionary<Assembly, TopNamespaceTracker> GetAssemblyMap(PythonContext context)
	{
		return context.GetOrCreateModuleState(_key, () => new Dictionary<Assembly, TopNamespaceTracker>());
	}

	[SpecialName]
	public static object GetBoundMember(CodeContext context, Assembly self, string name)
	{
		TopNamespaceTracker reflectedAssembly = GetReflectedAssembly(context, self);
		if (name == "__dict__")
		{
			return new PythonDictionary(new WrapperDictionaryStorage(reflectedAssembly));
		}
		MemberTracker memberTracker = reflectedAssembly.TryGetPackageAny(name);
		if (memberTracker != null)
		{
			if (memberTracker.MemberType == TrackerTypes.Type)
			{
				return DynamicHelpers.GetPythonTypeFromType(((TypeTracker)memberTracker).Type);
			}
			return memberTracker;
		}
		return OperationFailed.Value;
	}

	[SpecialName]
	public static List GetMemberNames(CodeContext context, Assembly self)
	{
		List memberNames = DynamicHelpers.GetPythonTypeFromType(self.GetType()).GetMemberNames(context);
		foreach (string key in GetReflectedAssembly(context, self).Keys)
		{
			if (key is string)
			{
				memberNames.AddNoLock(key);
			}
		}
		return memberNames;
	}

	public static object __repr__(Assembly self)
	{
		return "<Assembly " + self.FullName + ">";
	}

	private static TopNamespaceTracker GetReflectedAssembly(CodeContext context, Assembly assem)
	{
		Dictionary<Assembly, TopNamespaceTracker> assemblyMap = GetAssemblyMap(context.LanguageContext);
		lock (assemblyMap)
		{
			if (assemblyMap.TryGetValue(assem, out var value))
			{
				return value;
			}
			value = new TopNamespaceTracker(context.LanguageContext.DomainManager);
			value.LoadAssembly(assem);
			assemblyMap[assem] = value;
			return value;
		}
	}
}
