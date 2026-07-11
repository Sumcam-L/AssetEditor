using System;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class PythonModuleAttribute : Attribute
{
	private readonly string _name;

	private readonly Type _type;

	public string Name => _name;

	public Type Type => _type;

	public PythonModuleAttribute(string name, Type type)
	{
		ContractUtils.RequiresNotNull(name, "name");
		ContractUtils.RequiresNotNull(type, "type");
		_name = name;
		_type = type;
	}
}
