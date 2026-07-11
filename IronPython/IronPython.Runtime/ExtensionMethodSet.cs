using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

internal sealed class ExtensionMethodSet : IEquatable<ExtensionMethodSet>
{
	private sealed class AssemblyLoadInfo : IEquatable<AssemblyLoadInfo>
	{
		private static IEqualityComparer<HashSet<PythonType>> TypeComparer = CollectionUtils.CreateSetComparer<PythonType>();

		private static IEqualityComparer<HashSet<string>> StringComparer = CollectionUtils.CreateSetComparer<string>();

		public HashSet<PythonType> Types;

		public HashSet<string> Namespaces;

		public bool IsFullAssemblyLoaded;

		private readonly Assembly _asm;

		public AssemblyLoadInfo(Assembly asm)
		{
			_asm = asm;
		}

		public override int GetHashCode()
		{
			if (_asm != null)
			{
				return _asm.GetHashCode();
			}
			return 0;
		}

		public override bool Equals(object obj)
		{
			if (obj is AssemblyLoadInfo other)
			{
				return Equals(other);
			}
			return false;
		}

		public AssemblyLoadInfo EnsureTypesLoaded()
		{
			if (Namespaces != null || Types == null)
			{
				HashSet<PythonType> hashSet = new HashSet<PythonType>();
				HashSet<string> namespaces = Namespaces;
				Type[] exportedTypes = _asm.GetExportedTypes();
				foreach (Type type in exportedTypes)
				{
					if (type.IsExtension() && namespaces != null && namespaces.Contains(type.Namespace))
					{
						hashSet.Add(DynamicHelpers.GetPythonTypeFromType(type));
					}
				}
				AssemblyLoadInfo assemblyLoadInfo = new AssemblyLoadInfo(_asm);
				assemblyLoadInfo.Types = hashSet;
				if (namespaces == null)
				{
					assemblyLoadInfo.IsFullAssemblyLoaded = true;
				}
				return assemblyLoadInfo;
			}
			return this;
		}

		public bool Equals(AssemblyLoadInfo other)
		{
			if (this == other)
			{
				return true;
			}
			if (other == null || _asm != other._asm)
			{
				return false;
			}
			if (IsFullAssemblyLoaded && other.IsFullAssemblyLoaded)
			{
				return true;
			}
			if (TypeComparer.Equals(Types, other.Types))
			{
				return StringComparer.Equals(Namespaces, other.Namespaces);
			}
			return false;
		}
	}

	public const int OutOfIds = int.MinValue;

	private PythonExtensionBinder _extBinder;

	private Dictionary<Assembly, AssemblyLoadInfo> _loadedAssemblies;

	private int _id;

	private static int _curId;

	public static readonly ExtensionMethodSet Empty = new ExtensionMethodSet();

	public int Id => _id;

	private ExtensionMethodSet(Dictionary<Assembly, AssemblyLoadInfo> dict)
	{
		_loadedAssemblies = dict;
		if (_curId < 0 || (_id = Interlocked.Increment(ref _curId)) < 0)
		{
			_id = int.MinValue;
		}
	}

	public BindingRestrictions GetRestriction(Expression codeContext)
	{
		if (_id == int.MinValue)
		{
			return BindingRestrictions.GetInstanceRestriction(Expression.Call(typeof(PythonOps).GetMethod("GetExtensionMethodSet"), codeContext), this);
		}
		return BindingRestrictions.GetExpressionRestriction(Expression.Call(typeof(PythonOps).GetMethod("IsExtensionSet"), codeContext, Expression.Constant(_id)));
	}

	private ExtensionMethodSet()
	{
		_loadedAssemblies = new Dictionary<Assembly, AssemblyLoadInfo>();
	}

	public IEnumerable<MethodInfo> GetExtensionMethods(string name)
	{
		lock (this)
		{
			EnsureLoaded();
			foreach (KeyValuePair<Assembly, AssemblyLoadInfo> keyValue in _loadedAssemblies)
			{
				KeyValuePair<Assembly, AssemblyLoadInfo> keyValuePair = keyValue;
				AssemblyLoadInfo info = keyValuePair.Value;
				foreach (PythonType type in info.Types)
				{
					if (!type.ExtensionMethods.TryGetValue(name, out var methods))
					{
						continue;
					}
					foreach (MethodInfo item in methods)
					{
						yield return item;
					}
				}
			}
		}
	}

	private void EnsureLoaded()
	{
		bool flag = false;
		foreach (AssemblyLoadInfo value in _loadedAssemblies.Values)
		{
			if (value.Namespaces != null || value.Types == null)
			{
				flag = true;
			}
		}
		if (flag)
		{
			LoadAllTypes();
		}
	}

	public IEnumerable<MethodInfo> GetExtensionMethods(PythonType type)
	{
		lock (this)
		{
			EnsureLoaded();
			foreach (KeyValuePair<Assembly, AssemblyLoadInfo> keyValue in _loadedAssemblies)
			{
				KeyValuePair<Assembly, AssemblyLoadInfo> keyValuePair = keyValue;
				AssemblyLoadInfo info = keyValuePair.Value;
				foreach (PythonType containingType in info.Types)
				{
					foreach (List<MethodInfo> methodList in containingType.ExtensionMethods.Values)
					{
						foreach (MethodInfo method in methodList)
						{
							ParameterInfo[] methodParams = method.GetParameters();
							if (methodParams.Length != 0 && PythonExtensionBinder.IsApplicableExtensionMethod(type.UnderlyingSystemType, methodParams[0].ParameterType))
							{
								yield return method;
							}
						}
					}
				}
			}
		}
	}

