using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface IGraph<out TNode, out TEdge, out TEdgeRoute> where TNode : class, IGraphNode where TEdge : class, IGraphEdge<TNode, TEdgeRoute> where TEdgeRoute : class, IEdgeRoute
{
	IEnumerable<TNode> Nodes { get; }

	IEnumerable<TEdge> Edges { get; }
}
