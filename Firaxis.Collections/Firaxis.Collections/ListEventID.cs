using System.Collections.Generic;

namespace Firaxis.Collections;

public class ListEventID<T> : ListEvent<T> where T : IUniqueID
{
	private int lastID;

	public ListEventID()
	{
		ListenAdd();
	}

	public ListEventID(IEnumerable<T> collection)
		: base(collection)
	{
		ListenAdd();
		foreach (T item in collection)
		{
			ApplyNewID(item);
		}
	}

	public ListEventID(int capacity)
		: base(capacity)
	{
		ListenAdd();
	}

	private void ListenAdd()
	{
		base.AddedItem += ListEventID_AddedItem;
		base.ClearedItems += ListEventID_ClearedItems;
	}

	public T Find(int id)
	{
		return Find((T a) => a.ID == id);
	}

	private void ListEventID_ClearedItems(object sender, ListEventArgs e)
	{
		lastID = 0;
	}

	private void ListEventID_AddedItem(object sender, ListEventArgs e)
	{
		ApplyNewID(e.Item);
		if (e.Collection == null)
		{
			return;
		}
		foreach (T item in e.Collection)
		{
			ApplyNewID(item);
		}
	}

	private void ApplyNewID(T item)
	{
		if (item != null && item.ID == 0)
		{
			while (lastID == 0 || Find(lastID) != null)
			{
				lastID++;
			}
			int iD = lastID;
			item.ID = iD;
		}
	}
}
