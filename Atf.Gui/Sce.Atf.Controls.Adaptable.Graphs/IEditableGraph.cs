namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface IEditableGraph<in TNode, TEdge, in TEdgeRoute> where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	bool CanConnect(TNode fromNode, TEdgeRoute fromRoute, TNode toNode, TEdgeRoute toRoute);

	TEdge Connect(TNode fromNode, TEdgeRoute fromRoute, TNode toNode, TEdgeRoute toRoute, TEdge existingEdge);

	bool CanDisconnect(TEdge edge);

	void Disconnect(TEdge edge);
}
