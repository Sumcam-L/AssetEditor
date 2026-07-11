using System.Diagnostics;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime;

[DebuggerDisplay("{Value}", Name = "{GetName(),nq}", Type = "{GetClassName(),nq}")]
internal class ObjectDebugView
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string _name;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly object _value;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public object Value => _value;

	public ObjectDebugView(object name, object value)
	{
		_name = name.ToString();
		_value = value;
	}

	public string GetClassName()
	{
		return PythonTypeOps.GetName(_value);
	}

	public string GetName()
	{
		return _name;
	}
}
