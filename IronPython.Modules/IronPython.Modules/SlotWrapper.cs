using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Modules;

[PythonType("wrapper_descriptor")]
internal class SlotWrapper : PythonTypeSlot, ICodeFormattable
{
	private readonly string _name;

	private readonly PythonType _type;

	public SlotWrapper(string slotName, PythonType targetType)
	{
		_name = slotName;
		_type = targetType;
	}

	public virtual string __repr__(CodeContext context)
	{
		return $"<slot wrapper {PythonOps.Repr(context, _name)} of {PythonOps.Repr(context, _type.Name)} objects>";
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		if (instance == null)
		{
			value = this;
			return true;
		}
		if (!(instance is IProxyObject proxyObject))
		{
			throw PythonOps.TypeError("descriptor for {0} object doesn't apply to {1} object", PythonOps.Repr(context, _type.Name), PythonOps.Repr(context, PythonTypeOps.GetName(instance)));
		}
		if (!DynamicHelpers.GetPythonType(proxyObject.Target).TryGetBoundMember(context, proxyObject.Target, _name, out value))
		{
			return false;
		}
		value = new GenericMethodWrapper(_name, proxyObject);
		return true;
	}
}
