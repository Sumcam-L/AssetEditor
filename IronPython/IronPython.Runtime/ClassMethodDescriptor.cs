using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[PythonType("method_descriptor")]
[DontMapGetMemberNamesToDir]
public class ClassMethodDescriptor : PythonTypeSlot, ICodeFormattable
{
	internal readonly BuiltinFunction _func;

	internal ClassMethodDescriptor(BuiltinFunction func)
	{
		_func = func;
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		owner = CheckGetArgs(context, instance, owner);
		value = new Method(_func, owner, DynamicHelpers.GetPythonType(owner));
		return true;
	}

	private PythonType CheckGetArgs(CodeContext context, object instance, PythonType owner)
	{
		if (owner == null)
		{
			if (instance == null)
			{
				throw PythonOps.TypeError("__get__(None, None) is invalid");
			}
			owner = DynamicHelpers.GetPythonType(instance);
		}
		else if (!owner.IsSubclassOf(DynamicHelpers.GetPythonTypeFromType(_func.DeclaringType)))
		{
			throw PythonOps.TypeError("descriptor {0} for type {1} doesn't apply to type {2}", PythonOps.Repr(context, _func.Name), PythonOps.Repr(context, DynamicHelpers.GetPythonTypeFromType(_func.DeclaringType).Name), PythonOps.Repr(context, owner.Name));
		}
		if (instance != null)
		{
			BuiltinMethodDescriptor.CheckSelfWorker(context, instance, _func);
		}
		return owner;
	}

	public virtual string __repr__(CodeContext context)
	{
		BuiltinFunction func = _func;
		if (func != null)
		{
			return $"<method {PythonOps.Repr(context, func.Name)} of {PythonOps.Repr(context, DynamicHelpers.GetPythonTypeFromType(func.DeclaringType).Name)} objects>";
		}
		return $"<classmethod object at {IdDispenser.GetId(this)}>";
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ClassMethodDescriptor classMethodDescriptor))
		{
			return false;
		}
		return classMethodDescriptor._func == _func;
	}

	public override int GetHashCode()
	{
		return ~_func.GetHashCode();
	}
}
