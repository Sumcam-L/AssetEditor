using System;
using System.Collections.Generic;
using System.Drawing;
using Sce.Atf.Adaptation;
using Sce.Atf.Direct2D;
using Sce.Atf.Rendering;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class D2dGraphRenderer<TNode, TEdge, TEdgeRoute> : DiagramRenderer where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	public enum EdgeRouteDrawMode
	{
		Normal,
		CanConnect,
		CannotConnect
	}

	public class RouteConnectingInfo
	{
		public IEditableGraph<TNode, TEdge, TEdgeRoute> EditableGraph;

		public TNode StartNode;

		public TEdgeRoute StartRoute;
	}

	private readonly Dictionary<object, DiagramDrawingStyle> m_customStyles = new Dictionary<object, DiagramDrawingStyle>();

	public float EdgeThickness { get; set; }

	public float MinimumEdgeThickness { get; set; }

	public float MaximumEdgeThickness { get; set; }

	public RouteConnectingInfo RouteConnecting { get; set; }

	public abstract void Draw(TNode node, DiagramDrawingStyle style, D2dGraphics g);

	public abstract void Draw(TEdge edge, DiagramDrawingStyle style, D2dGraphics g);

	public abstract void Draw(TNode fromNode, TEdgeRoute fromRoute, TNode toNode, TEdgeRoute toRoute, string label, Point endPoint, D2dGraphics g);

	public virtual void DrawPartialEdge(TNode fromNode, TEdgeRoute fromRoute, TNode toNode, TEdgeRoute toRoute, string label, PointF startPoint, PointF endPoint, D2dGraphics g)
	{
		Draw(fromNode, fromRoute, toNode, toRoute, label, new Point((int)endPoint.X, (int)endPoint.Y), g);
	}

	public void Draw(IGraph<TNode, TEdge, TEdgeRoute> graph, Selection<object> selection, D2dGraphics g)
	{
		foreach (TNode node in graph.Nodes)
		{
			DiagramDrawingStyle style = DiagramDrawingStyle.Normal;
			if (selection != null && selection.Contains(node))
			{
				style = ((!selection.LastSelected.Equals(node)) ? DiagramDrawingStyle.Selected : DiagramDrawingStyle.LastSelected);
			}
			Draw(node, style, g);
		}
		foreach (TEdge edge in graph.Edges)
		{
			DiagramDrawingStyle style2 = DiagramDrawingStyle.Normal;
			if (selection != null && selection.Contains(edge))
			{
				style2 = ((!selection.LastSelected.Equals(edge)) ? DiagramDrawingStyle.Selected : DiagramDrawingStyle.LastSelected);
			}
			Draw(edge, style2, g);
		}
	}

	public void Print(IGraph<TNode, TEdge, TEdgeRoute> graph, D2dGraphics g)
	{
		try
		{
			base.IsPrinting = true;
			Draw(graph, null, g);
		}
		finally
		{
			base.IsPrinting = false;
		}
	}

	public virtual RectangleF GetBounds(TEdge edge, D2dGraphics g)
	{
		RectangleF bounds = GetBounds(edge.FromNode, g);
		return RectangleF.Union(bounds, GetBounds(edge.ToNode, g));
	}

	public virtual RectangleF GetBounds(TNode node, D2dGraphics g)
	{
		return node.Bounds;
	}

	public RectangleF GetBounds(IEnumerable<TNode> nodes, D2dGraphics g)
	{
		RectangleF rectangleF = RectangleF.Empty;
		bool flag = true;
		foreach (TNode node in nodes)
		{
			if (flag)
			{
				rectangleF = GetBounds(node, g);
				flag = false;
			}
			else
			{
				rectangleF = RectangleF.Union(rectangleF, GetBounds(node, g));
			}
		}
		return rectangleF;
	}

	public abstract GraphHitRecord<TNode, TEdge, TEdgeRoute> Pick(IGraph<TNode, TEdge, TEdgeRoute> graph, TEdge priorityEdge, PointF p, D2dGraphics g);

	public virtual GraphHitRecord<TNode, TEdge, TEdgeRoute> Pick(IEnumerable<TNode> nodes, IEnumerable<TEdge> edges, TEdge priorityEdge, PointF p, D2dGraphics g)
	{
		throw new NotImplementedException("Override in derived class");
	}

	public virtual IEnumerable<object> Pick(IGraph<TNode, TEdge, TEdgeRoute> graph, RectangleF rect, D2dGraphics g)
	{
		List<object> list = new List<object>();
		foreach (TNode node in graph.Nodes)
		{
			IVisible visible = node.As<IVisible>();
			if ((visible == null || visible.Visible) && GetBounds(node, g).IntersectsWith(rect))
			{
				list.Add(node);
			}
		}
		return list;
	}

	public virtual EdgeRouteDrawMode GetEdgeRouteDrawMode(TNode destinationNode, TEdgeRoute destination)
	{
		return EdgeRouteDrawMode.Normal;
	}

	public virtual void OnGraphObjectChanged(object sender, ItemChangedEventArgs<object> e)
	{
	}

	public virtual void OnGraphObjectRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
	}

	public virtual void OnGraphObjectInserted(object sender, ItemInsertedEventArgs<object> e)
	{
	}

	public void SetCustomStyle(object node, DiagramDrawingStyle style)
	{
		if (node != null)
		{
			m_customStyles[node] = style;
		}
	}

	public void ClearCustomStyle(object node)
	{
		if (node != null)
		{
			m_customStyles.Remove(node);
		}
	}

	public DiagramDrawingStyle GetCustomStyle(object node)
	{
		if (node != null && m_customStyles.TryGetValue(node, out var value))
		{
			return value;
		}
		return DiagramDrawingStyle.None;
	}
}
