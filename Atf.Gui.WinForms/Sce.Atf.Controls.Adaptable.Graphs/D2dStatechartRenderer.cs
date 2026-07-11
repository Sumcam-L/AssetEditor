using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Direct2D;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class D2dStatechartRenderer<TNode, TEdge> : D2dGraphRenderer<TNode, TEdge, BoundaryRoute>, IDisposable where TNode : class, IState where TEdge : class, IGraphEdge<TNode, BoundaryRoute>
{
	private const int CornerRadius = 12;

	public static readonly Size PseudostateSize = new Size(24, 24);

	private const float MinRadiusInPixel = 3f;

	private const float MinFontHeightInPixel = 5f;

	private D2dDiagramTheme m_theme;

	private D2dTextFormat m_centerText;

	private float m_fontHeight;

	private const int StateMargin = 32;

	private const int ArrowSize = 8;

	private const int Margin = 4;

	private D2dRoundedRect m_stateRect = default(D2dRoundedRect);

	public D2dStatechartRenderer(D2dDiagramTheme theme)
	{
		m_theme = theme;
		UpdateToTheme();
		m_theme.Redraw += theme_Redraw;
		m_stateRect.RadiusX = 12f;
		m_stateRect.RadiusY = 12f;
	}

	protected override void Dispose(bool disposing)
	{
		m_theme.Redraw -= theme_Redraw;
		if (disposing)
		{
			m_centerText.Dispose();
		}
		base.Dispose(disposing);
	}

	public override void Draw(TNode node, DiagramDrawingStyle style, D2dGraphics g)
	{
		switch (style)
		{
		case DiagramDrawingStyle.Normal:
			Draw(node, g, outline: true);
			break;
		case DiagramDrawingStyle.Selected:
		case DiagramDrawingStyle.LastSelected:
		case DiagramDrawingStyle.Hot:
		case DiagramDrawingStyle.Error:
			Draw(node, g, outline: false);
			DrawOutline(node, m_theme.GetOutLineBrush(style), g);
			break;
		default:
			DrawGhost(node, g);
			break;
		}
	}

	public override void Draw(TEdge edge, DiagramDrawingStyle style, D2dGraphics g)
	{
		Draw(edge, m_theme.GetOutLineBrush(style), g);
	}

	public override void Draw(TNode fromNode, BoundaryRoute fromRoute, TNode toNode, BoundaryRoute toRoute, string label, Point endPoint, D2dGraphics g)
	{
		Matrix3x2F transform = g.Transform;
		transform.Invert();
		PointF pointF = Matrix3x2F.TransformPoint(transform, endPoint);
		PointF p;
		PointF normal;
		if (fromNode != null)
		{
			p = ParameterToPoint(fromNode.Bounds, fromRoute.Position, out normal);
		}
		else
		{
			p = pointF;
			normal = default(Point);
		}
		PointF p2;
		PointF normal2;
		if (toNode != null)
		{
			p2 = ParameterToPoint(toNode.Bounds, toRoute.Position, out normal2);
		}
		else
		{
			p2 = pointF;
			normal2 = default(Point);
		}
		PointF p3;
		PointF p4;
		float transitionPoints = GetTransitionPoints(p, normal, p2, normal2, out p3, out p4);
		DrawEdgeSpline(p, p3, p4, p2, transitionPoints, m_theme.OutlineBrush, g);
		if (!string.IsNullOrEmpty(label))
		{
			BezierCurve2F bezierCurve2F = new BezierCurve2F(p, p3, p4, p2);
			Vec2F vec2F = bezierCurve2F.Evaluate(0.5f);
			g.DrawText(label, m_centerText, new PointF(vec2F.X, vec2F.Y), m_theme.TextBrush);
		}
	}

	public override GraphHitRecord<TNode, TEdge, BoundaryRoute> Pick(IGraph<TNode, TEdge, BoundaryRoute> graph, TEdge priorityEdge, PointF p, D2dGraphics g)
	{
		int pickTolerance = m_theme.PickTolerance;
		TNode val = null;
		TEdge val2 = null;
		BoundaryRoute fromRoute = null;
		BoundaryRoute toRoute = null;
		Vec2F v = new Vec2F(p.X, p.Y);
		if (priorityEdge != null && Pick(priorityEdge, v))
		{
			val2 = priorityEdge;
		}
		else
		{
			foreach (TEdge item in graph.Edges.Reverse())
			{
				if (Pick(item, v))
				{
					val2 = item;
					break;
				}
			}
		}
		foreach (TNode item2 in graph.Nodes.Reverse())
		{
			RectangleF bounds = item2.Bounds;
			bounds.Inflate(pickTolerance, pickTolerance);
			if (!bounds.Contains(p))
			{
				continue;
			}
			val = item2;
			float position = PointToParameter(bounds, p);
			bounds.Inflate(-2 * pickTolerance, -2 * pickTolerance);
			bool flag = !bounds.Contains(p);
			if (val2 == null)
			{
				if (flag)
				{
					fromRoute = new BoundaryRoute(position);
					toRoute = new BoundaryRoute(position);
				}
			}
			else if (flag)
			{
				if (val2.FromNode == val)
				{
					fromRoute = new BoundaryRoute(position);
				}
				else if (val2.ToNode == val)
				{
					toRoute = new BoundaryRoute(position);
				}
			}
			break;
		}
		if (val is IComplexState<TNode, TEdge> && val2 == null)
		{
			RectangleF rectangleF = val.Bounds;
			RectangleF value = new RectangleF(rectangleF.X + 12f, rectangleF.Y + 4f, rectangleF.Width - 24f, m_theme.TextFormat.FontHeight);
			if (value.Contains(p))
			{
				DiagramLabel part = new DiagramLabel(Rectangle.Truncate(value), TextFormatFlags.SingleLine);
				return new GraphHitRecord<TNode, TEdge, BoundaryRoute>(val, part);
			}
		}
		return new GraphHitRecord<TNode, TEdge, BoundaryRoute>(val, val2, fromRoute, toRoute);
	}

	private void theme_Redraw(object sender, EventArgs e)
	{
		UpdateToTheme();
		OnRedraw();
	}

	private bool Pick(TEdge edge, Vec2F v)
	{
		int pickTolerance = m_theme.PickTolerance;
		GetTransitionPoints(edge, out var p, out var p2, out var p3, out var p4);
		BezierCurve2F curve = new BezierCurve2F(p, p2, p3, p4);
		Vec2F hitPoint = default(Vec2F);
		return BezierCurve2F.Pick(curve, v, m_theme.PickTolerance, ref hitPoint);
	}

	private void Draw(TNode state, D2dGraphics g, bool outline)
	{
		RectangleF rect = state.Bounds;
		if (state.Type != StateType.Normal)
		{
			DrawPseudostate(state, g, outline);
			return;
		}
		float m = g.Transform.M11;
		float num = m * 12f;
		IComplexState<TNode, TEdge> complexState = state as IComplexState<TNode, TEdge>;
		StateIndicators indicators = state.Indicators;
		if ((indicators & StateIndicators.Active) != StateIndicators.Breakpoint && num > 3f)
		{
			g.FillEllipse(new D2dEllipse
			{
				RadiusX = 12f,
				RadiusY = 12f,
				Center = rect.Location
			}, Color.SpringGreen);
		}
		if (num > 3f)
		{
			m_stateRect.Rect = rect;
			D2dLinearGradientBrush fillGradientBrush = m_theme.FillGradientBrush;
			fillGradientBrush.StartPoint = rect.Location;
			fillGradientBrush.EndPoint = new PointF(rect.Right, rect.Bottom);
			g.FillRoundedRectangle(m_stateRect, fillGradientBrush);
			if (outline)
			{
				g.DrawRoundedRectangle(m_stateRect, m_theme.OutlineBrush);
			}
		}
		else
		{
			g.FillRectangle(rect, m_theme.FillBrush);
			if (outline)
			{
				g.DrawRectangle(rect, m_theme.OutlineBrush);
			}
		}
		g.DrawLine(rect.Left, rect.Top + m_fontHeight + 4f, rect.Right, rect.Top + m_fontHeight + 4f, m_theme.OutlineBrush);
		if (m * m_fontHeight > 5f)
		{
			g.DrawText(complexState.Name, m_theme.TextFormat, new PointF(rect.X + 12f, rect.Y + 4f), m_theme.TextBrush);
		}
	}

	private void DrawGhost(TNode state, D2dGraphics g)
	{
		RectangleF rectangleF = state.Bounds;
		if (state.Type != StateType.Normal)
		{
			g.FillEllipse((D2dEllipse)rectangleF, m_theme.GhostBrush);
			return;
		}
		m_stateRect.Rect = rectangleF;
		g.FillRoundedRectangle(m_stateRect, m_theme.GhostBrush);
	}

	private void DrawPseudostate(TNode state, D2dGraphics g, bool outline)
	{
		RectangleF rectangleF = state.Bounds;
		D2dEllipse d2dEllipse = (D2dEllipse)rectangleF;
		D2dEllipse ellipse = d2dEllipse;
		ellipse.RadiusX = 4f;
		ellipse.RadiusY = 4f;
		g.FillEllipse(d2dEllipse, m_theme.FillBrush);
		switch (state.Type)
		{
		case StateType.Start:
			g.FillEllipse(ellipse, m_theme.TextBrush);
			break;
		case StateType.Final:
			g.DrawEllipse(d2dEllipse, m_theme.OutlineBrush, 3f);
			g.FillEllipse(ellipse, m_theme.TextBrush);
			break;
		case StateType.ShallowHistory:
			g.DrawText("H", m_centerText, rectangleF, m_theme.TextBrush);
			break;
		case StateType.DeepHistory:
			g.DrawText("H*", m_centerText, rectangleF, m_theme.TextBrush);
			break;
		case StateType.Conditional:
			g.DrawText("C", m_centerText, rectangleF, m_theme.TextBrush);
			break;
		}
		if (outline && state.Type != StateType.Final)
		{
			g.DrawEllipse(d2dEllipse, m_theme.OutlineBrush);
		}
	}

	private void DrawOutline(TNode state, D2dBrush brush, D2dGraphics g)
	{
		RectangleF rectangleF = state.Bounds;
		if (state.Type != StateType.Normal)
		{
			g.DrawEllipse((D2dEllipse)rectangleF, brush, m_theme.StrokeWidth);
			return;
		}
		float m = g.Transform.M11;
		float num = m * 12f;
		if (num > 3f)
		{
			m_stateRect.Rect = rectangleF;
			g.DrawRoundedRectangle(m_stateRect, brush, m_theme.StrokeWidth);
		}
		else
		{
			g.DrawRectangle(rectangleF, brush, m_theme.StrokeWidth);
		}
	}

	private void Draw(TEdge edge, D2dBrush brush, D2dGraphics g)
	{
		PointF p;
		PointF p2;
		PointF p3;
		PointF p4;
		float transitionPoints = GetTransitionPoints(edge, out p, out p2, out p3, out p4);
		DrawEdgeSpline(p, p2, p3, p4, transitionPoints, brush, g);
		GdiUtil.MakeRectangle(p, p2);
		BezierCurve2F bezierCurve2F = new BezierCurve2F(p, p2, p3, p4);
		Vec2F vec2F = bezierCurve2F.Evaluate(0.5f);
		vec2F.X += 2f;
		string label = edge.Label;
		if (!string.IsNullOrEmpty(label))
		{
			g.DrawText(edge.Label, m_theme.TextFormat, new PointF(vec2F.X, vec2F.Y), m_theme.TextBrush);
		}
	}

	private void DrawEdgeSpline(PointF p1, PointF p2, PointF p3, PointF p4, float d, D2dBrush brush, D2dGraphics g)
	{
		g.DrawBezier(p1, p2, p3, p4, brush, m_theme.StrokeWidth);
		float num = 8f / d;
		float dx = (p3.X - p4.X) * num;
		float dy = (p3.Y - p4.Y) * num;
		DrawArrow(g, brush, p4, dx, dy);
	}

	private float GetTransitionPoints(TEdge edge, out PointF p1, out PointF p2, out PointF p3, out PointF p4)
	{
		TNode fromNode = edge.FromNode;
		float position = edge.FromRoute.Position;
		p1 = ParameterToPoint(fromNode.Bounds, position, out var normal);
		TNode toNode = edge.ToNode;
		float position2 = edge.ToRoute.Position;
		p4 = ParameterToPoint(toNode.Bounds, position2, out var normal2);
		return GetTransitionPoints(p1, normal, p4, normal2, out p2, out p3);
	}

	private float GetTransitionPoints(PointF p1, PointF normal1, PointF p4, PointF normal2, out PointF p2, out PointF p3)
	{
		float num = p4.X - p1.X;
		float num2 = p4.Y - p1.Y;
		int val = (int)Math.Sqrt(num * num + num2 * num2) / 2;
		val = Math.Max(1, val);
		val = Math.Min(val, 64);
		p2 = new PointF(p1.X + normal1.X * (float)val, p1.Y + normal1.Y * (float)val);
		p3 = new PointF(p4.X + normal2.X * (float)val, p4.Y + normal2.Y * (float)val);
		return val;
	}

	private float PointToParameter(RectangleF bounds, PointF p)
	{
		float num = bounds.X + bounds.Width * 0.5f;
		float num2 = bounds.Y + bounds.Height * 0.5f;
		float num3 = p.X - num;
		float num4 = p.Y - num2;
		float num5 = bounds.Width * 0.5f;
		float num6 = bounds.Height * 0.5f;
		float num7;
		if (num6 * num3 + num5 * num4 > 0f)
		{
			if ((0f - num6) * num3 + num5 * num4 < 0f)
			{
				num7 = 0f;
			}
			else
			{
				num7 = 1f;
				float num8 = num3;
				num3 = 0f - num4;
				num4 = num8;
				num8 = num5;
				num5 = num6;
				num6 = num8;
			}
		}
		else if (num6 * num3 + (0f - num5) * num4 < 0f)
		{
			num7 = 2f;
			num3 = 0f - num3;
			num4 = 0f - num4;
		}
		else
		{
			num7 = 3f;
			float num8 = num4;
			num4 = 0f - num3;
			num3 = num8;
			num8 = num5;
			num5 = num6;
			num6 = num8;
		}
		float num9 = num5 * num4 / num3;
		return num7 + (num9 + num6) / (num6 * 2f);
	}

	private PointF ParameterToPoint(RectangleF bounds, float t, out PointF normal)
	{
		float x = bounds.X;
		float y = bounds.Y;
		float width = bounds.Width;
		float height = bounds.Height;
		float num = x + width;
		float num2 = y + height;
		float num3 = 0f;
		float num4 = 0f;
		float num5;
		float num6;
		if (t < 2f)
		{
			if (t < 1f)
			{
				num5 = num;
				num6 = y;
				num4 = height;
				normal = new Point(1, 0);
			}
			else
			{
				num6 = num2;
				num5 = num;
				num3 = 0f - width;
				t -= 1f;
				normal = new Point(0, 1);
			}
		}
		else if (t < 3f)
		{
			num5 = x;
			num6 = num2;
			num4 = 0f - height;
			t -= 2f;
			normal = new Point(-1, 0);
		}
		else
		{
			num6 = y;
			num5 = x;
			num3 = width;
			t -= 3f;
			normal = new Point(0, -1);
		}
		return new PointF(num5 + num3 * t, num6 + num4 * t);
	}

	private PointF Project(RectangleF bounds, PointF p)
	{
		float t = PointToParameter(bounds, p);
		PointF normal;
		return ParameterToPoint(bounds, t, out normal);
	}

	private float Distance(RectangleF bounds, Point p)
	{
		PointF pointF = Project(bounds, p);
		float num = (float)p.X - pointF.X;
		float num2 = (float)p.Y - pointF.Y;
		return (float)Math.Sqrt(num * num + num2 * num2);
	}

	private Size ExpandSize(Size oldSize, Size newSize)
	{
		return new Size(Math.Max(oldSize.Width, newSize.Width), Math.Max(oldSize.Height, newSize.Height));
	}

	private void DrawArrow(D2dGraphics g, D2dBrush brush, PointF p, float dx, float dy)
	{
		PointF pt = new PointF(p.X + (dx * 0.866f + dy * -0.5f), p.Y + (dx * 0.5f + dy * 0.866f));
		PointF pt2 = new PointF(p.X + (dx * 0.866f + dy * 0.5f), p.Y + (dx * -0.5f + dy * 0.866f));
		g.DrawLine(p, pt, brush, m_theme.StrokeWidth);
		g.DrawLine(p, pt2, brush, m_theme.StrokeWidth);
	}

	private void UpdateToTheme()
	{
		D2dTextFormat textFormat = m_theme.TextFormat;
		if (m_centerText == null || textFormat.FontFamilyName != m_centerText.FontFamilyName || textFormat.FontSize != m_centerText.FontSize)
		{
			if (m_centerText != null)
			{
				m_centerText.Dispose();
			}
			m_centerText = D2dFactory.CreateTextFormat(textFormat.FontFamilyName, D2dFontWeight.Bold, D2dFontStyle.Normal, m_theme.TextFormat.FontHeight);
			m_centerText.TextAlignment = D2dTextAlignment.Center;
			m_centerText.ParagraphAlignment = D2dParagraphAlignment.Center;
			m_fontHeight = m_theme.TextFormat.FontHeight;
		}
	}
}
