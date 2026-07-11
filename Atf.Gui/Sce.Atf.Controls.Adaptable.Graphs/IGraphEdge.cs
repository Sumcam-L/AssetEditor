namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface IGraphEdge<out TNode> where TNode : class, IGraphNode
{
	TNode FromNode { get; }

	TNode ToNode { get; }

	string Label { get; }
}
public interface IGraphEdge<out TNode, out TEdgeRoute> : IGraphEdge<TNode> where TNode : class, IGraphNode where TEdgeRoute : class, IEdgeRoute
{
	TEdgeRoute FromRoute { get; }

	TEdgeRoute ToRoute { get; }
}
