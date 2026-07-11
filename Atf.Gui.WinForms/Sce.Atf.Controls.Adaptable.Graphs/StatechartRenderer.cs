using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class StatechartRenderer<TNode, TEdge> : GraphRenderer<TNode, TEdge, BoundaryRoute>, IDisposable where TNode : class, IState where TEdge : class, IGraphEdge<TNode, BoundaryRoute>
{
	private const int CornerRadius = 12;

	public static readonly Size PseudostateSize;

	private DiagramTheme m_theme;

	private Pen m_dividerPen;

	private int m_fontHeight;

	private const int StateMargin = 32;

	private const int ArrowSize = 8;

	private const int Margin = 4;

	private const int Levels = 8;

	private static readonly StringFormat s_stateTextFormat;

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

	public StatechartRenderer(DiagramTheme theme)
	{
		Theme = theme;
		UpdateToTheme();
	}

	protected override void Dispose(bool disposing)
	{
		Theme = null;
		base.Dispose(disposing);
	}

	public override void Draw(TNode node, DiagramDrawingStyle style, Graphics g)
	{
		switch (style)
		{
		case DiagramDrawingStyle.Normal:
			Draw(node, g);
			break;
		case DiagramDrawingStyle.Selected:
		case DiagramDrawingStyle.LastSelected:
		case DiagramDrawingStyle.Hot:
		case DiagramDrawingStyle.Error:
			Draw(node, g);
			DrawOutline(node, m_theme.GetPen(style), g);
			break;
		default:
			DrawGhost(node, g);
			break;
		}
	}

	public override void Draw(TEdge edge, DiagramDrawingStyle style, Graphics g)
	{
		Draw(edge, m_theme.GetPen(style), g);
	}

	public override void Draw(TNode fromNode, BoundaryRoute fromRoute, TNode toNode, BoundaryRoute toRoute, string label, Point endPoint, Graphics g)
	{
		endPoint = GdiUtil.InverseTransform(g.Transform, endPoint);
		Point point;
		Point normal;
		if (fromNode != null)
		{
			point = ParameterToPoint(fromNode.Bounds, fromRoute.Position, out normal);
		}
		else
		{
			point = endPoint;
			normal = default(Point);
		}
		Point point2;
		Point normal2;
		if (toNode != null)
		{
			point2 = ParameterToPoint(toNode.Bounds, toRoute.Position, out normal2);
		}
		else
		{
			point2 = endPoint;
			normal2 = default(Point);
		}
		Point p;
		Point p2;
		float transitionPoints = GetTransitionPoints(point, normal, point2, normal2, out p, out p2);
		DrawEdgeSpline(point, p, p2, point2, transitionPoints, m_theme.OutlinePen, g);
		BezierCurve2F bezierCurve2F = new BezierCurve2F(point, p, p2, point2);
		Vec2F vec2F = bezierCurve2F.Evaluate(0.5f);
		vec2F.X += 2f;
		g.DrawString(label, m_theme.Font, m_theme.TextBrush, new PointF(vec2F.X, vec2F.Y));
	}

	public override Rectangle GetBounds(TNode node, Graphics g)
	{
		return GdiUtil.Transform(g.Transform, node.Bounds);
	}

	public override GraphHitRecord<TNode, TEdge, BoundaryRoute> Pick(IGraph<TNode, TEdge, BoundaryRoute> graph, TEdge priorityEdge, Point p, Graphics g)
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
			Rectangle bounds = item2.Bounds;
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
			Rectangle bounds2 = val.Bounds;
			Rectangle labelBounds = new Rectangle(bounds2.X + 12, bounds2.Y + 4, bounds2.Width - 24, m_theme.Font.Height);
			if (labelBounds.Contains(p))
			{
				DiagramLabel part = new DiagramLabel(labelBounds, TextFormatFlags.SingleLine);
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

	private void UpdateToTheme()
	{
		if (m_dividerPen != null)
		{
			m_dividerPen.Dispose();
		}
		m_dividerPen = new Pen(m_theme.OutlinePen.Color);
		m_fontHeight = m_theme.Font.Height;
	}

	private bool Pick(TEdge edge, Vec2F v)
	{
		int pickTolerance = m_theme.PickTolerance;
		GetTransitionPoints(edge, out var p, out var p2, out var p3, out var p4);
		BezierCurve2F curve = new BezierCurve2F(p, p2, p3, p4);
		Vec2F hitPoint = default(Vec2F);
		return BezierCurve2F.Pick(curve, v, m_theme.PickTolerance, ref hitPoint);
	}

	private void Draw(TNode state, Graphics g)
	{
		Rectangle bounds = state.Bounds;
		if (state.Type != StateType.Normal)
		{
			DrawPseudostate(bounds.Location, state.Type, m_theme.OutlinePen, g);
			return;
		}
		IComplexState<TNode, TEdge> complexState = state as IComplexState<TNode, TEdge>;
		if (bounds.Width <= 0)
		{
			bounds.Width = 1;
		}
		if (bounds.Height <= 0)
		{
			bounds.Height = 1;
		}
		StateIndicators indicators = state.Indicators;
		if ((indicators & StateIndicators.Active) != StateIndicators.Breakpoint)
		{
			g.FillEllipse(Brushes.SpringGreen, bounds.X - 12, bounds.Y - 12, 24, 24);
		}
		using (GraphicsPath path = GetStatePath(bounds))
		{
			if (!base.IsPrinting)
			{
				using LinearGradientBrush brush = new LinearGradientBrush(bounds, Color.WhiteSmoke, Color.LightGray, LinearGradientMode.ForwardDiagonal);
				g.FillPath(brush, path);
			}
			else
			{
				g.FillPath(Brushes.White, path);
			}
			g.DrawPath(m_theme.OutlinePen, path);
		}
		g.DrawString(complexState.Name, m_theme.Font, m_theme.TextBrush, bounds.X + 12, bounds.Y + 4);
		g.DrawLine(m_theme.OutlinePen, bounds.Left, bounds.Top + m_fontHeight + 4, bounds.Right, bounds.Top + m_fontHeight + 4);
		g.DrawString(layoutRectangle: new RectangleF(bounds.Left + 4, bounds.Top + m_fontHeight + 2, bounds.Width - 5, bounds.Height - m_fontHeight - 4), s: complexState.Text, font: m_theme.Font, brush: m_theme.TextBrush, format: s_stateTextFormat);
	}

	private void DrawGhost(TNode state, Graphics g)
	{
		Rectangle bounds = state.Bounds;
		if (state.Type != StateType.Normal)
		{
			DrawPseudostate(bounds.Location, state.Type, m_theme.GhostPen, g);
			return;
		}
		using GraphicsPath path = GetStatePath(bounds);
		g.FillPath(m_theme.GhostBrush, path);
		g.DrawPath(m_theme.GhostPen, path);
	}

	private void DrawPseudostate(Point p, StateType type, Pen pen, Graphics g)
	{
		Point point = new Point(p.X + 12, p.Y + 12);
		int num = 24;
		Brush brush = (base.IsPrinting ? Brushes.White : Brushes.WhiteSmoke);
		g.FillEllipse(brush, p.X, p.Y, num, num);
		switch (type)
		{
		case StateType.Start:
			g.DrawEllipse(m_theme.OutlinePen, p.X, p.Y, num, num);
			g.FillEllipse(m_theme.TextBrush, point.X - 4, point.Y - 4, 8, 8);
			break;
		case StateType.Final:
		{
			using (Pen pen2 = new Pen(pen.Color, 3f))
			{
				g.DrawEllipse(pen2, p.X, p.Y, num, num);
			}
			g.FillEllipse(m_theme.TextBrush, point.X - 4, point.Y - 4, 8, 8);
			break;
		}
		case StateType.ShallowHistory:
			g.DrawEllipse(m_theme.OutlinePen, p.X, p.Y, num, num);
			g.DrawString("H", m_theme.Font, m_theme.TextBrush, point.X - 7, point.Y - 8);
			break;
		case StateType.DeepHistory:
			g.DrawEllipse(m_theme.OutlinePen, p.X, p.Y, num, num);
			g.DrawString("H*", m_theme.Font, m_theme.TextBrush, point.X - 8, point.Y - 8);
			break;
		case StateType.Conditional:
			g.DrawEllipse(m_theme.OutlinePen, p.X, p.Y, num, num);
			g.DrawString("C", m_theme.Font, m_theme.TextBrush, point.X - 7, point.Y - 8);
			break;
		}
	}

	private void DrawOutline(TNode state, Pen pen, Graphics g)
	{
		int pickTolerance = m_theme.PickTolerance;
		Rectangle bounds = state.Bounds;
		if (state.Type != StateType.Normal)
		{
			g.DrawEllipse(pen, bounds);
			return;
		}
		using GraphicsPath path = GetStatePath(bounds);
		g.DrawPath(pen, path);
	}

	private GraphicsPath GetStatePath(Rectangle bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddArc(bounds.X, bounds.Y, 24, 24, 180f, 90f);
		graphicsPath.AddArc(bounds.X + bounds.Width - 24, bounds.Y, 24, 24, 270f, 90f);
		graphicsPath.AddArc(bounds.X + bounds.Width - 24, bounds.Y + bounds.Height - 24, 24, 24, 0f, 90f);
		graphicsPath.AddArc(bounds.X, bounds.Y + bounds.Height - 24, 24, 24, 90f, 90f);
		graphicsPath.AddLine(bounds.X, bounds.Y + bounds.Height - 24, bounds.X, bounds.Y + 12);
		return graphicsPath;
	}

	private void Draw(TEdge edge, Pen pen, Graphics g)
	{
		Point p;
		Point p2;
		Point p3;
		Point p4;
		float transitionPoints = GetTransitionPoints(edge, out p, out p2, out p3, out p4);
		DrawEdgeSpline(p, p2, p3, p4, transitionPoints, pen, g);
		BezierCurve2F bezierCurve2F = new BezierCurve2F(p, p2, p3, p4);
		Vec2F vec2F = bezierCurve2F.Evaluate(0.5f);
		vec2F.X += 2f;
		g.DrawString(edge.Label, m_theme.Font, m_theme.TextBrush, new PointF(vec2F.X, vec2F.Y));
	}

	private static void DrawEdgeSpline(Point p1, Point p2, Point p3, Point p4, float d, Pen pen, Graphics g)
	{
		try
		{
			g.DrawBezier(pen, p1, p2, p3, p4);
		}
		catch (OutOfMemoryException)
		{
		}
		float num = 8f / d;
		float dx = (float)(p3.X - p4.X) * num;
		float dy = (float)(p3.Y - p4.Y) * num;
		DrawArrow(g, pen, p4, dx, dy);
	}

	private float GetTransitionPoints(TEdge edge, out Point p1, out Point p2, out Point p3, out Point p4)
	{
		TNode fromNode = edge.FromNode;
		float position = edge.FromRoute.Position;
		p1 = ParameterToPoint(fromNode.Bounds, position, out var normal);
		TNode toNode = edge.ToNode;
		float position2 = edge.ToRoute.Position;
		p4 = ParameterToPoint(toNode.Bounds, position2, out var normal2);
		return GetTransitionPoints(p1, normal, p4, normal2, out p2, out p3);
	}

	private float GetTransitionPoints(Point p1, Point normal1, Point p4, Point normal2, out Point p2, out Point p3)
	{
		float num = p4.X - p1.X;
		float num2 = p4.Y - p1.Y;
		int val = (int)Math.Sqrt(num * num + num2 * num2) / 2;
		val = Math.Max(1, val);
		val = Math.Min(val, 64);
		p2 = new Point(p1.X + normal1.X * val, p1.Y + normal1.Y * val);
		p3 = new Point(p4.X + normal2.X * val, p4.Y + normal2.Y * val);
		return val;
	}

	private float PointToParameter(Rectangle bounds, Point p)
	{
		float num = (float)bounds.X + (float)bounds.Width * 0.5f;
		float num2 = (float)bounds.Y + (float)bounds.Height * 0.5f;
		float num3 = (float)p.X - num;
		float num4 = (float)p.Y - num2;
		float num5 = (float)bounds.Width * 0.5f;
		float num6 = (float)bounds.Height * 0.5f;
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

	private Point ParameterToPoint(Rectangle bounds, float t, out Point normal)
	{
		float num = bounds.X;
		float num2 = bounds.Y;
		float num3 = bounds.Width;
		float num4 = bounds.Height;
		float num5 = num + num3;
		float num6 = num2 + num4;
		float num7 = 0f;
		float num8 = 0f;
		float num9;
		float num10;
		if (t < 2f)
		{
			if (t < 1f)
			{
				num9 = num5;
				num10 = num2;
				num8 = num4;
				normal = new Point(1, 0);
			}
			else
			{
				num10 = num6;
				num9 = num5;
				num7 = 0f - num3;
				t -= 1f;
				normal = new Point(0, 1);
			}
		}
		else if (t < 3f)
		{
			num9 = num;
			num10 = num6;
			num8 = 0f - num4;
			t -= 2f;
			normal = new Point(-1, 0);
		}
		else
		{
			num10 = num2;
			num9 = num;
			num7 = num3;
			t -= 3f;
			normal = new Point(0, -1);
		}
		return new Point((int)(num9 + num7 * t), (int)(num10 + num8 * t));
	}

	private Point Project(Rectangle bounds, Point p)
	{
		float t = PointToParameter(bounds, p);
		Point normal;
		return ParameterToPoint(bounds, t, out normal);
	}

	private float Distance(Rectangle bounds, Point p)
	{
		Point point = Project(bounds, p);
		float num = p.X - point.X;
		float num2 = p.Y - point.Y;
		return (float)Math.Sqrt(num * num + num2 * num2);
	}

	private Size ExpandSize(Size oldSize, Size newSize)
	{
		return new Size(Math.Max(oldSize.Width, newSize.Width), Math.Max(oldSize.Height, newSize.Height));
	}

	private static void DrawArrow(Graphics g, Pen pen, Point p, float dx, float dy)
	{
		PointF pt = new PointF((float)((double)p.X + ((double)dx * 0.866 + (double)dy * -0.5)), (float)((double)p.Y + ((double)dx * 0.5 + (double)dy * 0.866)));
		PointF pt2 = new PointF((float)((double)p.X + ((double)dx * 0.866 + (double)dy * 0.5)), (float)((double)p.Y + ((double)dx * -0.5 + (double)dy * 0.866)));
		g.DrawLine(pen, p, pt);
		g.DrawLine(pen, p, pt2);
	}

	static StatechartRenderer()
	{
		PseudostateSize = new Size(24, 24);
		s_stateTextFormat = new StringFormat(StringFormatFlags.NoWrap);
		s_stateTextFormat.Alignment = StringAlignment.Near;
		s_stateTextFormat.LineAlignment = StringAlignment.Far;
	}
}
