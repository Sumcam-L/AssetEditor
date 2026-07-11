using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types;

[PythonType("indexer#")]
public sealed class ReflectedIndexer : ReflectedGetterSetter
{
	private readonly object _instance;

	private readonly PropertyInfo _info;

	internal override bool GetAlwaysSucceeds => true;

	internal override Type DeclaringType => _info.DeclaringType;

	public override PythonType PropertyType
	{
		[PythonHidden]
		get
		{
			return DynamicHelpers.GetPythonTypeFromType(_info.PropertyType);
		}
	}

	public override string __name__ => _info.Name;

	public object this[SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>> storage, params object[] key]
	{
		get
		{
			return GetValue(DefaultContext.Default, storage, key);
		}
		set
		{
			if (!SetValue(DefaultContext.Default, storage, key, value))
			{
				throw PythonOps.AttributeErrorForReadonlyAttribute(DeclaringType.Name, __name__);
			}
		}
	}

	public ReflectedIndexer(PropertyInfo info, NameType nt, bool privateBinding)
		: base(new MethodInfo[1] { info.GetGetMethod(privateBinding) }, new MethodInfo[1] { info.GetSetMethod(privateBinding) }, nt)
	{
		_info = info;
	}

	public ReflectedIndexer(ReflectedIndexer from, object instance)
		: base(from)
	{
		_instance = instance;
		_info = from._info;
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		value = new ReflectedIndexer(this, instance);
		return true;
	}

	public bool SetValue(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>> storage, object[] keys, object value)
	{
		return CallSetter(context, storage, _instance, keys, value);
	}

	public object GetValue(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>> storage, object[] keys)
	{
		return CallGetter(context, storage, _instance, keys);
	}

	public new object __get__(CodeContext context, object instance, object owner)
	{
		TryGetValue(context, instance, owner as PythonType, out var value);
		return value;
	}
}
