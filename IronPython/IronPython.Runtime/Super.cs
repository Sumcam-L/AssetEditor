using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[PythonType("super")]
public class Super : PythonTypeSlot, ICodeFormattable
{
	private PythonType _thisClass;

	private object _self;

	private object _selfClass;

	public PythonType __thisclass__ => _thisClass;

	public object __self__ => _self;

	public object __self_class__ => _selfClass;

	private PythonType DescriptorContext
	{
		get
		{
			if (!DynamicHelpers.GetPythonType(_self).IsSubclassOf(_thisClass))
			{
				return _thisClass;
			}
			if (_selfClass is PythonType result)
			{
				return result;
			}
			return ((OldClass)_selfClass).TypeObject;
		}
	}

	private PythonType PythonType
	{
		get
		{
			if (GetType() == typeof(Super))
			{
				return TypeCache.Super;
			}
			IPythonObject pythonObject = this as IPythonObject;
			return pythonObject.PythonType;
		}
	}

	internal override bool GetAlwaysSucceeds => true;

	public void __init__(PythonType type)
	{
		__init__(type, null);
	}

	public void __init__(PythonType type, object obj)
	{
		if (obj != null)
		{
			PythonType pythonType = obj as PythonType;
			if (PythonOps.IsInstance(obj, type))
			{
				_thisClass = type;
				_self = obj;
				_selfClass = DynamicHelpers.GetPythonType(obj);
				return;
			}
			if (pythonType == null || !pythonType.IsSubclassOf(type))
			{
				throw PythonOps.TypeError("super(type, obj): obj must be an instance or subtype of type {1}, not {0}", PythonTypeOps.GetName(obj), type.Name);
			}
			_thisClass = type;
			_selfClass = obj;
			_self = obj;
		}
		else
		{
			_thisClass = type;
			_self = null;
			_selfClass = null;
		}
	}

	public new object __get__(CodeContext context, object instance, object owner)
	{
		PythonType pythonType = PythonType;
		if (pythonType == TypeCache.Super)
		{
			Super super = new Super();
			super.__init__(_thisClass, instance);
			return super;
		}
		return PythonCalls.Call(context, pythonType, _thisClass, instance);
	}

	[SpecialName]
	public object GetCustomMember(CodeContext context, string name)
	{
		object value;
		if (_selfClass is PythonType { ResolutionOrder: var resolutionOrder })
		{
			bool flag = false;
			int i;
			for (i = 0; i < resolutionOrder.Count; i++)
			{
				if (resolutionOrder[i] == _thisClass)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				i = 0;
				resolutionOrder = _thisClass.ResolutionOrder;
			}
			object self = ((_self == _selfClass) ? null : _self);
			for (i++; i < resolutionOrder.Count; i++)
			{
				if (TryLookupInBase(context, resolutionOrder[i], name, self, out value))
				{
					return value;
				}
			}
		}
		if (PythonType.TryGetBoundMember(context, this, name, out value))
		{
			return value;
		}
		return OperationFailed.Value;
	}

	[SpecialName]
	public void SetMember(CodeContext context, string name, object value)
	{
		PythonType.SetMember(context, this, name, value);
	}

	[SpecialName]
	public void DeleteCustomMember(CodeContext context, string name)
	{
		PythonType.DeleteMember(context, this, name);
	}

	private bool TryLookupInBase(CodeContext context, PythonType pt, string name, object self, out object value)
	{
		if (pt.OldClass == null)
		{
			if (pt.TryLookupSlot(context, name, out var slot) && slot.TryGetValue(context, self, DescriptorContext, out value))
			{
				return true;
			}
		}
		else
		{
			OldClass oldClass = pt.OldClass;
			if (PythonOps.TryGetBoundAttr(context, oldClass, name, out value))
			{
				value = OldClass.GetOldStyleDescriptor(context, value, self, DescriptorContext);
				return true;
			}
		}
		value = null;
		return false;
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		value = __get__(context, instance, owner);
		return true;
	}

	public string __repr__(CodeContext context)
	{
		return string.Format(arg2: (_self != this) ? PythonOps.Repr(context, _self) : "<super object>", format: "<{0}: {1}, {2}>", arg0: PythonTypeOps.GetName(this), arg1: PythonOps.Repr(context, _thisClass));
	}
}
