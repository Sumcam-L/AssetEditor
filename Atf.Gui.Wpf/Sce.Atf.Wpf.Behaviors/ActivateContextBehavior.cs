using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors;

public class ActivateContextBehavior : Behavior<FrameworkElement>
{
	protected override void OnAttached()
	{
		base.AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseDown;
		base.AssociatedObject.GotFocus += AssociatedObject_GotFocus;
	}

	private void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
	{
		SetActiveContext();
	}

	private void AssociatedObject_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		SetActiveContext();
	}

	private void SetActiveContext()
	{
		object obj = base.AssociatedObject.SafeGetDataContext();
		if (obj != null)
		{
			IContextRegistry exportedValueOrDefault = Composer.Current.Container.GetExportedValueOrDefault<IContextRegistry>();
			if (exportedValueOrDefault != null)
			{
				exportedValueOrDefault.ActiveContext = obj;
			}
		}
	}
}
