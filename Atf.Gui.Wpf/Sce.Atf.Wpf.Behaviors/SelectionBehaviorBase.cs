using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors;

public abstract class SelectionBehaviorBase : Behavior<Selector>
{
	public static readonly DependencyProperty SelectionContextProperty = DependencyProperty.Register("SelectionContext", typeof(ISelectionContext), typeof(SelectionBehaviorBase), new PropertyMetadata(null, SelectionContextPropertyChanged));

	public static readonly DependencyProperty EnableSelectionClearProperty = DependencyProperty.Register("EnableSelectionClear", typeof(bool), typeof(SelectionBehaviorBase));

	public ISelectionContext SelectionContext
	{
		get
		{
			return (ISelectionContext)GetValue(SelectionContextProperty);
		}
		set
		{
			SetValue(SelectionContextProperty, value);
		}
	}

	public bool EnableSelectionClear
	{
		get
		{
			return (bool)GetValue(EnableSelectionClearProperty);
		}
		set
		{
			SetValue(EnableSelectionClearProperty, value);
		}
	}

	private static void SelectionContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((SelectionBehaviorBase)d).ChangedContext(e.OldValue as ISelectionContext);
	}

	private void ChangedContext(ISelectionContext oldSelectionContext)
	{
		if (oldSelectionContext != SelectionContext)
		{
			if (oldSelectionContext != null)
			{
				oldSelectionContext.SelectionChanging -= SelectionContext_SelectionChanging;
				oldSelectionContext.SelectionChanged -= SelectionContext_SelectionChanged;
			}
			if (SelectionContext != null)
			{
				SelectionContext.SelectionChanging += SelectionContext_SelectionChanging;
				SelectionContext.SelectionChanged += SelectionContext_SelectionChanged;
			}
			OnSelectionContextChanged();
		}
	}

	protected override void OnAttached()
	{
		base.OnAttached();
		base.AssociatedObject.SelectionChanged += SelectorSelectionChanged;
		base.AssociatedObject.PreviewMouseDown += SelectorMouseDown;
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		base.AssociatedObject.SelectionChanged -= SelectorSelectionChanged;
		base.AssociatedObject.PreviewMouseDown -= SelectorMouseDown;
	}

	protected virtual void OnSelectionContextChanged()
	{
	}

	protected virtual void OnSelectionContextSelectionChanging()
	{
	}

	protected virtual void OnSelectionContextSelectionChanged()
	{
	}

	protected virtual void OnAssociatedObjectSelectionChanged(IList addeditems, IList removedItems)
	{
	}

	private void SelectionContext_SelectionChanged(object sender, EventArgs e)
	{
		OnSelectionContextSelectionChanged();
	}

	private void SelectionContext_SelectionChanging(object sender, EventArgs e)
	{
		OnSelectionContextSelectionChanging();
	}

	private void SelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.OriginalSource == base.AssociatedObject)
		{
			OnAssociatedObjectSelectionChanged(e.AddedItems, e.RemovedItems);
		}
	}

	private void SelectorMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (!EnableSelectionClear)
		{
			return;
		}
		for (DependencyObject dependencyObject = e.OriginalSource as DependencyObject; dependencyObject != null; dependencyObject = dependencyObject.GetVisualOrLogicalParent())
		{
			if (dependencyObject is ListBox)
			{
				base.AssociatedObject.SelectedItem = null;
				break;
			}
			ListBoxItem listBoxItem = dependencyObject as ListBoxItem;
			if (listBoxItem != null)
			{
				break;
			}
		}
	}
}
