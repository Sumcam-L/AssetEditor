using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Firaxis.MVVMBase.Controls;

public class DragAdorner : Adorner
{
	public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(DragAdorner), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

	public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(DragAdorner), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

	public static readonly DependencyProperty AllowRenderProperty = DependencyProperty.Register("AllowRender", typeof(bool), typeof(DragAdorner), new FrameworkPropertyMetadata(true, AllowRenderChanged));

	public double X
	{
		get
		{
			return (double)GetValue(XProperty);
		}
		set
		{
			SetValue(XProperty, value);
		}
	}

	public double Y
	{
		get
		{
			return (double)GetValue(YProperty);
		}
		set
		{
			SetValue(YProperty, value);
		}
	}

	public bool AllowRender
	{
		get
		{
			return (bool)GetValue(AllowRenderProperty);
		}
		set
		{
			SetValue(AllowRenderProperty, value);
		}
	}

	private static void AllowRenderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is DragAdorner dragAdorner)
		{
			dragAdorner.X = 0.0;
			dragAdorner.Y = 0.0;
		}
	}

	public DragAdorner(AutoAdjustingStackPanelSplitter adornedElement)
		: base(adornedElement)
	{
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		if (AllowRender)
		{
			Rect rectangle = new Rect(new Point(X, Y), base.AdornedElement.RenderSize);
			SolidColorBrush brush = new SolidColorBrush(((AutoAdjustingStackPanelSplitter)base.AdornedElement).DragColor);
			drawingContext.DrawRectangle(brush, null, rectangle);
		}
	}
}
