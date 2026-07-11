using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class DigraphRenderer<TNode, TEdge> : GraphRenderer<TNode, TEdge, NumberedRoute>, IDisposable where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, NumberedRoute>
{
	private const int MinimumRadius = 32;

	private DiagramTheme m_theme;

	private int m_routeOffset = 24;

	private int m_arrowSize = 8;

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

	public int RouteOffset
	{
		get
		{
			return m_routeOffset;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentException("Route offset must be greater than 0");
			}
			if (m_routeOffset != value)
			{
				m_routeOffset = value;
				OnRedraw();
			}
		}
	}

	public int ArrowSize
	{
		get
		{
			return m_arrowSize;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentException("Arrow size must be greater than 0");
			}
			if (m_arrowSize != value)
			{
				m_arrowSize = value;
				OnRedraw();
			}
		}
	}

	public DigraphRenderer(DiagramTheme theme)
	{
		Theme = theme;
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

	public override void Draw(TNode fromNode, NumberedRoute fromRoute, TNode toNode, NumberedRoute toRoute, string label, Point endPoint, Graphics g)
	{
		Matrix transform = g.Transform;
		transform.Invert();
		PointF pointF = GdiUtil.Transform(transform, endPoint);
		TNode node = ((fromNode != null) ? fromNode : toNode);
		CircleF boundary = GetBoundary(node);
		Vec2F projection = default(Vec2F);
		if (CircleF.Project(new Vec2F(pointF), boundary, ref projection))
		{
			PointF pointF2 = new PointF(projection.X, projection.Y);
			g.DrawLine(m_theme.OutlinePen, pointF2, pointF);
			if (fromNode == null)
			{
				PointF pointF3 = pointF;
				pointF = pointF2;
				pointF2 = pointF3;
			}
			Vec2F d = new Vec2F(pointF.X - pointF2.X, pointF.Y - pointF2.Y);
			Vec2F p = new Vec2F(pointF);
			DrawArrow(p, d, m_theme.OutlinePen, g);
			if (!string.IsNullOrEmpty(label))
			{
				PointF pointF4 = new PointF((pointF.X + pointF2.X) * 0.5f, (pointF.Y + pointF2.Y) * 0.5f);
				g.DrawString(layoutRectangle: new RectangleF(pointF4.X - 512f, pointF4.Y, 1024f, m_theme.Font.Height), s: label, font: m_theme.Font, brush: m_theme.TextBrush, format: m_theme.CenterStringFormat);
			}
		}
	}

	public override Rectangle GetBounds(TNode node, Graphics g)
	{
		return GdiUtil.Transform(g.Transform, node.Bounds);
	}

	public override GraphHitRecord<TNode, TEdge, NumberedRoute> Pick(IGraph<TNode, TEdge, NumberedRoute> graph, TEdge priorityEdge, Point p, Graphics g)
	{
		TNode val = null;
		TEdge val2 = null;
		NumberedRoute fromRoute = null;
		NumberedRoute toRoute = null;
		Vec2F p2 = new Vec2F(p.X, p.Y);
		if (priorityEdge != null && Pick(priorityEdge, p2))
		{
			val2 = priorityEdge;
		}
		else
		{
			foreach (TEdge item in graph.Edges.Reverse())
			{
				if (Pick(item, p2))
				{
					val2 = item;
					break;
				}
			}
		}
		foreach (TNode item2 in graph.Nodes.Reverse())
		{
			if (!Pick(item2, p))
			{
				continue;
			}
			val = item2;
			CircleF boundary = GetBoundary(item2);
			boundary.Radius -= m_theme.PickTolerance;
			bool flag = !boundary.Contains(p2);
			if (val2 == null)
			{
				if (flag)
				{
					fromRoute = new NumberedRoute();
					toRoute = new NumberedRoute();
				}
			}
			else if (flag)
			{
				if (val2.FromNode == val)
				{
					fromRoute = new NumberedRoute();
				}
				else if (val2.ToNode == val)
				{
					toRoute = new NumberedRoute();
				}
			}
			break;
		}
		GraphHitRecord<TNode, TEdge, NumberedRoute> result = new GraphHitRecord<TNode, TEdge, NumberedRoute>(val, val2, fromRoute, toRoute);
		if (val != null && val2 == null)
		{
			Rectangle bounds = GetBounds(val, g);
			int num = bounds.Height - m_theme.Font.Height;
			bounds = new Rectangle(bounds.X, bounds.Y + num / 2, bounds.Width, bounds.Height - num);
			if (bounds.Contains(p))
			{
				DiagramLabel part = new DiagramLabel(bounds, TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine);
				return new GraphHitRecord<TNode, TEdge, NumberedRoute>(val, part);
			}
		}
		else if (val2 != null)
		{
			Rectangle labelBounds = GetLabelBounds(val2);
			DiagramLabel part2 = new DiagramLabel(labelBounds, TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine);
			if (labelBounds.Contains(p))
			{
				return new GraphHitRecord<TNode, TEdge, NumberedRoute>(val2, part2);
			}
		}
		return result;
	}

	private void theme_Redraw(object sender, EventArgs e)
	{
		OnRedraw();
	}

	private void Draw(TNode node, Graphics g)
	{
		RectangleF rectangleF = node.Bounds;
		if (g.ClipBounds.IntersectsWith(rectangleF))
		{
			using (LinearGradientBrush brush = new LinearGradientBrush(rectangleF, Color.White, Color.LightSteelBlue, LinearGradientMode.ForwardDiagonal))
			{
				g.FillEllipse(brush, rectangleF);
				g.DrawEllipse(m_theme.OutlinePen, rectangleF);
			}
			g.DrawString(node.Name, m_theme.Font, m_theme.TextBrush, rectangleF, m_theme.CenterStringFormat);
		}
	}

	private void DrawGhost(TNode node, Graphics g)
	{
		RectangleF rect = node.Bounds;
		if (g.ClipBounds.IntersectsWith(rect))
		{
			g.FillEllipse(Brushes.White, rect);
			g.DrawEllipse(m_theme.GhostPen, rect);
		}
	}

	private void DrawOutline(TNode node, Pen pen, Graphics g)
	{
		g.DrawEllipse(pen, node.Bounds);
	}

	private bool Pick(TNode node, Point p)
	{
		CircleF boundary = GetBoundary(node);
		boundary.Radius += m_theme.PickTolerance;
		return boundary.Contains(new Vec2F(p.X, p.Y));
	}

	private bool Pick(TEdge edge, Vec2F p)
	{
		bool result = false;
		Vec2F projection = default(Vec2F);
		Vec2F startPoint = default(Vec2F);
		Vec2F endPoint = default(Vec2F);
		CircleF circle = default(CircleF);
		bool moreThan = false;
		int index = edge.FromRoute.Index;
		if (GetEdgeGeometry(edge, index, ref startPoint, ref endPoint, ref circle, ref moreThan))
		{
			Seg2F seg = new Seg2F(startPoint, endPoint);
			projection = Seg2F.Project(seg, p);
			if (Vec2F.Distance(projection, p) < (float)m_theme.PickTolerance)
			{
				result = true;
			}
		}
		else if (CircleF.Project(p, circle, ref projection) && Vec2F.Distance(projection, p) < (float)m_theme.PickTolerance)
		{
			Vec2F v = endPoint - startPoint;
			Vec2F u = circle.Center - startPoint;
			Vec2F u2 = p - startPoint;
			float num = Vec2F.PerpDot(u, v);
			float num2 = Vec2F.PerpDot(u2, v);
			bool flag = num * num2 < 0f;
			result = moreThan ^ flag;
		}
		return result;
	}

	private bool GetEdgeGeometry(TEdge edge, int route, ref Vec2F startPoint, ref Vec2F endPoint, ref CircleF circle, ref bool moreThan180)
	{
		bool flag = false;
		if (edge.FromNode == edge.ToNode)
		{
			CircleF c = (circle = GetBoundary(edge.FromNode));
			circle.Center.X -= circle.Radius;
			circle.Radius *= 0.85f;
			float num = route * m_routeOffset / 2;
			circle.Center.X -= num;
			circle.Radius += num;
			CircleF.Intersect(circle, c, ref startPoint, ref endPoint);
			moreThan180 = true;
		}
		else
		{
			CircleF boundary = GetBoundary(edge.FromNode);
			CircleF boundary2 = GetBoundary(edge.ToNode);
			Vec2F vec2F = Vec2F.Sub(boundary2.Center, boundary.Center);
			float length = vec2F.Length;
			if (length < boundary.Radius + boundary2.Radius)
			{
				vec2F = Vec2F.XAxis;
				flag = true;
			}
			else if (route == 0)
			{
				vec2F *= 1f / length;
				flag = true;
			}
			else
			{
				Vec2F p = boundary.Center + vec2F * 0.5f;
				float length2 = vec2F.Length;
				Vec2F vec2F2 = vec2F.Perp * (1f / length2);
				p -= vec2F2 * m_routeOffset * route;
				circle = new CircleF(boundary.Center, p, boundary2.Center);
				Vec2F p2 = default(Vec2F);
				CircleF.Intersect(circle, boundary, ref startPoint, ref p2);
				CircleF.Intersect(circle, boundary2, ref p2, ref endPoint);
				Vec2F v = startPoint - endPoint;
				Vec2F u = circle.Center - endPoint;
				moreThan180 = Vec2F.PerpDot(u, v) < 0f;
			}
			if (flag)
			{
				startPoint = Vec2F.Add(boundary.Center, Vec2F.Mul(vec2F, boundary.Radius));
				endPoint = Vec2F.Sub(boundary2.Center, Vec2F.Mul(vec2F, boundary2.Radius));
			}
		}
		return flag;
	}

	private void Draw(TEdge edge, Pen pen, Graphics g)
	{
		Vec2F startPoint = default(Vec2F);
		Vec2F endPoint = default(Vec2F);
		CircleF circle = default(CircleF);
		bool moreThan = false;
		int index = edge.FromRoute.Index;
		Vec2F d;
		Vec2F projection;
		if (GetEdgeGeometry(edge, index, ref startPoint, ref endPoint, ref circle, ref moreThan))
		{
			g.DrawLine(pen, new PointF(startPoint.X, startPoint.Y), new PointF(endPoint.X, endPoint.Y));
			d = endPoint - startPoint;
			projection = (endPoint + startPoint) * 0.5f;
		}
		else
		{
			RectangleF rect = new RectangleF(circle.Center.X - circle.Radius, circle.Center.Y - circle.Radius, 2f * circle.Radius, 2f * circle.Radius);
			double num = Math.Atan2(startPoint.Y - circle.Center.Y, startPoint.X - circle.Center.X);
			double num2 = Math.Atan2(endPoint.Y - circle.Center.Y, endPoint.X - circle.Center.X);
			if (num > num2)
			{
				double num3 = num;
				num = num2;
				num2 = num3;
			}
			double num4 = num;
			double num5 = num2 - num;
			if (moreThan)
			{
				if (num5 < Math.PI)
				{
					num5 = 0.0 - (Math.PI * 2.0 - num5);
				}
			}
			else if (num5 > Math.PI)
			{
				num5 = 0.0 - (Math.PI * 2.0 - num5);
			}
			num4 *= 180.0 / Math.PI;
			num5 *= 180.0 / Math.PI;
			g.DrawArc(pen, rect, (float)num4, (float)num5);
			d = (endPoint - circle.Center).Perp;
			projection = (endPoint + startPoint) * 0.5f;
			CircleF.Project(projection, circle, ref projection);
			if (moreThan)
			{
				projection -= 2f * (projection - circle.Center);
			}
		}
		DrawArrow(endPoint, d, pen, g);
		string label = edge.Label;
		if (!string.IsNullOrEmpty(label))
		{
			g.DrawString(layoutRectangle: new RectangleF(projection.X - 512f, projection.Y, 1024f, m_theme.Font.Height), s: label, font: m_theme.Font, brush: m_theme.TextBrush, format: m_theme.CenterStringFormat);
		}
	}

	private Rectangle GetLabelBounds(TEdge edge)
	{
		Vec2F startPoint = default(Vec2F);
		Vec2F endPoint = default(Vec2F);
		CircleF circle = default(CircleF);
		bool moreThan = false;
		int index = edge.FromRoute.Index;
		Vec2F projection;
		if (GetEdgeGeometry(edge, index, ref startPoint, ref endPoint, ref circle, ref moreThan))
		{
			projection = (endPoint + startPoint) * 0.5f;
		}
		else
		{
			projection = (endPoint + startPoint) * 0.5f;
			CircleF.Project(projection, circle, ref projection);
			if (moreThan)
			{
				projection -= 2f * (projection - circle.Center);
			}
		}
		float num = m_theme.Font.Height;
		float num2 = 32f;
		if (!string.IsNullOrEmpty(edge.Label))
		{
			SizeF sizeF = TextRenderer.MeasureText(edge.Label, m_theme.Font);
			num2 = sizeF.Width;
			num = sizeF.Height;
		}
		return new Rectangle((int)(projection.X - num2 * 0.5f), (int)projection.Y, (int)num2, (int)num);
	}

	private void DrawArrow(Vec2F p, Vec2F d, Pen pen, Graphics g)
	{
		d *= 1f / d.Length;
		PointF pt = new PointF(p.X, p.Y);
		PointF pt2 = new PointF((float)((double)p.X + ((double)d.X * -0.866 + (double)d.Y * 0.5) * (double)m_arrowSize), (float)((double)p.Y + ((double)d.X * -0.5 + (double)d.Y * -0.866) * (double)m_arrowSize));
		PointF pt3 = new PointF((float)((double)p.X + ((double)d.X * -0.866 + (double)d.Y * -0.5) * (double)m_arrowSize), (float)((double)p.Y + ((double)d.X * 0.5 + (double)d.Y * -0.866) * (double)m_arrowSize));
		g.DrawLine(pen, pt, pt2);
		g.DrawLine(pen, pt, pt3);
	}

	private CircleF GetBoundary(TNode node)
	{
		Rectangle bounds = node.Bounds;
		float num = bounds.Width / 2;
		Vec2F center = new Vec2F((float)bounds.X + num, (float)bounds.Y + num);
		return new CircleF(center, num);
	}
}
