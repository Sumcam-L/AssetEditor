using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public interface IGraphNode
{
	string Name { get; }

	Rectangle Bounds { get; }
}
