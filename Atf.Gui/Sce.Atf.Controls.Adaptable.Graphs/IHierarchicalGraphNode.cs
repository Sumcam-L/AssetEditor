using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface IHierarchicalGraphNode<out TNode, TEdge, TEdgeRoute> : IGraphNode where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	IEnumerable<TNode> SubNodes { get; }
}
