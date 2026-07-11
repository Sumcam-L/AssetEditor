using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Controls;

internal class ShadowChrome : Decorator
{
	private static SolidColorBrush backgroundBrush;

	private static LinearGradientBrush rightBrush;

	private static LinearGradientBrush bottomBrush;

	private static RadialGradientBrush bottomRightBrush;

	private static RadialGradientBrush topRightBrush;

	private static RadialGradientBrush bottomLeftBrush;

	static ShadowChrome()
	{
		FrameworkElement.MarginProperty.OverrideMetadata(typeof(ShadowChrome), new FrameworkPropertyMetadata(new Thickness(0.0, 0.0, 1.0, 1.0)));
		CreateBrushes();
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		double num = Math.Min(base.Margin.Right, base.Margin.Bottom);
		if (!(num <= 0.0) && !(base.ActualWidth < num * 2.0) && !(base.ActualHeight < num * 2.0))
		{
			drawingContext.DrawRectangle(rectangle: new Rect(num, num, base.ActualWidth - num, base.ActualHeight - num), brush: backgroundBrush, pen: null);
			drawingContext.DrawRectangle(rectangle: new Rect(base.ActualWidth, num, num, num), brush: topRightBrush, pen: null);
			drawingContext.DrawRectangle(rectangle: new Rect(base.ActualWidth, num * 2.0, num, base.ActualHeight - num * 2.0), brush: rightBrush, pen: null);
			drawingContext.DrawRectangle(rectangle: new Rect(base.ActualWidth, base.ActualHeight, num, num), brush: bottomRightBrush, pen: null);
			drawingContext.DrawRectangle(rectangle: new Rect(num * 2.0, base.ActualHeight, base.ActualWidth - num * 2.0, num), brush: bottomBrush, pen: null);
			drawingContext.DrawRectangle(rectangle: new Rect(num, base.ActualHeight, num, num), brush: bottomLeftBrush, pen: null);
		}
	}

	private static void CreateBrushes()
	{
		Color color = Color.FromArgb(128, 0, 0, 0);
		Color color2 = Color.FromArgb(16, 0, 0, 0);
		GradientStopCollection gradientStopCollection = new GradientStopCollection(2);
		gradientStopCollection.Add(new GradientStop(color, 0.5));
		gradientStopCollection.Add(new GradientStop(color2, 1.0));
		backgroundBrush = new SolidColorBrush(color);
		rightBrush = new LinearGradientBrush(gradientStopCollection, new Point(0.0, 0.0), new Point(1.0, 0.0));
		bottomBrush = new LinearGradientBrush(gradientStopCollection, new Point(0.0, 0.0), new Point(0.0, 1.0));
		bottomRightBrush = new RadialGradientBrush(gradientStopCollection);
		bottomRightBrush.GradientOrigin = new Point(0.0, 0.0);
		bottomRightBrush.Center = new Point(0.0, 0.0);
		bottomRightBrush.RadiusX = 1.0;
		bottomRightBrush.RadiusY = 1.0;
		topRightBrush = new RadialGradientBrush(gradientStopCollection);
		topRightBrush.GradientOrigin = new Point(0.0, 1.0);
		topRightBrush.Center = new Point(0.0, 1.0);
		topRightBrush.RadiusX = 1.0;
		topRightBrush.RadiusY = 1.0;
		bottomLeftBrush = new RadialGradientBrush(gradientStopCollection);
		bottomLeftBrush.GradientOrigin = new Point(1.0, 0.0);
		bottomLeftBrush.Center = new Point(1.0, 0.0);
		bottomLeftBrush.RadiusX = 1.0;
		bottomLeftBrush.RadiusY = 1.0;
	}
}
