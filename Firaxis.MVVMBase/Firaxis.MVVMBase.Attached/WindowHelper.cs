using System.Windows;
using Firaxis.MVVMBase.Helpers;

namespace Firaxis.MVVMBase.Attached;

public class WindowHelper
{
	public static readonly DependencyProperty RequestedCloseProperty = DependencyProperty.RegisterAttached("RequestedClose", typeof(bool), typeof(WindowHelper), new PropertyMetadata(false, RequestedCloseChanged));

	public static readonly DependencyProperty DialogResultProperty = DependencyProperty.RegisterAttached("DialogResult", typeof(bool?), typeof(WindowHelper), new PropertyMetadata(null, DialogResultChanged));

	public static bool GetRequestedClose(Window target)
	{
		return (bool)target.GetValue(RequestedCloseProperty);
	}

	public static void SetRequestedClose(Window target, bool value)
	{
		target.SetValue(RequestedCloseProperty, value);
	}

	private static void RequestedCloseChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		Window window = sender as Window;
		if (window != null && GetRequestedClose(window))
		{
			window.Close();
			ApplicationHelper.InvokeIfNeeded(window.Dispatcher, delegate
			{
				SetRequestedClose(window, value: false);
			});
		}
	}

	public static bool? GetDialogResult(Window target)
	{
		return (bool?)target.GetValue(DialogResultProperty);
	}

	public static void SetDialogResult(Window target, bool? value)
	{
		target.SetValue(DialogResultProperty, value);
	}

	private static void DialogResultChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is Window window)
		{
			window.DialogResult = GetDialogResult(window);
		}
	}
}
