using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors;

public class ListBoxScrollToEndBehavior : Behavior<ListBox>
{
	private INotifyCollectionChanged m_itemsSource;

	protected override void OnAttached()
	{
		base.OnAttached();
		Bind();
		DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ListBox)).AddValueChanged(base.AssociatedObject, ItemsSourceChanged);
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		Unbind();
		DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ListBox)).RemoveValueChanged(base.AssociatedObject, ItemsSourceChanged);
	}

	private void ItemsSourceChanged(object sender, EventArgs e)
	{
		Bind();
	}

	private void Unbind()
	{
		if (m_itemsSource != null)
		{
			m_itemsSource.CollectionChanged -= ItemsSource_CollectionChanged;
			m_itemsSource = null;
		}
	}

	private void Bind()
	{
		Unbind();
		m_itemsSource = base.AssociatedObject.ItemsSource as INotifyCollectionChanged;
		if (m_itemsSource != null)
		{
			m_itemsSource.CollectionChanged += ItemsSource_CollectionChanged;
		}
		if (base.AssociatedObject.Items.Count > 0)
		{
			SelectAndScrollIntoView(base.AssociatedObject.Items[base.AssociatedObject.Items.Count - 1]);
		}
	}

	private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		ItemCollection items = base.AssociatedObject.Items;
		object selectedItem = null;
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
		case NotifyCollectionChangedAction.Move:
			selectedItem = e.NewItems[e.NewItems.Count - 1];
			break;
		case NotifyCollectionChangedAction.Remove:
			if (items.Count < e.OldStartingIndex)
			{
				selectedItem = items[e.OldStartingIndex - 1];
			}
			else if (items.Count > 0)
			{
				selectedItem = items[0];
			}
			break;
		case NotifyCollectionChangedAction.Reset:
			if (items.Count > 0)
			{
				selectedItem = items[0];
			}
			break;
		}
		SelectAndScrollIntoView(selectedItem);
	}

	private void SelectAndScrollIntoView(object selectedItem)
	{
		if (selectedItem != null)
		{
			base.AssociatedObject.Items.MoveCurrentTo(selectedItem);
			base.AssociatedObject.ScrollIntoView(selectedItem);
		}
	}
}
