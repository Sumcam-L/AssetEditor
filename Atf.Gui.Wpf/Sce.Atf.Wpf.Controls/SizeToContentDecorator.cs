using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Controls;

public class SizeToContentDecorator : Border
{
	public static readonly DependencyProperty SizeToContentProperty = DependencyProperty.Register("SizeToContent", typeof(SizeToContent), typeof(SizeToContentDecorator), new FrameworkPropertyMetadata(SizeToContent.Manual, FrameworkPropertyMetadataOptions.AffectsMeasure));

	public static readonly RoutedEvent DesiredSizeChangedEvent = EventManager.RegisterRoutedEvent("DesiredSizeChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SizeToContentDecorator));

	public SizeToContent SizeToContent
	{
		get
		{
			return (SizeToContent)GetValue(SizeToContentProperty);
		}
		set
		{
			SetValue(SizeToContentProperty, value);
		}
	}

	public event RoutedEventHandler DesiredSizeChanged
	{
		add
		{
			AddHandler(DesiredSizeChangedEvent, value);
		}
		remove
		{
			RemoveHandler(DesiredSizeChangedEvent, value);
		}
	}

	protected override Size MeasureOverride(Size constraint)
	{
		Size constraint2 = constraint;
		switch (SizeToContent)
		{
		case SizeToContent.Height:
			constraint2 = new Size(constraint.Width, double.PositiveInfinity);
			break;
		case SizeToContent.Width:
			constraint2 = new Size(double.PositiveInfinity, constraint.Height);
			break;
		case SizeToContent.WidthAndHeight:
			constraint2 = new Size(double.PositiveInfinity, double.PositiveInfinity);
			break;
		}
		return base.MeasureOverride(constraint2);
	}

	protected override void OnChildDesiredSizeChanged(UIElement child)
	{
		base.OnChildDesiredSizeChanged(child);
		RaiseDesiredSizedChangedEvent();
	}

	protected void RaiseDesiredSizedChangedEvent()
	{
		RoutedEventArgs e = new RoutedEventArgs(DesiredSizeChangedEvent);
		RaiseEvent(e);
	}
}
