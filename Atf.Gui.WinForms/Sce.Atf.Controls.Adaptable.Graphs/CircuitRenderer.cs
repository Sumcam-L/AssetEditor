using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class CircuitRenderer<TElement, TWire, TPin> : GraphRenderer<TElement, TWire, TPin>, IDisposable where TElement : class, ICircuitElement where TWire : class, IGraphEdge<TElement, TPin> where TPin : class, ICircuitPin
{
	private class ElementTypeInfo
	{
		public Size Size;

		public Rectangle Interior;

		public int[] OutputLeftX;

		public GraphicsPath Path;
	}

	public class ElementSizeInfo
	{
		private readonly Size m_size;

		private readonly Rectangle m_interior;

		private readonly IEnumerable<int> m_outputLeftX;

		public Size Size => m_size;

		public Rectangle Interior => m_interior;

		public IEnumerable<int> OutputLeftX => m_outputLeftX;

		public ElementSizeInfo(Size size, Rectangle interior, IEnumerable<int> outputLeftX)
		{
			m_size = size;
			m_interior = interior;
			m_outputLeftX = outputLeftX;
		}
	}

	private DiagramTheme m_theme;

	private int m_rowSpacing;

	private int m_pinSize = 8;

	private int m_pinOffset;

	private readonly Dictionary<ICircuitElementType, ElementTypeInfo> m_elementTypeCache = new Dictionary<ICircuitElementType, ElementTypeInfo>();

	private const int PinMargin = 2;

	private const int MinElementWidth = 4;

	private const int MinElementHeight = 4;

	private const int HighlightingWidth = 3;

	private const int MaxNameOverhang = 64;

	private static readonly Matrix s_pathTransform = new Matrix();

	public DiagramTheme Theme
	{
		get
		{
			return m_theme;
		}
		set
		{
			if (m_theme != value)
			{
				if (m_theme != null)
				{
					m_theme.Redraw -= theme_Redraw;
				}
				m_theme = value;
				if (m_theme != null)
				{
					m_theme.Redraw += theme_Redraw;
				}
				base.OnRedraw();
			}
		}
	}

	public CircuitRenderer(DiagramTheme theme)
	{
		Theme = theme;
		SetPinSpacing();
	}

	protected override void Dispose(bool disposing)
	{
		DisposeElementInfo();
		Theme = null;
		base.Dispose(disposing);
	}

	public void Invalidate(ICircuitElementType elementType)
	{
		m_elementTypeCache.Remove(elementType);
		OnRedraw();
	}

	public override void Draw(TElement element, DiagramDrawingStyle style, Graphics g)
	{
		switch (style)
		{
		case DiagramDrawingStyle.Normal:
			Draw(element, g);
			break;
		default:
			Draw(element, g);
			DrawOutline(element, m_theme.GetPen(style), g);
			break;
		case DiagramDrawingStyle.Ghosted:
		case DiagramDrawingStyle.Hidden:
			DrawGhost(element, g);
			break;
		}
	}

	public override void Draw(TWire edge, DiagramDrawingStyle style, Graphics g)
	{
		TPin toRoute = edge.ToRoute;
		TPin fromRoute = edge.FromRoute;
		Pen pen = ((style == DiagramDrawingStyle.Normal) ? GetPen(toRoute) : m_theme.GetPen(style));
		DrawWire(edge.FromNode, fromRoute, edge.ToNode, toRoute, g, pen);
	}

	public override void Draw(TElement outputElement, TPin outputPin, TElement inputElement, TPin inputPin, string label, Point endPoint, Graphics g)
	{
		if (inputElement == null)
		{
			DrawWire(outputElement, outputPin, endPoint, fromOutput: true, g);
			return;
		}
		if (outputElement == null)
		{
			DrawWire(inputElement, inputPin, endPoint, fromOutput: false, g);
			return;
		}
		Pen pen = GetPen(outputPin);
		DrawWire(outputElement, outputPin, inputElement, inputPin, g, pen);
	}

	public override Rectangle GetBounds(TElement element, Graphics g)
	{
		Rectangle elementBounds = GetElementBounds(element, g);
		elementBounds.Height += 2 + m_rowSpacing;
		return GdiUtil.Transform(g.Transform, elementBounds);
	}

	public override GraphHitRecord<TElement, TWire, TPin> Pick(IGraph<TElement, TWire, TPin> graph, TWire priorityEdge, Point p, Graphics g)
	{
		TElement val = null;
		TPin toRoute = null;
		TPin fromRoute = null;
		TWire val2 = null;
		if (priorityEdge != null && PickEdge(priorityEdge, p, g))
		{
			val2 = priorityEdge;
		}
		else
		{
			foreach (TWire edge in graph.Edges)
			{
				if (PickEdge(edge, p, g))
				{
					val2 = edge;
					break;
				}
			}
		}
		foreach (TElement item in graph.Nodes.Reverse())
		{
			if (Pick(item, g, p))
			{
				val = item;
				toRoute = PickInput(item, g, p);
				fromRoute = PickOutput(item, g, p);
				break;
			}
		}
		if (val != null && val2 == null)
		{
			Rectangle elementBounds = GetElementBounds(val, g);
			Rectangle labelBounds = GdiUtil.Transform(r: new Rectangle(elementBounds.Left, elementBounds.Bottom + 2, elementBounds.Width, m_rowSpacing), matrix: g.Transform);
			if (labelBounds.Contains(p))
			{
				DiagramLabel part = new DiagramLabel(labelBounds, TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine);
				return new GraphHitRecord<TElement, TWire, TPin>(val, part);
			}
		}
		return new GraphHitRecord<TElement, TWire, TPin>(val, val2, fromRoute, toRoute);
	}

	private bool PickEdge(TWire edge, Point p, Graphics g)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(edge.FromNode, g);
		TPin toRoute = edge.ToRoute;
		TPin fromRoute = edge.FromRoute;
		Point location = edge.FromNode.Bounds.Location;
		int num = location.X + elementTypeInfo.Size.Width;
		int num2 = location.Y + GetPinOffset(fromRoute.Index);
		Point location2 = edge.ToNode.Bounds.Location;
		int x = location2.X;
		int num3 = location2.Y + GetPinOffset(toRoute.Index);
		int tangentLength = GetTangentLength(num, x);
		BezierCurve2F curve = new BezierCurve2F(new Vec2F(num, num2), new Vec2F(num + tangentLength, num2), new Vec2F(x - tangentLength, num3), new Vec2F(x, num3));
		Vec2F hitPoint = default(Vec2F);
		return BezierCurve2F.Pick(curve, new Vec2F(p.X, p.Y), m_theme.PickTolerance, ref hitPoint);
	}

	protected override void OnRedraw()
	{
		DisposeElementInfo();
		m_elementTypeCache.Clear();
		base.OnRedraw();
	}

	private void theme_Redraw(object sender, EventArgs e)
	{
		OnRedraw();
	}

	private void Draw(TElement element, Graphics g)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(element, g);
		Point location = element.Bounds.Location;
		Rectangle rectangle = new Rectangle(location, elementTypeInfo.Size);
		if (!g.ClipBounds.IntersectsWith(rectangle))
		{
			return;
		}
		ICircuitElementType type = element.Type;
		if (elementTypeInfo.Path == null)
		{
			BuildGraphics(type, elementTypeInfo, g);
		}
		s_pathTransform.Translate(location.X, location.Y);
		elementTypeInfo.Path.Transform(s_pathTransform);
		Brush customBrush = m_theme.GetCustomBrush(type.Name);
		if (customBrush != null)
		{
			g.FillPath(customBrush, elementTypeInfo.Path);
		}
		else
		{
			using LinearGradientBrush brush = new LinearGradientBrush(rectangle, Color.White, Color.LightSteelBlue, LinearGradientMode.Vertical);
			g.FillPath(brush, elementTypeInfo.Path);
		}
		g.DrawPath(m_theme.OutlinePen, elementTypeInfo.Path);
		int num = m_rowSpacing + 2;
		g.DrawLine(m_theme.OutlinePen, location.X, location.Y + num, location.X + elementTypeInfo.Size.Width, location.Y + num);
		g.DrawString(type.Name, m_theme.Font, m_theme.TextBrush, location.X + 2 + 1, location.Y + 2 + 1);
		int num2 = location.Y + num + 2;
		foreach (TPin input in type.Inputs)
		{
			Pen pen = GetPen(input);
			if (pen != null)
			{
				g.DrawRectangle(pen, location.X + 1, num2 + m_pinOffset, m_pinSize, m_pinSize);
			}
			g.DrawString(input.Name, m_theme.Font, m_theme.TextBrush, location.X + m_pinSize + 2, num2);
			num2 += m_rowSpacing;
		}
		num2 = location.Y + num + 2;
		int num3 = 0;
		foreach (TPin output in type.Outputs)
		{
			Pen pen2 = GetPen(output);
			if (pen2 != null)
			{
				g.DrawRectangle(pen2, location.X + elementTypeInfo.Size.Width - m_pinSize, num2 + m_pinOffset, m_pinSize, m_pinSize);
			}
			g.DrawString(output.Name, m_theme.Font, m_theme.TextBrush, location.X + elementTypeInfo.OutputLeftX[num3], num2);
			num2 += m_rowSpacing;
			num3++;
		}
		Image image = type.Image;
		if (image != null)
		{
			g.DrawImage(image, location.X + elementTypeInfo.Interior.X, location.Y + elementTypeInfo.Interior.Y, elementTypeInfo.Interior.Width, elementTypeInfo.Interior.Height);
		}
		s_pathTransform.Translate(-2 * location.X, -2 * location.Y);
		elementTypeInfo.Path.Transform(s_pathTransform);
		s_pathTransform.Reset();
		string name = element.Name;
		if (!string.IsNullOrEmpty(name))
		{
			g.DrawString(layoutRectangle: new RectangleF(rectangle.Left - 64, rectangle.Bottom + 2, rectangle.Width + 128, m_rowSpacing), s: name, font: m_theme.Font, brush: m_theme.TextBrush, format: m_theme.CenterStringFormat);
		}
	}

	private void DrawGhost(TElement element, Graphics g)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(element, g);
		Point location = element.Bounds.Location;
		Rectangle rectangle = new Rectangle(location, elementTypeInfo.Size);
		if (g.ClipBounds.IntersectsWith(rectangle))
		{
			if (elementTypeInfo.Path == null)
			{
				ICircuitElementType type = element.Type;
				BuildGraphics(type, elementTypeInfo, g);
			}
			s_pathTransform.Translate(location.X, location.Y);
			elementTypeInfo.Path.Transform(s_pathTransform);
			g.FillPath(m_theme.GhostBrush, elementTypeInfo.Path);
			g.DrawPath(m_theme.GhostPen, elementTypeInfo.Path);
			s_pathTransform.Translate(-2 * location.X, -2 * location.Y);
			elementTypeInfo.Path.Transform(s_pathTransform);
			s_pathTransform.Reset();
		}
	}

	private void DrawOutline(TElement element, Pen pen, Graphics g)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(element, g);
		if (elementTypeInfo.Path == null)
		{
			ICircuitElementType type = element.Type;
			BuildGraphics(type, elementTypeInfo, g);
		}
		Point location = element.Bounds.Location;
		s_pathTransform.Translate(location.X, location.Y);
		elementTypeInfo.Path.Transform(s_pathTransform);
		g.DrawPath(pen, elementTypeInfo.Path);
		s_pathTransform.Translate(2 * -location.X, 2 * -location.Y);
		elementTypeInfo.Path.Transform(s_pathTransform);
		s_pathTransform.Reset();
	}

	private bool Pick(TElement element, Graphics g, Point p)
	{
		Rectangle bounds = GetBounds(element, g);
		int pickTolerance = m_theme.PickTolerance;
		bounds.Inflate(pickTolerance, pickTolerance);
		return bounds.Contains(p.X, p.Y);
	}

	private TPin PickInput(TElement element, Graphics g, Point p)
	{
		Point location = element.Bounds.Location;
		int x = location.X;
		int y = location.Y + m_rowSpacing + 4 + m_pinOffset;
		int pinSize = m_pinSize;
		Rectangle rectangle = new Rectangle(x, y, pinSize, m_pinSize);
		int pickTolerance = m_theme.PickTolerance;
		rectangle.Inflate(pickTolerance, pickTolerance);
		ICircuitElementType type = element.Type;
		foreach (TPin input in type.Inputs)
		{
			if (rectangle.Contains(p.X, p.Y))
			{
				return input;
			}
			rectangle.Y += m_rowSpacing;
		}
		return null;
	}

	private TPin PickOutput(TElement element, Graphics g, Point p)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(element, g);
		Point location = element.Bounds.Location;
		int x = location.X + elementTypeInfo.Size.Width - m_pinSize;
		int y = location.Y + m_rowSpacing + 4 + m_pinOffset;
		int pinSize = m_pinSize;
		Rectangle rectangle = new Rectangle(x, y, pinSize, m_pinSize);
		int pickTolerance = m_theme.PickTolerance;
		rectangle.Inflate(pickTolerance, pickTolerance);
		ICircuitElementType type = element.Type;
		foreach (TPin output in type.Outputs)
		{
			if (rectangle.Contains(p.X, p.Y))
			{
				return output;
			}
			rectangle.Y += m_rowSpacing;
		}
		return null;
	}

	private ElementTypeInfo GetElementTypeInfo(TElement element, Graphics g)
	{
		ICircuitElementType type = element.Type;
		if (m_elementTypeCache.TryGetValue(type, out var value))
		{
			return value;
		}
		ElementSizeInfo elementSizeInfo = GetElementSizeInfo(type, g);
		ElementTypeInfo elementTypeInfo = new ElementTypeInfo
		{
			Size = elementSizeInfo.Size,
			Interior = elementSizeInfo.Interior,
			OutputLeftX = elementSizeInfo.OutputLeftX.ToArray()
		};
		m_elementTypeCache.Add(type, elementTypeInfo);
		return elementTypeInfo;
	}

	protected virtual ElementSizeInfo GetElementSizeInfo(ICircuitElementType type, Graphics g)
	{
		int val = (int)g.MeasureString(type.Name, m_theme.Font).Width + 4;
		IList<ICircuitPin> inputs = type.Inputs;
		IList<ICircuitPin> outputs = type.Outputs;
		int count = inputs.Count;
		int count2 = outputs.Count;
		int num = Math.Min(count, count2);
		int num2 = Math.Max(count, count2);
		int[] array = new int[count2];
		int num3 = m_rowSpacing + 4;
		num3 += Math.Max(num2 * m_rowSpacing, num * m_rowSpacing + type.InteriorSize.Height - 2);
		bool flag = true;
		for (int i = 0; i < num2; i++)
		{
			double num4 = 4.0;
			if (count > i)
			{
				num4 += (double)(g.MeasureString(inputs[i].Name, m_theme.Font).Width + (float)m_pinSize + 2f);
			}
			else
			{
				num4 += (double)type.InteriorSize.Width;
				flag = false;
			}
			if (count2 > i)
			{
				SizeF sizeF = g.MeasureString(outputs[i].Name, m_theme.Font);
				array[i] = (int)sizeF.Width;
				num4 += (double)(sizeF.Width + (float)m_pinSize + 2f);
			}
			else
			{
				num4 += (double)type.InteriorSize.Width;
			}
			val = Math.Max(val, (int)num4);
		}
		if (count == count2)
		{
			val = Math.Max(val, type.InteriorSize.Width + 2);
		}
		val = Math.Max(val, 4);
		num3 = Math.Max(num3, 4);
		Size size = new Size(val, num3);
		Rectangle interior = new Rectangle((!flag) ? 1 : (val - type.InteriorSize.Width), num3 - type.InteriorSize.Height, type.InteriorSize.Width, type.InteriorSize.Height);
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = val - 2 - m_pinSize - array[j];
		}
		return new ElementSizeInfo(size, interior, array);
	}

	private void DisposeElementInfo()
	{
		foreach (ElementTypeInfo value in m_elementTypeCache.Values)
		{
			if (value.Path != null)
			{
				value.Path.Dispose();
			}
		}
	}

	private void BuildGraphics(ICircuitElementType elementType, ElementTypeInfo info, Graphics g)
	{
		int width = info.Size.Width;
		int height = info.Size.Height;
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddLine(6f, 0f, (float)width - 12f, 0f);
		graphicsPath.AddArc((float)width - 12f, 0f, 12f, 12f, 270f, 90f);
		graphicsPath.AddLine(width, 6f, width, (float)height - 12f);
		graphicsPath.AddArc((float)width - 12f, (float)height - 12f, 12f, 12f, 0f, 90f);
		graphicsPath.AddLine((float)width - 12f, height, 6f, height);
		graphicsPath.AddArc(0f, (float)height - 12f, 12f, 12f, 90f, 90f);
		graphicsPath.AddLine(0f, (float)height - 12f, 0f, 6f);
		graphicsPath.AddArc(0f, 0f, 12f, 12f, 180f, 90f);
		info.Path = graphicsPath;
	}

	private void DrawWire(TElement outputElement, TPin outputPin, TElement inputElement, TPin inputPin, Graphics g, Pen pen)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(outputElement, g);
		Point location = outputElement.Bounds.Location;
		int x = location.X + elementTypeInfo.Size.Width;
		int y = location.Y + GetPinOffset(outputPin.Index);
		Point location2 = inputElement.Bounds.Location;
		int x2 = location2.X;
		int y2 = location2.Y + GetPinOffset(inputPin.Index);
		DrawWire(g, pen, x, y, x2, y2);
	}

	private void DrawWire(TElement element, TPin pin, Point p, bool fromOutput, Graphics g)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(element, g);
		Point location = element.Bounds.Location;
		int num = location.X;
		int num2 = location.Y + GetPinOffset(pin.Index);
		if (fromOutput)
		{
			num += elementTypeInfo.Size.Width;
		}
		Matrix transform = g.Transform;
		transform.Invert();
		Point point = GdiUtil.Transform(transform, p);
		Pen pen = GetPen(pin);
		if (fromOutput)
		{
			DrawWire(g, pen, num, num2, point.X, point.Y);
		}
		else
		{
			DrawWire(g, pen, point.X, point.Y, num, num2);
		}
	}

	private void DrawWire(Graphics g, Pen pen, int x1, int y1, int x2, int y2)
	{
		int tangentLength = GetTangentLength(x1, x2);
		try
		{
			g.DrawBezier(pen, new Point(x1, y1), new Point(x1 + tangentLength, y1), new Point(x2 - tangentLength, y2), new Point(x2, y2));
		}
		catch (OutOfMemoryException ex)
		{
			Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
		}
	}

	private int GetTangentLength(int x1, int x2)
	{
		int val = Math.Abs(x1 - x2) / 2;
		return Math.Max(val, 32);
	}

	private Pen GetPen(TPin pin)
	{
		Pen pen = m_theme.GetCustomPen(pin.TypeName);
		if (pen == null)
		{
			pen = m_theme.GhostPen;
		}
		return pen;
	}

	private int GetPinOffset(int index)
	{
		return m_rowSpacing + 4 + index * m_rowSpacing + m_pinOffset + m_pinSize / 2;
	}

	private void SetPinSpacing()
	{
		int height = m_theme.Font.Height;
		m_rowSpacing = height + 2;
		m_pinOffset = (height - m_pinSize) / 2;
	}

	private Rectangle GetElementBounds(TElement element, Graphics g)
	{
		ElementTypeInfo elementTypeInfo = GetElementTypeInfo(element, g);
		return new Rectangle(element.Bounds.Location, elementTypeInfo.Size);
	}
}
