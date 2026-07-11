using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

public abstract class ReflectedGetterSetter : PythonTypeSlot
{
	private MethodInfo[] _getter;

	private MethodInfo[] _setter;

	private readonly NameType _nameType;

	private BuiltinFunction _getfunc;

	private BuiltinFunction _setfunc;

	internal abstract Type DeclaringType { get; }

	public abstract string __name__ { get; }

	public PythonType __objclass__ => DynamicHelpers.GetPythonTypeFromType(DeclaringType);

	internal MethodInfo[] Getter => _getter;

	internal MethodInfo[] Setter => _setter;

	public virtual PythonType PropertyType
	{
		[PythonHidden]
		get
		{
			if (Getter != null && Getter.Length > 0)
			{
				return DynamicHelpers.GetPythonTypeFromType(Getter[0].ReturnType);
			}
			return DynamicHelpers.GetPythonTypeFromType(typeof(object));
		}
	}

	internal NameType NameType => _nameType;

	internal override bool IsAlwaysVisible => _nameType == NameType.PythonProperty;

	protected ReflectedGetterSetter(MethodInfo[] getter, MethodInfo[] setter, NameType nt)
	{
		_getter = RemoveNullEntries(getter);
		_setter = RemoveNullEntries(setter);
		_nameType = nt;
	}

	protected ReflectedGetterSetter(ReflectedGetterSetter from)
	{
		_getter = from._getter;
		_setter = from._setter;
		_nameType = from._nameType;
	}

	internal void AddGetter(MethodInfo mi)
	{
		lock (this)
		{
			_getter = ArrayUtils.Append(_getter, mi);
			MakeGetFunc();
		}
	}

	private void MakeGetFunc()
	{
		_getfunc = PythonTypeOps.GetBuiltinFunction(DeclaringType, __name__, _getter);
	}

	internal void AddSetter(MethodInfo mi)
	{
		lock (this)
		{
			_setter = ArrayUtils.Append(_setter, mi);
			MakeSetFunc();
		}
	}

	private void MakeSetFunc()
	{
		_setfunc = PythonTypeOps.GetBuiltinFunction(DeclaringType, __name__, _setter);
	}

	internal object CallGetter(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>> storage, object instance, object[] args)
	{
		if (NeedToReturnProperty(instance, Getter))
		{
			return this;
		}
		if (Getter.Length == 0)
		{
			throw new MissingMemberException("unreadable property");
		}
		if (_getfunc == null)
		{
			lock (this)
			{
				if (_getfunc == null)
				{
					MakeGetFunc();
				}
			}
		}
		return _getfunc.Call(context, storage, instance, args);
	}

	internal object CallTarget(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>> storage, MethodInfo[] targets, object instance, params object[] args)
	{
		BuiltinFunction builtinFunction = PythonTypeOps.GetBuiltinFunction(DeclaringType, __name__, targets);
		return builtinFunction.Call(context, storage, instance, args);
	}

	internal static bool NeedToReturnProperty(object instance, MethodInfo[] mis)
	{
		if (instance == null)
		{
			if (mis.Length == 0)
			{
				return true;
			}
			foreach (MethodInfo methodInfo in mis)
			{
				if (!methodInfo.IsStatic || (methodInfo.IsDefined(typeof(PropertyMethodAttribute), inherit: true) && !methodInfo.IsDefined(typeof(StaticExtensionMethodAttribute), inherit: true) && !methodInfo.IsDefined(typeof(WrapperDescriptorAttribute), inherit: true)))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal bool CallSetter(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>> storage, object instance, object[] args, object value)
	{
		if (NeedToReturnProperty(instance, Setter))
		{
			return false;
		}
		if (_setfunc == null)
		{
			lock (this)
			{
				if (_setfunc == null)
				{
					MakeSetFunc();
				}
			}
		}
		if (args.Length != 0)
		{
			_setfunc.Call(context, storage, instance, ArrayUtils.Append(args, value));
		}
		else
		{
			_setfunc.Call(context, storage, instance, new object[1] { value });
		}
		return true;
	}

	private static MethodInfo[] RemoveNullEntries(MethodInfo[] mis)
	{
		List<MethodInfo> list = null;
		for (int i = 0; i < mis.Length; i++)
		{
			if (mis[i] == null)
			{
				if (list == null)
				{
					list = new List<MethodInfo>();
					for (int j = 0; j < i; j++)
					{
						list.Add(mis[j]);
					}
				}
			}
			else
			{
				list?.Add(mis[i]);
			}
		}
		if (list != null)
		{
			return list.ToArray();
		}
		return mis;
	}
}
