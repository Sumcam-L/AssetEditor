using System.Collections.Generic;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types;

[PythonType("member_descriptor")]
internal class ReflectedSlotProperty : PythonTypeDataSlot, ICodeFormattable
{
	private class SlotValue
	{
		public SlotGetValue Getter;

		public SlotSetValue Setter;
	}

	private readonly string _name;

	private readonly string _typeName;

	private readonly int _index;

	private static readonly Dictionary<int, SlotValue> _methods = new Dictionary<int, SlotValue>();

	private SlotValue Value
	{
		get
		{
			SlotValue value;
			lock (_methods)
			{
				if (!_methods.TryGetValue(_index, out value))
				{
					SlotValue slotValue = (_methods[_index] = new SlotValue());
					value = slotValue;
					return value;
				}
			}
			return value;
		}
	}

	internal SlotGetValue Getter
	{
		get
		{
			SlotValue value = Value;
			lock (value)
			{
				EnsureGetter(value);
				return value.Getter;
			}
		}
	}

	internal SlotSetValue Setter
	{
		get
		{
			SlotValue value = Value;
			lock (value)
			{
				EnsureSetter(value);
				return value.Setter;
			}
		}
	}

	internal int Index => _index;

	public ReflectedSlotProperty(string name, string typeName, int index)
	{
		_index = index;
		_name = name;
		_typeName = typeName;
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		if (instance != null)
		{
			value = Getter(instance);
			PythonOps.CheckInitializedAttribute(value, instance, _name);
			return true;
		}
		value = this;
		return true;
	}

	internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value)
	{
		if (instance != null)
		{
			Setter(instance, value);
			return true;
		}
		return false;
	}

	internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner)
	{
		return TrySetValue(context, instance, owner, Uninitialized.Instance);
	}

	public string __repr__(CodeContext context)
	{
		return $"<member '{_name}' of '{_typeName}' objects>";
	}

	private void EnsureGetter(SlotValue value)
	{
		if (value.Getter == null)
		{
			value.Getter = (object instance) => ((IPythonObject)instance).GetSlots()[_index];
		}
	}

	private void EnsureSetter(SlotValue value)
	{
		if (value.Setter == null)
		{
			value.Setter = delegate(object instance, object setvalue)
			{
				((IPythonObject)instance).GetSlots()[_index] = setvalue;
			};
		}
	}
}
