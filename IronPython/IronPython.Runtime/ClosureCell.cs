using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[PythonType("cell")]
public sealed class ClosureCell : ICodeFormattable
{
	public const object __hash__ = null;

	[PythonHidden]
	public object Value;

	public object cell_contents
	{
		get
		{
			if (Value == Uninitialized.Instance)
			{
				throw PythonOps.ValueError("cell is empty");
			}
			return Value;
		}
	}

	internal ClosureCell(object value)
	{
		Value = value;
	}

	public string __repr__(CodeContext context)
	{
		return $"<cell at {IdDispenser.GetId(this)}: {GetContentsRepr()}>";
	}

	private string GetContentsRepr()
	{
		if (Value == Uninitialized.Instance)
		{
			return "empty";
		}
		return $"{PythonTypeOps.GetName(Value)} object at {IdDispenser.GetId(Value)}";
	}

	[Python3Warning("cell comparisons not supported in 3.x")]
	public int __cmp__(object other)
	{
		if (!(other is ClosureCell closureCell))
		{
			throw PythonOps.TypeError("cell.__cmp__(x,y) expected cell, got {0}", PythonTypeOps.GetName(other));
		}
		return PythonOps.Compare(Value, closureCell.Value);
	}
}
