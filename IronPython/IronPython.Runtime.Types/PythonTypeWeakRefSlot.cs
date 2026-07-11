using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types;

[PythonType("getset_descriptor")]
public sealed class PythonTypeWeakRefSlot : PythonTypeSlot, ICodeFormattable
{
	private PythonType _type;

	public PythonTypeWeakRefSlot(PythonType parent)
	{
		_type = parent;
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		if (instance == null)
		{
			value = this;
			return true;
		}
		if (instance is IWeakReferenceable weakReferenceable)
		{
			WeakRefTracker weakRef = weakReferenceable.GetWeakRef();
			if (weakRef == null || weakRef.HandlerCount == 0)
			{
				value = null;
			}
			else
			{
				value = weakRef.GetHandlerCallback(0);
			}
			return true;
		}
		value = null;
		return false;
	}

	internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value)
	{
		if (instance is IWeakReferenceable weakReferenceable)
		{
			return weakReferenceable.SetWeakRef(new WeakRefTracker(value, instance));
		}
		return false;
	}

	internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner)
	{
		throw PythonOps.TypeError("__weakref__ attribute cannot be deleted");
	}

	public override string ToString()
	{
		return $"<attribute '__weakref__' of '{_type.Name}' objects>";
	}

	public string __repr__(CodeContext context)
	{
		return $"<attribute '__weakref__' of {PythonOps.Repr(context, _type)} objects";
	}
}
