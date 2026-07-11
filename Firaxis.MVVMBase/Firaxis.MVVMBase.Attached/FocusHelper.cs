using System.Windows;
using System.Windows.Input;
using Firaxis.MVVMBase.Helpers;

namespace Firaxis.MVVMBase.Attached;

public class FocusHelper
{
	public static readonly DependencyProperty FocusNavigationBindingsProperty = DependencyProperty.RegisterAttached("FocusNavigationBindings", typeof(FocusNavigationBindingCollection), typeof(FocusHelper), new PropertyMetadata(null, FocusNavigationBindingsChanged));

	public static readonly DependencyProperty FocusElementNameOnLoadProperty = DependencyProperty.RegisterAttached("FocusElementNameOnLoad", typeof(string), typeof(FocusHelper), new PropertyMetadata(null, FocusElementNameOnLoadChanged));

	public static FocusNavigationBindingCollection GetFocusNavigationBindings(FrameworkElement target)
	{
		return (FocusNavigationBindingCollection)target.GetValue(FocusNavigationBindingsProperty);
	}

	public static void SetFocusNavigationBindings(FrameworkElement target, FocusNavigationBindingCollection value)
	{
		target.SetValue(FocusNavigationBindingsProperty, value);
	}

	private static void FocusNavigationBindingsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (!(sender is FrameworkElement frameworkElement))
		{
			return;
		}
		frameworkElement.PreviewKeyDown -= FrameworkElement_PreviewKeyDown;
		FocusNavigationBindingCollection focusNavigationBindings = GetFocusNavigationBindings(frameworkElement);
		if (focusNavigationBindings == null || focusNavigationBindings.Count == 0)
		{
			if (e.OldValue is FocusNavigationBindingCollection focusNavigationBindingCollection)
			{
				focusNavigationBindingCollection.Owner = null;
			}
		}
		else
		{
			focusNavigationBindings.Owner = frameworkElement;
			frameworkElement.PreviewKeyDown += FrameworkElement_PreviewKeyDown;
		}
	}

	private static void FrameworkElement_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (!(sender is FrameworkElement frameworkElement))
		{
			return;
		}
		FocusNavigationBindingCollection focusNavigationBindings = GetFocusNavigationBindings(frameworkElement);
		if (focusNavigationBindings == null)
		{
			return;
		}
		foreach (FocusNavigationBinding item in focusNavigationBindings)
		{
			if (e.Key != item.Key || (item.CanNavigate != null && !item.CanNavigate()))
			{
				continue;
			}
			IInputElement inputElement = frameworkElement.FindName(item.ElementName) as IInputElement;
			if (inputElement != null)
			{
				e.Handled = true;
				ApplicationHelper.ExecuteOnPriority(delegate
				{
					inputElement.Focus();
					Keyboard.Focus(inputElement);
				});
			}
		}
	}

	public static string GetFocusElementNameOnLoad(FrameworkElement target)
	{
		return (string)target.GetValue(FocusElementNameOnLoadProperty);
	}

	public static void SetFocusElementNameOnLoad(FrameworkElement target, string value)
	{
		target.SetValue(FocusElementNameOnLoadProperty, value);
	}

	private static void FocusElementNameOnLoadChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is FrameworkElement frameworkElement)
		{
			if (GetFocusElementNameOnLoad(frameworkElement) != null)
			{
				frameworkElement.Loaded -= FrameworkElement_Loaded;
				frameworkElement.Loaded += FrameworkElement_Loaded;
			}
			else
			{
				frameworkElement.Loaded -= FrameworkElement_Loaded;
			}
		}
	}

	private static void FrameworkElement_Loaded(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement frameworkElement)
		{
			string focusElementNameOnLoad = GetFocusElementNameOnLoad(frameworkElement);
			if (frameworkElement.FindName(focusElementNameOnLoad) is IInputElement inputElement)
			{
				inputElement.Focus();
			}
		}
	}
}
