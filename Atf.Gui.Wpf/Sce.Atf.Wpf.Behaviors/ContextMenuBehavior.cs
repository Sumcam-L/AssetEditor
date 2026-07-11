using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Interop;

namespace Sce.Atf.Wpf.Behaviors;

public class ContextMenuBehavior : Behavior<FrameworkElement>
{
	private static IContextMenuService s_cachedMenuService;
	private static bool s_menuServiceCached;

	public static readonly DependencyProperty ContextProperty = DependencyProperty.Register("Context", typeof(object), typeof(ContextMenuBehavior), new PropertyMetadata((object)null));

	public object Context
	{
		get
		{
			return GetValue(ContextProperty);
		}
		set
		{
			SetValue(ContextProperty, value);
		}
	}

	protected override void OnAttached()
	{
		base.OnAttached();
		base.AssociatedObject.MouseRightButtonUp += Element_MouseRightButtonUp;
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		base.AssociatedObject.MouseRightButtonUp -= Element_MouseRightButtonUp;
	}

	protected virtual object GetCommandContext(object sender, MouseButtonEventArgs e)
	{
		FrameworkElement frameworkElement = (FrameworkElement)sender;
		object obj = Context;
		if (obj == null)
		{
			obj = frameworkElement.DataContext;
		}
		return obj;
	}

	protected virtual object GetCommandTarget(object sender, MouseButtonEventArgs e)
	{
		object result = null;
		if (e.OriginalSource is DependencyObject dep)
		{
			FrameworkElement frameworkElement = dep.FindAncestor<FrameworkElement>();
			if (frameworkElement != null)
			{
				result = frameworkElement.DataContext;
			}
		}
		return result;
	}

	private void Element_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
	{
		object commandContext = GetCommandContext(sender, e);
		object commandTarget = GetCommandTarget(sender, e);
		if (commandContext != null && commandTarget != null)
		{
			if (!s_menuServiceCached)
			{
				s_cachedMenuService = Composer.Current.Container.GetExportedValueOrDefault<IContextMenuService>();
				s_menuServiceCached = true;
			}
			if (s_cachedMenuService != null)
			{
				IEnumerable<IContextMenuCommandProvider> exportedValues = Composer.Current.Container.GetExportedValues<IContextMenuCommandProvider>();
				IEnumerable<object> commands = exportedValues.GetCommands(commandContext, commandTarget);
				s_cachedMenuService.RunContextMenu(commands, (FrameworkElement)sender, e.GetPosition((FrameworkElement)sender));
				e.Handled = true;
			}
		}
	}
}