	private void LoadAllTypes()
	{
		Dictionary<Assembly, AssemblyLoadInfo> dictionary = new Dictionary<Assembly, AssemblyLoadInfo>(_loadedAssemblies.Count);
		foreach (KeyValuePair<Assembly, AssemblyLoadInfo> loadedAssembly in _loadedAssemblies)
		{
			AssemblyLoadInfo value = loadedAssembly.Value;
			Assembly key = loadedAssembly.Key;
			dictionary[key] = value.EnsureTypesLoaded();
		}
		_loadedAssemblies = dictionary;
	}

	public static ExtensionMethodSet AddType(PythonContext context, ExtensionMethodSet existingSet, PythonType type)
	{
		lock (existingSet)
		{
			if (existingSet._loadedAssemblies.TryGetValue(type.UnderlyingSystemType.Assembly, out var value) && (value.IsFullAssemblyLoaded || (value.Types != null && value.Types.Contains(type)) || (value.Namespaces != null && value.Namespaces.Contains(type.UnderlyingSystemType.Namespace))))
			{
				return existingSet;
			}
			Dictionary<Assembly, AssemblyLoadInfo> dictionary = NewInfoOrCopy(existingSet);
			if (!dictionary.TryGetValue(type.UnderlyingSystemType.Assembly, out value))
			{
				value = (dictionary[type.UnderlyingSystemType.Assembly] = new AssemblyLoadInfo(type.UnderlyingSystemType.Assembly));
			}
			if (value.Types == null)
			{
				value.Types = new HashSet<PythonType>();
			}
			value.Types.Add(type);
			return context.UniqifyExtensions(new ExtensionMethodSet(dictionary));
		}
	}

	public static ExtensionMethodSet AddNamespace(PythonContext context, ExtensionMethodSet existingSet, NamespaceTracker ns)
	{
		lock (existingSet)
		{
			Dictionary<Assembly, AssemblyLoadInfo> dictionary = null;
			foreach (Assembly packageAssembly in ns.PackageAssemblies)
			{
				if (existingSet != null && existingSet._loadedAssemblies.TryGetValue(packageAssembly, out var value))
				{
					if (!value.IsFullAssemblyLoaded && (value.Namespaces == null || !value.Namespaces.Contains(ns.Name)))
					{
						if (dictionary == null)
						{
							dictionary = NewInfoOrCopy(existingSet);
						}
						if (dictionary[packageAssembly].Namespaces == null)
						{
							dictionary[packageAssembly].Namespaces = new HashSet<string>();
						}
						dictionary[packageAssembly].Namespaces.Add(ns.Name);
					}
				}
				else
				{
					if (dictionary == null)
					{
						dictionary = NewInfoOrCopy(existingSet);
					}
					AssemblyLoadInfo assemblyLoadInfo = (dictionary[packageAssembly] = new AssemblyLoadInfo(packageAssembly));
					AssemblyLoadInfo assemblyLoadInfo3 = assemblyLoadInfo;
					assemblyLoadInfo3.Namespaces = new HashSet<string>();
					assemblyLoadInfo3.Namespaces.Add(ns.Name);
				}
			}
			if (dictionary != null)
			{
				return context.UniqifyExtensions(new ExtensionMethodSet(dictionary));
			}
			return existingSet;
		}
	}

	public static bool operator ==(ExtensionMethodSet set1, ExtensionMethodSet set2)
	{
		return set1?.Equals(set2) ?? ((object)set2 == null);
	}

	public static bool operator !=(ExtensionMethodSet set1, ExtensionMethodSet set2)
	{
		return !(set1 == set2);
	}

	public PythonExtensionBinder GetBinder(PythonContext context)
	{
		if (_extBinder == null)
		{
			_extBinder = new PythonExtensionBinder(context.Binder, this);
		}
		return _extBinder;
	}

	private static Dictionary<Assembly, AssemblyLoadInfo> NewInfoOrCopy(ExtensionMethodSet existingSet)
	{
		Dictionary<Assembly, AssemblyLoadInfo> dictionary = new Dictionary<Assembly, AssemblyLoadInfo>();
		if (existingSet != null)
		{
			foreach (KeyValuePair<Assembly, AssemblyLoadInfo> loadedAssembly in existingSet._loadedAssemblies)
			{
				AssemblyLoadInfo assemblyLoadInfo = new AssemblyLoadInfo(loadedAssembly.Key);
				if (loadedAssembly.Value.Namespaces != null)
				{
					assemblyLoadInfo.Namespaces = new HashSet<string>(loadedAssembly.Value.Namespaces);
				}
				if (loadedAssembly.Value.Types != null)
				{
					assemblyLoadInfo.Types = new HashSet<PythonType>(loadedAssembly.Value.Types);
				}
				dictionary[loadedAssembly.Key] = assemblyLoadInfo;
			}
		}
		return dictionary;
	}

	public override bool Equals(object obj)
	{
		ExtensionMethodSet extensionMethodSet = obj as ExtensionMethodSet;
		if (extensionMethodSet != null)
		{
			return Equals(extensionMethodSet);
		}
		return false;
	}

	public bool Equals(ExtensionMethodSet other)
	{
		if (other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (_loadedAssemblies.Count != other._loadedAssemblies.Count)
		{
			return false;
		}
		foreach (KeyValuePair<Assembly, AssemblyLoadInfo> loadedAssembly in _loadedAssemblies)
		{
			Assembly key = loadedAssembly.Key;
			AssemblyLoadInfo value = loadedAssembly.Value;
			if (!other._loadedAssemblies.TryGetValue(key, out var value2))
			{
				return false;
			}
			if (value2 != value)
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int num = 6551;
		foreach (Assembly key in _loadedAssemblies.Keys)
		{
			num ^= key.GetHashCode();
		}
		return num;
	}
}
