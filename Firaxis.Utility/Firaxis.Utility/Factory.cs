using System;
using Firaxis.Collections;

namespace Firaxis.Utility;

public class Factory<T> : ListEvent<IMaker>
{
	public IMaker Find(string name)
	{
		return Find((IMaker a) => a.Name.CompareTo(name) == 0);
	}

	public T[] MakeAll()
	{
		int count = base.Count;
		T[] array = new T[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = (T)base[i].Make();
		}
		return array;
	}

	public T Make(string name)
	{
		IMaker maker = Find((IMaker a) => string.Compare(name, a.Name) == 0);
		if (maker != null)
		{
			return (T)maker.Make();
		}
		throw new Exception("Type does not exist");
	}

	public T Make(string name, params object[] args)
	{
		IMaker maker = Find((IMaker a) => string.Compare(name, a.Name) == 0);
		if (maker != null)
		{
			return (T)maker.Make(args);
		}
		return default(T);
	}
}
