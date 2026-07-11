using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Docking;

public class SideBarButton : Button
{
	public static DependencyProperty IsCheckedProperty;

	public static DependencyProperty GradientStartPointProperty;

	public static DependencyProperty GradientEndPointProperty;

	public static DependencyProperty DockProperty;

	public bool IsChecked
	{
		get
		{
			return (bool)GetValue(IsCheckedProperty);
		}
		set
		{
			SetValue(IsCheckedProperty, value);
		}
	}

	public Point GradientStartPoint
	{
		get
		{
			return (Point)GetValue(GradientStartPointProperty);
		}
		set
		{
			SetValue(GradientStartPointProperty, value);
		}
	}

	public Point GradientEndPoint
	{
		get
		{
			return (Point)GetValue(GradientEndPointProperty);
		}
		set
		{
			SetValue(GradientEndPointProperty, value);
		}
	}

	public Dock Dock
	{
		get
		{
			return (Dock)GetValue(DockProperty);
		}
		set
		{
			SetValue(DockProperty, value);
		}
	}

	static SideBarButton()
	{
		IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(SideBarButton));
		GradientStartPointProperty = DependencyProperty.Register("GradientStartPoint", typeof(Point), typeof(SideBarButton));
		GradientEndPointProperty = DependencyProperty.Register("GradientEndPoint", typeof(Point), typeof(SideBarButton));
		DockProperty = DependencyProperty.Register("Dock", typeof(Dock), typeof(SideBarButton));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(SideBarButton), new FrameworkPropertyMetadata(typeof(SideBarButton)));
	}

	public SideBarButton()
	{
	}

	public SideBarButton(Dock dock)
	{
		Dock = dock;
	}
}
