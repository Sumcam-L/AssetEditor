using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface ICircuitContainer
{
	IList<Element> Elements { get; }

	IList<Wire> Wires { get; }

	IList<Annotation> Annotations { get; }

	bool Expanded { get; set; }

	bool Dirty { get; set; }

	Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide);

	Pair<Element, ICircuitPin> FullyMatchPinTarget(PinTarget pinTarget, bool inputSide);

	void Update();
}
