using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors;

public class ContextSensitiveToolBarBehavior : Behavior<ToolBarTray>
{
	public static readonly DependencyProperty ResourceKeyProperty = DependencyProperty.Register("ResourceKey", typeof(object), typeof(ContextSensitiveToolBarBehavior), new PropertyMetadata(null, delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ContextSensitiveToolBarBehavior)d).OnResourceKeyChanged(e);
	}));

	private ToolBar m_toolBar;

	public object ResourceKey
	{
		get
		{
			return GetValue(ResourceKeyProperty);
		}
		set
		{
			SetValue(ResourceKeyProperty, value);
		}
	}

	private void OnResourceKeyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (base.AssociatedObject != null)
		{
			if (m_toolBar != null)
			{
				base.AssociatedObject.ToolBars.Remove(m_toolBar);
				m_toolBar = null;
			}
			if (e.NewValue != null && base.AssociatedObject.TryFindResource(e.NewValue) is ToolBar toolBar)
			{
				base.AssociatedObject.ToolBars.Add(toolBar);
				m_toolBar = toolBar;
			}
		}
	}
}
