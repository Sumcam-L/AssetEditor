using System;
using System.Reflection;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

[PythonType("member_descriptor")]
public class ReflectedExtensionProperty : ReflectedGetterSetter
{
	private readonly MethodInfo _deleter;

	private readonly ExtensionPropertyInfo _extInfo;

	internal override bool GetAlwaysSucceeds
	{
		get
		{
			if (base.Getter.Length != 0)
			{
				return !base.Getter[0].IsDefined(typeof(WrapperDescriptorAttribute), inherit: false);
			}
			return false;
		}
	}

	internal override bool CanOptimizeGets => true;

	internal override Type DeclaringType => _extInfo.DeclaringType;

	internal ExtensionPropertyInfo ExtInfo => _extInfo;

	public override string __name__ => _extInfo.Name;

	public string __doc__ => DocBuilder.DocOneInfo(ExtInfo);

	public ReflectedExtensionProperty(ExtensionPropertyInfo info, NameType nt)
		: base(new MethodInfo[1] { info.Getter }, new MethodInfo[1] { info.Setter }, nt)
	{
		_extInfo = info;
		_deleter = info.Deleter;
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		if (base.Getter.Length == 0 || (instance == null && base.Getter[0].IsDefined(typeof(WrapperDescriptorAttribute), inherit: false)))
		{
			value = null;
			return false;
		}
		value = CallGetter(context, null, instance, ArrayUtils.EmptyObjects);
		return true;
	}

	internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value)
	{
		if (base.Setter.Length == 0 || instance == null)
		{
			return false;
		}
		return CallSetter(context, null, instance, ArrayUtils.EmptyObjects, value);
	}

	internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner)
	{
		if (_deleter == null || instance == null)
		{
			return base.TryDeleteValue(context, instance, owner);
		}
		CallTarget(context, null, new MethodInfo[1] { _deleter }, instance);
		return true;
	}

	internal override bool IsSetDescriptor(CodeContext context, PythonType owner)
	{
		return base.Setter.Length > 0;
	}

	public void __set__(CodeContext context, object instance, object value)
	{
		if (!TrySetValue(context, instance, DynamicHelpers.GetPythonType(instance), value))
		{
			throw PythonOps.TypeError("readonly attribute");
		}
	}

	public void __delete__(CodeContext context, object instance)
	{
		if (!TryDeleteValue(context, instance, DynamicHelpers.GetPythonType(instance)))
		{
			throw PythonOps.AttributeErrorForObjectMissingAttribute(instance, "__delete__");
		}
	}
}
