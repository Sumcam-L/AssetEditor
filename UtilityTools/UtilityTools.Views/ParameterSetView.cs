using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

namespace UtilityTools.Views;

public partial class ParameterSetView : UserControl, IComponentConnector, IStyleConnector
{
	private bool m_dragStarted = false;

	public ParameterSetView()
	{
		InitializeComponent();
	}

	private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
	{
		UpdateDataBinding(sender);
		m_dragStarted = false;
	}

	private void Slider_DragStarted(object sender, DragStartedEventArgs e)
	{
		m_dragStarted = true;
	}

	private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		if (!m_dragStarted)
		{
			UpdateDataBinding(sender);
		}
	}

	private void UpdateDataBinding(object sender)
	{
		DependencyProperty dependencyProperty = null;
		Control control = sender as Control;
		if (control is Slider)
		{
			dependencyProperty = RangeBase.ValueProperty;
		}
		else if (control is TextBox)
		{
			dependencyProperty = TextBox.TextProperty;
		}
		if (dependencyProperty != null)
		{
			control.GetBindingExpression(dependencyProperty)?.UpdateSource();
		}
	}

	private void ValueControl_LostFocus(object sender, RoutedEventArgs e)
	{
		UpdateDataBinding(sender);
	}

	private void ValueTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			UpdateDataBinding(sender);
			e.Handled = true;
		}
		else
		{
			e.Handled = false;
		}
	}


}
