using System;

namespace IronPython.Runtime;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
public sealed class PythonTypeAttribute : Attribute
{
	private readonly string _name;

	public string Name => _name;

	public PythonTypeAttribute()
	{
	}

	public PythonTypeAttribute(string name)
	{
		_name = name;
	}
}
