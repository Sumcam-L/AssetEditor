using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Firaxis.MVVMBase.Attached;

public class ComboBoxHelper
{
	public static readonly DependencyProperty SelectionChangedActionProperty = DependencyProperty.RegisterAttached("SelectionChangedAction", typeof(Action<IList, IList>), typeof(ComboBoxHelper), new PropertyMetadata(null, SelectionChangedActionChanged));

	public static Action<IList, IList> GetSelectionChangedAction(ComboBox target)
	{
		return (Action<IList, IList>)target.GetValue(SelectionChangedActionProperty);
	}

	public static void SetSelectionChangedAction(ComboBox target, Action<IList, IList> value)
	{
		target.SetValue(SelectionChangedActionProperty, value);
	}

	private static void SelectionChangedActionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is ComboBox comboBox)
		{
			comboBox.SelectionChanged -= ComboBox_SelectionChanged;
			if (GetSelectionChangedAction(comboBox) != null)
			{
				comboBox.SelectionChanged += ComboBox_SelectionChanged;
			}
		}
	}

	private static void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is ComboBox target)
		{
			GetSelectionChangedAction(target)?.Invoke(e.AddedItems, e.RemovedItems);
		}
	}
}
