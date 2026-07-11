using System.Windows;
using System.Windows.Input;

namespace Firaxis.AssetBrowser;

public class AttachedBehaviors
{
	public static DependencyProperty MouseOverCommandProperty = DependencyProperty.RegisterAttached("MouseOverCommand", typeof(ICommand), typeof(AttachedBehaviors), new FrameworkPropertyMetadata(MouseOverChanged));

	public static DependencyProperty MouseOverCommandParameterProperty = DependencyProperty.RegisterAttached("MouseOverCommandParameter", typeof(object), typeof(AttachedBehaviors), new FrameworkPropertyMetadata(MouseOverCommandParameterChanged));

	public static void SetMouseOverCommand(DependencyObject target, ICommand value)
	{
		target.SetValue(MouseOverCommandProperty, value);
	}

	public static ICommand GetMouseOverCommand(DependencyObject target)
	{
		return (ICommand)target.GetValue(MouseOverCommandProperty);
	}

	private static void MouseOverChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
	{
		if (target is UIElement uIElement)
		{
			if (e.NewValue != null && e.OldValue == null)
			{
				uIElement.MouseEnter += element_MouseEnter;
			}
			else if (e.NewValue == null && e.OldValue != null)
			{
				uIElement.MouseEnter -= element_MouseEnter;
			}
		}
	}

	private static void element_MouseEnter(object sender, MouseEventArgs e)
	{
		UIElement uIElement = (UIElement)sender;
		ICommand command = (ICommand)uIElement.GetValue(MouseOverCommandProperty);
		command.Execute(uIElement.GetValue(MouseOverCommandParameterProperty));
	}

	public static void SetMouseOverCommandParameter(DependencyObject target, object value)
	{
		target.SetValue(MouseOverCommandParameterProperty, value);
	}

	public static object GetMouseOverCommandParameter(DependencyObject target)
	{
		return target.GetValue(MouseOverCommandParameterProperty);
	}

	private static void MouseOverCommandParameterChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
	{
		if (target is UIElement uIElement)
		{
			uIElement.SetValue(MouseOverCommandParameterProperty, e.NewValue);
		}
	}
}
