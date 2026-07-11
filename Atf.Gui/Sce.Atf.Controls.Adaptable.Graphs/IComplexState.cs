namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface IComplexState<TNode, TEdge> : IHierarchicalGraphNode<TNode, TEdge, BoundaryRoute>, IGraphNode where TNode : class, IState where TEdge : class, IGraphEdge<TNode, BoundaryRoute>
{
	string Text { get; }
}
