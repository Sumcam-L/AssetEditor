using System.Collections.Generic;
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class GraphRenderer<TNode, TEdge, TEdgeRoute> : DiagramRenderer where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	public abstract void Draw(TNode node, DiagramDrawingStyle style, Graphics g);

	public abstract void Draw(TEdge edge, DiagramDrawingStyle style, Graphics g);

	public abstract void Draw(TNode fromNode, TEdgeRoute fromRoute, TNode toNode, TEdgeRoute toRoute, string label, Point endPoint, Graphics g);

	public void Draw(IGraph<TNode, TEdge, TEdgeRoute> graph, Selection<object> selection, Graphics g)
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

	public void Print(IGraph<TNode, TEdge, TEdgeRoute> graph, Graphics g)
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

	public abstract Rectangle GetBounds(TNode node, Graphics g);

	public Rectangle GetBounds(IEnumerable<TNode> nodes, Graphics g)
	{
		Rectangle rectangle = Rectangle.Empty;
		bool flag = true;
		foreach (TNode node in nodes)
		{
			if (flag)
			{
				rectangle = GetBounds(node, g);
				flag = false;
			}
			else
			{
				rectangle = Rectangle.Union(rectangle, GetBounds(node, g));
			}
		}
		return rectangle;
	}

	public abstract GraphHitRecord<TNode, TEdge, TEdgeRoute> Pick(IGraph<TNode, TEdge, TEdgeRoute> graph, TEdge priorityEdge, Point p, Graphics g);
}
