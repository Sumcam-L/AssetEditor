using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface IGraphAdapter<TNode, TEdge, TEdgeRoute> where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	void SetStyle(object item, DiagramDrawingStyle style);

	void ResetStyle(object item);

	DiagramDrawingStyle GetStyle(object item);

	GraphHitRecord<TNode, TEdge, TEdgeRoute> Pick(Point p);
}
