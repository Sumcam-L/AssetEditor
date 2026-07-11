using System.Collections;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Operations;

public class ExtensibleString : Extensible<string>, ICodeFormattable, IStructuralEquatable
{
	public virtual object this[int index] => ScriptingRuntimeHelpers.CharToString(base.Value[index]);

	public object this[Slice slice] => StringOps.GetItem(base.Value, slice);

	public ExtensibleString()
		: base(string.Empty)
	{
	}

	public ExtensibleString(string self)
		: base(self)
	{
	}

	public override string ToString()
	{
		return base.Value;
	}

	public virtual string __repr__(CodeContext context)
	{
		return StringOps.Quote(base.Value);
	}

	[return: MaybeNotImplemented]
	public object __eq__(object other)
	{
		if (other is string || other is ExtensibleString || other is Bytes)
		{
			return ScriptingRuntimeHelpers.BooleanToObject(EqualsWorker(other));
		}
		return NotImplementedType.Value;
	}

	[return: MaybeNotImplemented]
	public object __ne__(object other)
	{
		if (other is string || other is ExtensibleString || other is Bytes)
		{
			return ScriptingRuntimeHelpers.BooleanToObject(!EqualsWorker(other));
		}
		return NotImplementedType.Value;
	}

	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		if (comparer is PythonContext.PythonEqualityComparer)
		{
			return GetHashCode();
		}
		return ((IStructuralEquatable)PythonTuple.MakeTuple(base.Value.ToCharArray())).GetHashCode(comparer);
	}

	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (comparer is PythonContext.PythonEqualityComparer)
		{
			return EqualsWorker(other);
		}
		if (other is ExtensibleString extensibleString)
		{
			return EqualsWorker(extensibleString.Value, comparer);
		}
		if (other is string other2)
		{
			return EqualsWorker(other2, comparer);
		}
		if (other is Bytes bytes)
		{
			return EqualsWorker(bytes.ToString(), comparer);
		}
		return false;
	}

	private bool EqualsWorker(object other)
	{
		if (other == null)
		{
			return false;
		}
		if (other is ExtensibleString extensibleString)
		{
			return base.Value == extensibleString.Value;
		}
		if (other is string text)
		{
			return base.Value == text;
		}
		if (other is Bytes bytes)
		{
			return base.Value == bytes.ToString();
		}
		return false;
	}

	private bool EqualsWorker(string other, IEqualityComparer comparer)
	{
		if (base.Value.Length != other.Length)
		{
			return false;
		}
		if (base.Value.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < base.Value.Length; i++)
		{
			if (!comparer.Equals(base.Value[i], other[i]))
			{
				return false;
			}
		}
		return true;
	}

	public object __getslice__(int start, int stop)
	{
		return StringOps.__getslice__(base.Value, start, stop);
	}

	public virtual int __len__()
	{
		return base.Value.Length;
	}

	public virtual bool __contains__(object value)
	{
		if (value is string)
		{
			return base.Value.Contains((string)value);
		}
		if (value is ExtensibleString)
		{
			return base.Value.Contains(((ExtensibleString)value).Value);
		}
		throw PythonOps.TypeErrorForBadInstance("expected string, got {0}", value);
	}
}
