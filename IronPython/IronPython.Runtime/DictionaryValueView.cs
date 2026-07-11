using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime;

[PythonType("dict_values")]
public sealed class DictionaryValueView : IEnumerable<object>, IEnumerable, ICodeFormattable
{
	private readonly PythonDictionary _dict;

	internal DictionaryValueView(PythonDictionary dict)
	{
		_dict = dict;
	}

	[PythonHidden]
	public IEnumerator GetEnumerator()
	{
		return _dict.itervalues();
	}

	IEnumerator<object> IEnumerable<object>.GetEnumerator()
	{
		return new DictionaryValueEnumerator(_dict._storage);
	}

	public int __len__()
	{
		return _dict.Count;
	}

	public string __repr__(CodeContext context)
	{
		StringBuilder stringBuilder = new StringBuilder(20);
		stringBuilder.Append("dict_values([");
		string value = "";
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object current = enumerator.Current;
				stringBuilder.Append(value);
				value = ", ";
				stringBuilder.Append(PythonOps.Repr(context, current));
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		stringBuilder.Append("])");
		return stringBuilder.ToString();
	}
}
