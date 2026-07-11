using System.Collections.Generic;
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface ICircuitElementType
{
	string Name { get; }

	Size InteriorSize { get; }

	Image Image { get; }

	IList<ICircuitPin> Inputs { get; }

	IList<ICircuitPin> Outputs { get; }
}
