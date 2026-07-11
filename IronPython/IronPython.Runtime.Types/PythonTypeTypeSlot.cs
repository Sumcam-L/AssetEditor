using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types;

public class PythonTypeTypeSlot : PythonTypeDataSlot
{
	public static string __doc__ = "the object's class";

	internal override bool GetAlwaysSucceeds => true;

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		if (instance == null)
		{
			if (owner == TypeCache.Null)
			{
				value = owner;
			}
			else
			{
				value = DynamicHelpers.GetPythonType(owner);
			}
		}
		else
		{
			value = DynamicHelpers.GetPythonType(instance);
		}
		return true;
	}

	internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value)
	{
		if (instance == null)
		{
			return false;
		}
		if (!(instance is IPythonObject pythonObject))
		{
			throw PythonOps.TypeError("__class__ assignment: only for user defined types");
		}
		if (!(value is PythonType pythonType))
		{
			throw PythonOps.TypeError("__class__ must be set to new-style class, not '{0}' object", DynamicHelpers.GetPythonType(value).Name);
		}
		if (pythonType.UnderlyingSystemType != DynamicHelpers.GetPythonType(instance).UnderlyingSystemType)
		{
			throw PythonOps.TypeErrorForIncompatibleObjectLayout("__class__ assignment", DynamicHelpers.GetPythonType(instance), pythonType.UnderlyingSystemType);
		}
		pythonObject.SetPythonType(pythonType);
		return true;
	}

	internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner)
	{
		throw PythonOps.AttributeErrorForReadonlyAttribute(PythonTypeOps.GetName(instance), "__class__");
	}
}
