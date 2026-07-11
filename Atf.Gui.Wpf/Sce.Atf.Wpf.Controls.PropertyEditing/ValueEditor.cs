using System.Windows;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public abstract class ValueEditor : DependencyObject
{
	public virtual bool UsesCustomContext => false;

	public virtual object GetCustomContext(PropertyNode node)
	{
		return null;
	}

	public virtual bool CanEdit(PropertyNode node)
	{
		return true;
	}

	public virtual Style GetStyle(PropertyNode node, DependencyObject container)
	{
		return null;
	}

	public abstract DataTemplate GetTemplate(PropertyNode node, DependencyObject container);

	public virtual DataTemplate GetNonEditingTemplate(PropertyNode node, DependencyObject container)
	{
		return FindResource<DataTemplate>(PropertyGrid.ReadOnlyTemplateKey, container);
	}

	protected static T FindResource<T>(object key, DependencyObject container) where T : class
	{
		if (container is FrameworkElement frameworkElement)
		{
			return frameworkElement.FindResource(key) as T;
		}
		return Application.Current.FindResource(key) as T;
	}
}
