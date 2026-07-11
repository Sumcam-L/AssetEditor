using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types;

[PythonType("getset_descriptor")]
public sealed class PythonTypeDictSlot : PythonTypeSlot, ICodeFormattable
{
	private PythonType _type;

	public PythonTypeDictSlot(PythonType type)
	{
		_type = type;
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		if (instance == null)
		{
			value = new DictProxy(owner);
			return true;
		}
		if (instance is PythonType dt)
		{
			value = new DictProxy(dt);
			return true;
		}
		if (instance is IPythonObject pythonObject && pythonObject.PythonType.HasDictionary)
		{
			PythonDictionary pythonDictionary = pythonObject.Dict;
			if (pythonDictionary != null || (pythonDictionary = pythonObject.SetDict(pythonObject.PythonType.MakeDictionary())) != null)
			{
				value = pythonDictionary;
				return true;
			}
		}
		value = null;
		return false;
	}

	internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value)
	{
		if (instance is IPythonObject pythonObject)
		{
			if (!(value is PythonDictionary))
			{
				throw PythonOps.TypeError("__dict__ must be set to a dictionary, not '{0}'", owner.Name);
			}
			if (!pythonObject.PythonType.HasDictionary)
			{
				return false;
			}
			pythonObject.ReplaceDict((PythonDictionary)value);
			return true;
		}
		if (instance == null)
		{
			throw PythonOps.AttributeError("'__dict__' of '{0}' objects is not writable", owner.Name);
		}
		return false;
	}

	internal override bool IsSetDescriptor(CodeContext context, PythonType owner)
	{
		return true;
	}

	internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner)
	{
		if (instance is IPythonObject pythonObject)
		{
			if (!pythonObject.PythonType.HasDictionary)
			{
				return false;
			}
			pythonObject.ReplaceDict(null);
			return true;
		}
		if (instance == null)
		{
			throw PythonOps.TypeError("'__dict__' of '{0}' objects is not writable", owner.Name);
		}
		return false;
	}

	public string __repr__(CodeContext context)
	{
		return $"<attribute '__dict__' of '{_type.Name}' objects>";
	}
}
