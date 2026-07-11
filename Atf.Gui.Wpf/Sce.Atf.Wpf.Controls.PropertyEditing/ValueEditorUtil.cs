using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public static class ValueEditorUtil
{
	public static readonly DependencyProperty HandlesCommitKeysProperty = DependencyProperty.RegisterAttached("HandlesCommitKeys", typeof(bool), typeof(ValueEditorUtil), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

	public static bool ExecuteCommand(ICommand command, IInputElement element, object parameter)
	{
		if (command is RoutedCommand routedCommand)
		{
			if (routedCommand.CanExecute(parameter, element))
			{
				routedCommand.Execute(parameter, element);
				return true;
			}
		}
		else if (command != null && command.CanExecute(parameter))
		{
			command.Execute(parameter);
			return true;
		}
		return false;
	}

	public static bool GetHandlesCommitKeys(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(HandlesCommitKeysProperty);
	}

	public static void SetHandlesCommitKeys(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(HandlesCommitKeysProperty, value);
	}

	public static void UpdateBinding(FrameworkElement element, DependencyProperty property, UpdateBindingType updateType)
	{
		BindingExpression bindingExpression = element.GetBindingExpression(property);
		if (bindingExpression != null)
		{
			if (updateType == UpdateBindingType.Source || updateType == UpdateBindingType.Both)
			{
				bindingExpression.UpdateSource();
			}
			if (updateType == UpdateBindingType.Target || updateType == UpdateBindingType.Both)
			{
				bindingExpression.UpdateTarget();
			}
		}
	}

	public static void UpdateBinding(FrameworkElement element, DependencyProperty property, bool updateSource)
	{
		UpdateBinding(element, property, (!updateSource) ? UpdateBindingType.Target : UpdateBindingType.Both);
	}
}
