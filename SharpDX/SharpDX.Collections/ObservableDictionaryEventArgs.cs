using System;
using System.Collections.Generic;

namespace SharpDX.Collections;

public class ObservableDictionaryEventArgs<TKey, TValue> : EventArgs
{
	public TKey Key { get; private set; }

	public TValue Value { get; private set; }

	public ObservableDictionaryEventArgs(KeyValuePair<TKey, TValue> pair)
		: this(pair.Key, pair.Value)
	{
	}

	public ObservableDictionaryEventArgs(TKey key, TValue value)
	{
		Key = key;
		Value = value;
	}
}
