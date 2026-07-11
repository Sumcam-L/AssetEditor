using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface IEditableGraphContainer<in TNode, TEdge, in TEdgeRoute> : IEditableGraph<TNode, TEdge, TEdgeRoute> where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	bool CanMove(object newParent, IEnumerable<object> movingObjects);

	void Move(object newParent, IEnumerable<object> movingObjects);

	bool CanResize(object container, DiagramBorder borderPart);

	void Resize(object container, int newWidth, int newHeight);
}
