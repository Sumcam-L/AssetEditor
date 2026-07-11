using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors;

public class SelectorSelectFirstBehavior : Behavior<Selector>
{
	protected override void OnAttached()
	{
		base.OnAttached();
		base.AssociatedObject.SelectionChanged += OnSelectionChanged;
		base.AssociatedObject.IsEnabledChanged += OnIsEnabledChanged;
	}

	protected override void OnDetaching()
	{
		base.AssociatedObject.SelectionChanged -= OnSelectionChanged;
		base.AssociatedObject.IsEnabledChanged -= OnIsEnabledChanged;
		base.OnDetaching();
	}

	private void OnSelectionChanged(object sender, EventArgs e)
	{
		if (base.AssociatedObject.Items.Count > 0 && base.AssociatedObject.SelectedItem == null)
		{
			base.AssociatedObject.SelectedIndex = 0;
		}
	}

	private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (base.AssociatedObject.Items.Count > 0 && base.AssociatedObject.SelectedItem == null)
		{
			base.AssociatedObject.SelectedIndex = 0;
		}
	}
}
