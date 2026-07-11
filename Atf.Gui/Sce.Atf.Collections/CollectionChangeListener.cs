using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sce.Atf.Collections;

public class CollectionChangeListener : ChangeListener
{
	private readonly INotifyCollectionChanged m_value;

	private readonly Dictionary<INotifyPropertyChanged, ChangeListener> m_collectionListeners = new Dictionary<INotifyPropertyChanged, ChangeListener>();

	public CollectionChangeListener(INotifyCollectionChanged collection, string propertyName)
	{
		m_value = collection;
		PropertyName = propertyName;
		Subscribe();
	}

	private void Subscribe()
	{
		m_value.CollectionChanged += ValueCollectionChanged;
		foreach (INotifyPropertyChanged item in (IEnumerable)m_value)
		{
			ResetChildListener(item);
		}
	}

	private void ResetChildListener(INotifyPropertyChanged item)
	{
		Requires.NotNull(item, "item");
		RemoveItem(item);
		ChangeListener changeListener = null;
		changeListener = ((!(item is INotifyCollectionChanged)) ? ((ChangeListener)new ChildChangeListener(item)) : ((ChangeListener)new CollectionChangeListener(item as INotifyCollectionChanged, PropertyName)));
		changeListener.PropertyChanged += ListenerPropertyChanged;
		m_collectionListeners.Add(item, changeListener);
	}

	private void RemoveItem(INotifyPropertyChanged item)
	{
		if (m_collectionListeners.ContainsKey(item))
		{
			m_collectionListeners[item].PropertyChanged -= ListenerPropertyChanged;
			m_collectionListeners[item].Dispose();
			m_collectionListeners.Remove(item);
		}
	}

	private void ClearCollection()
	{
		foreach (INotifyPropertyChanged key in m_collectionListeners.Keys)
		{
			m_collectionListeners[key].Dispose();
		}
		m_collectionListeners.Clear();
	}

	private void ValueCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			ClearCollection();
			return;
		}
		if (e.OldItems != null)
		{
			foreach (INotifyPropertyChanged oldItem in e.OldItems)
			{
				RemoveItem(oldItem);
			}
		}
		if (e.NewItems == null)
		{
			return;
		}
		foreach (INotifyPropertyChanged newItem in e.NewItems)
		{
			ResetChildListener(newItem);
		}
	}

	private void ListenerPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		RaisePropertyChanged(sender, string.Format("{0}{1}{2}", PropertyName, (PropertyName != null) ? "[]." : null, e.PropertyName));
	}

	protected override void Unsubscribe()
	{
		ClearCollection();
		m_value.CollectionChanged -= ValueCollectionChanged;
	}
}
