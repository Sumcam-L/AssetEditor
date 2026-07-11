using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Behaviors;

public class InsertionAdorner : Adorner
{
	private bool isSeparatorHorizontal;

	private AdornerLayer adornerLayer;

	private static Pen pen;

	private static PathGeometry triangle;

	public bool IsInFirstHalf { get; set; }

	static InsertionAdorner()
	{
		pen = new Pen
		{
			Brush = Brushes.Gray,
			Thickness = 2.0
		};
		pen.Freeze();
		LineSegment lineSegment = new LineSegment(new Point(0.0, -5.0), isStroked: false);
		lineSegment.Freeze();
		LineSegment lineSegment2 = new LineSegment(new Point(0.0, 5.0), isStroked: false);
		lineSegment2.Freeze();
		PathFigure pathFigure = new PathFigure
		{
			StartPoint = new Point(5.0, 0.0)
		};
		pathFigure.Segments.Add(lineSegment);
		pathFigure.Segments.Add(lineSegment2);
		pathFigure.Freeze();
		triangle = new PathGeometry();
		triangle.Figures.Add(pathFigure);
		triangle.Freeze();
	}

	public InsertionAdorner(bool isSeparatorHorizontal, bool isInFirstHalf, UIElement adornedElement, AdornerLayer adornerLayer)
		: base(adornedElement)
	{
		this.isSeparatorHorizontal = isSeparatorHorizontal;
		IsInFirstHalf = isInFirstHalf;
		this.adornerLayer = adornerLayer;
		base.IsHitTestVisible = false;
		this.adornerLayer.Add(this);
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		CalculateStartAndEndPoint(out var startPoint, out var endPoint);
		drawingContext.DrawLine(pen, startPoint, endPoint);
		if (isSeparatorHorizontal)
		{
			DrawTriangle(drawingContext, startPoint, 0.0);
			DrawTriangle(drawingContext, endPoint, 180.0);
		}
		else
		{
			DrawTriangle(drawingContext, startPoint, 90.0);
			DrawTriangle(drawingContext, endPoint, -90.0);
		}
	}

	private void DrawTriangle(DrawingContext drawingContext, Point origin, double angle)
	{
		drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
		drawingContext.PushTransform(new RotateTransform(angle));
		drawingContext.DrawGeometry(pen.Brush, null, triangle);
		drawingContext.Pop();
		drawingContext.Pop();
	}

	private void CalculateStartAndEndPoint(out Point startPoint, out Point endPoint)
	{
		startPoint = default(Point);
		endPoint = default(Point);
		double width = base.AdornedElement.RenderSize.Width;
		double height = base.AdornedElement.RenderSize.Height;
		if (isSeparatorHorizontal)
		{
			endPoint.X = width;
			if (!IsInFirstHalf)
			{
				startPoint.Y = height;
				endPoint.Y = height;
			}
		}
		else
		{
			endPoint.Y = height;
			if (!IsInFirstHalf)
			{
				startPoint.X = width;
				endPoint.X = width;
			}
		}
	}

	public void Detach()
	{
		adornerLayer.Remove(this);
	}
}
