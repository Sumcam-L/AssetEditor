using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface ICircuitGroupType<TElement, TWire, TPin> : IHierarchicalGraphNode<TElement, TWire, TPin>, IGraphNode, ICircuitElementType where TElement : class, IGraphNode where TWire : class, IGraphEdge<TElement, TPin> where TPin : class, IEdgeRoute
{
	bool Expanded { get; set; }

	bool AutoSize { get; set; }

	IEnumerable<TWire> SubEdges { get; }

	CircuitGroupInfo Info { get; }
}
