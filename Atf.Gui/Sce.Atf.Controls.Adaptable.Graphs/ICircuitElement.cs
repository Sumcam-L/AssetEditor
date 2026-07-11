namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface ICircuitElement : IGraphNode
{
	ICircuitElementType Type { get; }

	CircuitElementInfo ElementInfo { get; }
}
