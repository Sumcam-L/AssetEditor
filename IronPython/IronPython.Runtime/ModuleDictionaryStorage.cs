using System;
using System.Collections.Generic;
using System.Reflection;
using IronPython.Compiler;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

internal class ModuleDictionaryStorage : GlobalDictionaryStorage
{
	private Type _type;

	private bool _cleared;

	private static readonly Dictionary<string, PythonGlobal> _emptyGlobalDict = new Dictionary<string, PythonGlobal>(0);

	private static readonly PythonGlobal[] _emptyGlobals = new PythonGlobal[0];

	public virtual BuiltinPythonModule Instance => null;

	public override int Count
	{
		get
		{
			GetItems();
			return base.Count;
		}
	}

	public ModuleDictionaryStorage(Type moduleType)
		: base(_emptyGlobalDict, _emptyGlobals)
	{
		_type = moduleType;
	}

	public ModuleDictionaryStorage(Type moduleType, Dictionary<string, PythonGlobal> globals)
		: base(globals, _emptyGlobals)
	{
		_type = moduleType;
	}

	public override bool Remove(ref DictionaryStorage storage, object key)
	{
		if (!(key is string name))
		{
			return base.Remove(ref storage, key);
		}
		bool result = base.Remove(ref storage, key);
		if (TryGetLazyValue(name, out var _))
		{
			Add(key, Uninitialized.Instance);
			result = true;
		}
		return result;
	}

	protected virtual void LazyAdd(object name, object value)
	{
		Add(name, value);
	}

	public override bool Contains(object key)
	{
		object value;
		return TryGetValue(key, out value);
	}

	public override void Clear(ref DictionaryStorage storage)
	{
		_cleared = true;
		base.Clear(ref storage);
	}

	public override List<KeyValuePair<object, object>> GetItems()
	{
		List<KeyValuePair<object, object>> list = new List<KeyValuePair<object, object>>();
		foreach (KeyValuePair<object, object> item in base.GetItems())
		{
			if (item.Value != Uninitialized.Instance)
			{
				list.Add(item);
			}
		}
		MemberInfo[] members = _type.GetMembers();
		MemberInfo[] array = members;
		foreach (MemberInfo memberInfo in array)
		{
			if (!base.Contains((object)memberInfo.Name) && TryGetLazyValue(memberInfo.Name, out var value))
			{
				list.Add(new KeyValuePair<object, object>(memberInfo.Name, value));
			}
		}
		return list;
	}

	private bool TryGetLazyValue(string name, out object value)
	{
		return TryGetLazyValue(name, publish: true, out value);
	}

	private bool TryGetLazyValue(string name, bool publish, out object value)
	{
		if (!_cleared)
		{
			MemberInfo[] array = NonHiddenMembers(GetMember(name));
			if (array.Length > 0)
			{
				switch (array[0].MemberType)
				{
				case MemberTypes.Field:
				{
					FieldInfo fieldInfo = (FieldInfo)array[0];
					if (fieldInfo.IsStatic)
					{
						value = ((FieldInfo)array[0]).GetValue(null);
						if (publish)
						{
							LazyAdd(name, value);
						}
						return true;
					}
					throw new InvalidOperationException("instance field declared on module.  Fields should stored as PythonGlobals, should be static readonly, or marked as PythonHidden.");
				}
				case MemberTypes.Method:
					if (!((MethodInfo)array[0]).IsSpecialName)
					{
						MethodInfo[] array2 = new MethodInfo[array.Length];
						FunctionType functionType = FunctionType.AlwaysVisible | FunctionType.ModuleMethod;
						for (int j = 0; j < array.Length; j++)
						{
							MethodInfo methodInfo = (MethodInfo)array[j];
							functionType = ((!methodInfo.IsStatic) ? (functionType | FunctionType.Method) : (functionType | FunctionType.Function));
							array2[j] = methodInfo;
						}
						BuiltinFunction builtinFunction = BuiltinFunction.MakeMethod(name, array2, array[0].DeclaringType, functionType);
						if ((functionType & FunctionType.Method) != FunctionType.None && Instance != null)
						{
							value = builtinFunction.BindToInstance(Instance);
						}
						else
						{
							value = builtinFunction;
						}
						if (publish)
						{
							LazyAdd(name, value);
						}
						return true;
					}
					break;
				case MemberTypes.Property:
				{
					PropertyInfo propertyInfo = (PropertyInfo)array[0];
					if ((propertyInfo.GetGetMethod() ?? propertyInfo.GetSetMethod()).IsStatic)
					{
						value = ((PropertyInfo)array[0]).GetValue(null, ArrayUtils.EmptyObjects);
						if (publish)
						{
							LazyAdd(name, value);
						}
						return true;
					}
					throw new InvalidOperationException("instance property declared on module.  Propreties should be declared as static, marked as PythonHidden, or you should use a PythonGlobal.");
				}
				case MemberTypes.NestedType:
					if (array.Length == 1)
					{
						value = DynamicHelpers.GetPythonTypeFromType((Type)array[0]);
					}
					else
					{
						TypeTracker typeTracker = (TypeTracker)MemberTracker.FromMemberInfo(array[0]);
						for (int i = 1; i < array.Length; i++)
						{
							typeTracker = TypeGroup.UpdateTypeEntity(typeTracker, (TypeTracker)MemberTracker.FromMemberInfo(array[i]));
						}
						value = typeTracker;
					}
					if (publish)
					{
						LazyAdd(name, value);
					}
					return true;
				}
			}
		}
		value = null;
		return false;
	}

	private static MemberInfo[] NonHiddenMembers(MemberInfo[] members)
	{
		List<MemberInfo> list = new List<MemberInfo>(members.Length);
		foreach (MemberInfo memberInfo in members)
		{
			if (!memberInfo.IsDefined(typeof(PythonHiddenAttribute), inherit: false))
			{
				list.Add(memberInfo);
			}
		}
		return list.ToArray();
	}

	private MemberInfo[] GetMember(string name)
	{
		return _type.GetMember(name, (BindingFlags)(0x12 | ((Instance == null) ? 8 : 12)));
	}

	public override bool TryGetValue(object key, out object value)
	{
		if (base.TryGetValue(key, out value))
		{
			return value != Uninitialized.Instance;
		}
		if (key is string name)
		{
			return TryGetLazyValue(name, out value);
		}
		return false;
	}

	public virtual void Reload()
	{
		foreach (KeyValuePair<object, object> item in base.GetItems())
		{
			if (item.Value == Uninitialized.Instance)
			{
				Remove(item.Key);
			}
			else if (item.Key is string name && GetMember(name).Length > 0)
			{
				Remove(item.Key);
			}
		}
	}
}
