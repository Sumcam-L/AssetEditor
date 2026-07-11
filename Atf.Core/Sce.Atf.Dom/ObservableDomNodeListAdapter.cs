using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public class ObservableDomNodeListAdapter<T> : DomNodeListAdapter<T>, IObservableCollection<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, INotifyPropertyChanged, INotifyCollectionChanged, IObservableCollection, IList, ICollection where T : class
{
	private object m_syncRoot;

	private readonly ChildInfo m_childInfo;

	private readonly DomNode m_node;

	private static PropertyChangedEventArgs s_countPropertyChangedArgs = new PropertyChangedEventArgs("Count");

	private static PropertyChangedEventArgs s_indexerPropertyChangedArgs = new PropertyChangedEventArgs("[]");

	bool IList.IsFixedSize => false;

	object IList.this[int index]
	{
		get
		{
			return base[index];
		}
		set
		{
			base[index] = (T)value;
		}
	}

	bool ICollection.IsSynchronized => false;

	int ICollection.Count => base.Count;

	object ICollection.SyncRoot => m_syncRoot ?? (m_syncRoot = new object());

	public event PropertyChangedEventHandler PropertyChanged;

	public event NotifyCollectionChangedEventHandler CollectionChanged;

	public ObservableDomNodeListAdapter(DomNode node, ChildInfo childInfo)
		: base(node, childInfo)
	{
		m_node = node;
		m_childInfo = childInfo;
		node.ChildInserted += Node_ChildInserted;
		node.ChildRemoved += Node_ChildRemoved;
	}

	private void Node_ChildRemoved(object sender, ChildEventArgs e)
	{
		OnChildrenChanged(e, NotifyCollectionChangedAction.Remove);
	}

	private void Node_ChildInserted(object sender, ChildEventArgs e)
	{
		OnChildrenChanged(e, NotifyCollectionChangedAction.Add);
	}

	private void OnChildrenChanged(ChildEventArgs e, NotifyCollectionChangedAction action)
	{
		if (e.ChildInfo.IsEquivalent(m_childInfo) && e.Parent == m_node)
		{
			T changedItem = e.Child.As<T>();
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItem, e.Index));
			OnPropertyChanged(s_countPropertyChangedArgs);
			OnPropertyChanged(s_indexerPropertyChangedArgs);
		}
	}

	protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		this.CollectionChanged?.Invoke(this, e);
	}

	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		this.PropertyChanged?.Invoke(this, e);
	}

	int IList.Add(object value)
	{
		T item = (T)value;
		Add(item);
		return IndexOf(item);
	}

	bool IList.Contains(object value)
	{
		return value is T item && Contains(item);
	}

	int IList.IndexOf(object value)
	{
		if (!(value is T item))
		{
			return -1;
		}
		if (!Contains(item))
		{
			return -1;
		}
		return IndexOf(item);
	}

	void IList.Insert(int index, object value)
	{
		Insert(index, (T)value);
	}

	void IList.Remove(object value)
	{
		Remove((T)value);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		for (int i = index; i < base.Count; i++)
		{
			array.SetValue(base[i], i);
		}
	}
}
