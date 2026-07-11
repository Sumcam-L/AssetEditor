using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface ICircuitGroupPin<out TElement> : ICircuitPin, IEdgeRoute where TElement : class, ICircuitElement
{
	TElement InternalElement { get; }

	int InternalPinIndex { get; }

	Rectangle Bounds { get; }

	CircuitGroupPinInfo Info { get; }
}
