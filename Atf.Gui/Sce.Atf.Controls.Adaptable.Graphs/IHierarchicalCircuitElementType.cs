using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface IHierarchicalCircuitElementType<TElement, TWire, TPin> : IHierarchicalGraphNode<TElement, TWire, TPin>, IGraphNode, ICircuitElementType where TElement : class, ICircuitElement where TWire : class, IGraphEdge<TElement, TPin> where TPin : class, ICircuitPin
{
	bool Expanded { get; set; }

	IEnumerable<TWire> Edges { get; }
}
