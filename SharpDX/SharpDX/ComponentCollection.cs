using System.Collections;
using System.Collections.Generic;

namespace SharpDX;

public abstract class ComponentCollection<T> : IEnumerable<T>, IEnumerable where T : ComponentBase
{
	protected internal readonly List<T> Items;

	private readonly Dictionary<string, T> mapItems;

	protected int Capacity
	{
		get
		{
			return Items.Capacity;
		}
		set
		{
			Items.Capacity = value;
		}
	}

	public int Count => Items.Count;

	public T this[int index]
	{
		get
		{
			if (index >= 0 && index < Items.Count)
			{
				return Items[index];
			}
			return null;
		}
	}

	public T this[string name]
	{
		get
		{
			if (!mapItems.TryGetValue(name, out var value))
			{
				return TryToGetOnNotFound(name);
			}
			return value;
		}
	}

	protected ComponentCollection()
	{
		Items = new List<T>();
		mapItems = new Dictionary<string, T>();
	}

	protected ComponentCollection(int capacity)
	{
		Items = new List<T>(capacity);
		mapItems = new Dictionary<string, T>(capacity);
	}

	protected internal T Add(T item)
	{
		Items.Add(item);
		mapItems.Add(item.Name, item);
		return item;
	}

	protected internal void Clear()
	{
		Items.Clear();
		mapItems.Clear();
	}

	public bool Contains(string name)
	{
		return mapItems.ContainsKey(name);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return Items.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	protected virtual T TryToGetOnNotFound(string name)
	{
		return null;
	}
}
