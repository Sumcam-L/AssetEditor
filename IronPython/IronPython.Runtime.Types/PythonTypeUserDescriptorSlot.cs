using System;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types;

public sealed class PythonTypeUserDescriptorSlot : PythonTypeSlot
{
	private const int UserDescriptorFalse = -1;

	private object _value;

	private int _descVersion;

	private PythonTypeSlot _desc;

	internal object Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	internal PythonTypeUserDescriptorSlot(object value)
	{
		_value = value;
	}

	internal PythonTypeUserDescriptorSlot(object value, bool isntDescriptor)
	{
		_value = value;
		if (isntDescriptor)
		{
			_descVersion = -1;
		}
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		try
		{
			value = PythonOps.GetUserDescriptor(Value, instance, owner);
			return true;
		}
		catch (MissingMemberException)
		{
			value = null;
			return false;
		}
	}

	internal object GetValue(CodeContext context, object instance, PythonType owner)
	{
		if (_descVersion == -1)
		{
			return _value;
		}
		if (_descVersion != DynamicHelpers.GetPythonType(_value).Version)
		{
			CalculateDescriptorInfo();
			if (_descVersion == -1)
			{
				return _value;
			}
		}
		_desc.TryGetValue(context, _value, DynamicHelpers.GetPythonType(_value), out var value);
		return PythonContext.GetContext(context).Call(context, value, instance, owner);
	}

	private void CalculateDescriptorInfo()
	{
		PythonType pythonType = DynamicHelpers.GetPythonType(_value);
		if (!pythonType.IsSystemType)
		{
			_descVersion = pythonType.Version;
			if (!pythonType.TryResolveSlot(pythonType.Context.SharedClsContext, "__get__", out _desc))
			{
				_descVersion = -1;
			}
		}
		else
		{
			_descVersion = -1;
		}
	}

	internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner)
	{
		return PythonOps.TryDeleteUserDescriptor(Value, instance);
	}

	internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value)
	{
		return PythonOps.TrySetUserDescriptor(Value, instance, value);
	}

	internal override bool IsSetDescriptor(CodeContext context, PythonType owner)
	{
		object ret;
		return PythonOps.TryGetBoundAttr(context, Value, "__set__", out ret);
	}
}
