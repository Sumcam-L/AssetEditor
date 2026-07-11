using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Docking;

public partial class ResizablePopup : Popup, IComponentConnector, IStyleConnector
{
	public static DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(ResizablePopup));

	public static DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(ResizablePopup));

	public static DependencyProperty DockSideProperty = DependencyProperty.Register("DockSide", typeof(Dock), typeof(ResizablePopup));

	public static DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(UIElement), typeof(ResizablePopup));

	public Brush Background
	{
		get
		{
			return (Brush)GetValue(BackgroundProperty);
		}
		set
		{
			SetValue(BackgroundProperty, value);
		}
	}

	public Brush BorderBrush
	{
		get
		{
			return (Brush)GetValue(BorderBrushProperty);
		}
		set
		{
			SetValue(BorderBrushProperty, value);
		}
	}

	public bool Resizing { get; private set; }

	public Dock DockSide
	{
		get
		{
			return (Dock)GetValue(DockSideProperty);
		}
		set
		{
			SetValue(DockSideProperty, value);
			UpdateUi();
		}
	}

	public UIElement Content
	{
		get
		{
			return (UIElement)GetValue(ContentProperty);
		}
		set
		{
			SetValue(ContentProperty, value);
		}
	}

	private void UpdateUi()
	{
		TopResizeThumb.Visibility = ((DockSide != Dock.Bottom) ? Visibility.Collapsed : Visibility.Visible);
		BottomResizeThumb.Visibility = ((DockSide != Dock.Top) ? Visibility.Collapsed : Visibility.Visible);
		LeftResizeThumb.Visibility = ((DockSide != Dock.Right) ? Visibility.Collapsed : Visibility.Visible);
		RightResizeThumb.Visibility = ((DockSide != Dock.Left) ? Visibility.Collapsed : Visibility.Visible);
	}

	public ResizablePopup()
	{
		InitializeComponent();
		Resizing = false;
		UpdateUi();
	}

	private void ThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		switch (DockSide)
		{
		case Dock.Top:
			base.Height += e.VerticalChange;
			break;
		case Dock.Bottom:
			base.Height -= e.VerticalChange;
			break;
		case Dock.Left:
			base.Width += e.HorizontalChange;
			break;
		case Dock.Right:
			base.Width -= e.HorizontalChange;
			break;
		}
		base.Width = Math.Min(base.Width, base.MaxWidth);
		base.Height = Math.Min(base.Height, base.MaxHeight);
		Thumb thumb = sender as Thumb;
	}

	private void ThumbDragStarted(object sender, DragStartedEventArgs e)
	{
		Resizing = true;
	}

	private void ThumbDragCompleted(object sender, DragCompletedEventArgs e)
	{
		Resizing = false;
	}
}
