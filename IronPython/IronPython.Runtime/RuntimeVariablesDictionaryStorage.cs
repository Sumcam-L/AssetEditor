using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

internal class RuntimeVariablesDictionaryStorage : CustomDictionaryStorage
{
	private readonly MutableTuple _boxes;

	private readonly string[] _args;

	internal MutableTuple Tuple => _boxes;

	internal string[] Names => _args;

	public RuntimeVariablesDictionaryStorage(MutableTuple boxes, string[] args)
	{
		_boxes = boxes;
		_args = args;
	}

	protected override IEnumerable<KeyValuePair<string, object>> GetExtraItems()
	{
		for (int i = 0; i < _args.Length; i++)
		{
			if (GetCell(i).Value != Uninitialized.Instance && _args[i] != null)
			{
				yield return new KeyValuePair<string, object>(_args[i], GetCell(i).Value);
			}
		}
	}

	protected override bool TrySetExtraValue(string key, object value)
	{
		for (int i = 0; i < _args.Length; i++)
		{
			if (_args[i] == key)
			{
				ClosureCell cell = GetCell(i);
				cell.Value = value;
				return true;
			}
		}
		return false;
	}

	protected override bool TryGetExtraValue(string key, out object value)
	{
		for (int i = 0; i < _args.Length; i++)
		{
			if (_args[i] == key)
			{
				value = GetCell(i).Value;
				return true;
			}
		}
		value = null;
		return false;
	}

	protected override bool? TryRemoveExtraValue(string key)
	{
		for (int i = 0; i < _args.Length; i++)
		{
			if (_args[i] == key)
			{
				ClosureCell cell = GetCell(i);
				if (cell.Value != Uninitialized.Instance)
				{
					cell.Value = Uninitialized.Instance;
					return true;
				}
				return false;
			}
		}
		return null;
	}

	internal ClosureCell GetCell(int i)
	{
		return (ClosureCell)_boxes.GetNestedValue(_args.Length, i);
	}
}
